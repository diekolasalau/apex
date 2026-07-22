using StudyMgt.Data;
using StudyMgt.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace StudyMgt.Services;

public class CentreAdminSession
{
    public bool IsAuthenticated { get; private set; }
    public string? Username { get; private set; }
    public int? AdminId { get; private set; }
    public string? Email { get; private set; }
    public string? FullName { get; private set; }

    public bool CanOnboardStudents => IsAuthenticated;
    public bool CanOnboardTutors => IsAuthenticated;
    public bool CanOnboardCarers => IsAuthenticated;

    public void SignIn(string username, int adminId, string email, string fullName)
    {
        IsAuthenticated = true;
        Username = username;
        AdminId = adminId;
        Email = email;
        FullName = fullName;
    }

    public void SignOut()
    {
        IsAuthenticated = false;
        Username = null;
        AdminId = null;
        Email = null;
        FullName = null;
    }
}

public class CentreAdminAccessService
{
    private readonly StudyMgtDbContext _dbContext;
    private const int FailedAttemptLockoutThreshold = 5;
    private const int LockoutDurationMinutes = 15;
    private const int PasswordResetTokenExpiryMinutes = 60;

    public CentreAdminAccessService(StudyMgtDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Authenticate a centre administrator with username and password.
    /// </summary>
    public async Task<(bool success, string? message)> AuthenticateAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Username and password are required.");
        }

        var admin = await _dbContext.CentreAdminPortalAccounts
            .FirstOrDefaultAsync(a => a.Username.ToLower() == username.ToLower());

        if (admin == null)
        {
            return (false, "Invalid username or password.");
        }

        // Check if account is locked
        if (admin.LockedUntilUtc.HasValue && admin.LockedUntilUtc > DateTime.UtcNow)
        {
            return (false, "Account is temporarily locked due to multiple failed login attempts. Please try again later.");
        }

        // Reset failed attempts if lockout period has expired
        if (admin.LockedUntilUtc.HasValue && admin.LockedUntilUtc <= DateTime.UtcNow)
        {
            admin.FailedLoginAttempts = 0;
            admin.LockedUntilUtc = null;
        }

        // Verify password
        if (!VerifyPasswordHash(password, admin.PasswordHash, admin.PasswordSalt))
        {
            admin.FailedLoginAttempts++;

            // Lock account if threshold exceeded
            if (admin.FailedLoginAttempts >= FailedAttemptLockoutThreshold)
            {
                admin.LockedUntilUtc = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
            }

            await _dbContext.SaveChangesAsync();
            return (false, "Invalid username or password.");
        }

        // Successful login - reset failed attempts and update last login
        admin.FailedLoginAttempts = 0;
        admin.LockedUntilUtc = null;
        admin.LastLoginAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return (true, null);
    }

    /// <summary>
    /// Get a centre administrator by username.
    /// </summary>
    public async Task<CentreAdminPortalAccountEntity?> GetAdminByUsernameAsync(string username)
    {
        return await _dbContext.CentreAdminPortalAccounts
            .FirstOrDefaultAsync(a => a.Username.ToLower() == username.ToLower());
    }

    /// <summary>
    /// Generate a password reset token for an administrator.
    /// </summary>
    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        var admin = await _dbContext.CentreAdminPortalAccounts
            .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower() && a.IsActive);

        if (admin == null)
        {
            return null;
        }

        // Invalidate any existing unused tokens
        var existingTokens = await _dbContext.CentreAdminPasswordResetTokens
            .Where(t => t.CentreAdminPortalAccountId == admin.Id && !t.IsUsed)
            .ToListAsync();

        foreach (var token in existingTokens)
        {
            token.IsUsed = true;
            token.UsedAtUtc = DateTime.UtcNow;
        }

        // Generate new token
        var resetToken = GenerateSecureToken(32);
        var tokenEntity = new CentreAdminPasswordResetTokenEntity
        {
            CentreAdminPortalAccountId = admin.Id,
            ResetToken = HashToken(resetToken),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(PasswordResetTokenExpiryMinutes),
            IsUsed = false
        };

        _dbContext.CentreAdminPasswordResetTokens.Add(tokenEntity);
        await _dbContext.SaveChangesAsync();

        // Return unhashed token (to be sent in email)
        return resetToken;
    }

    /// <summary>
    /// Validate a password reset token.
    /// </summary>
    public async Task<CentreAdminPortalAccountEntity?> ValidateResetTokenAsync(string token)
    {
        var hashedToken = HashToken(token);
        var tokenEntity = await _dbContext.CentreAdminPasswordResetTokens
            .Include(t => t.CentreAdminAccount)
            .FirstOrDefaultAsync(t =>
                t.ResetToken == hashedToken &&
                !t.IsUsed &&
                t.ExpiresAtUtc > DateTime.UtcNow);

        return tokenEntity?.CentreAdminAccount;
    }

    /// <summary>
    /// Reset password using a valid reset token.
    /// </summary>
    public async Task<(bool success, string message)> ResetPasswordAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
        {
            return (false, "Password must be at least 8 characters long.");
        }

        var admin = await ValidateResetTokenAsync(token);
        if (admin == null)
        {
            return (false, "Invalid or expired password reset token.");
        }

        // Mark token as used
        var hashedToken = HashToken(token);
        var tokenEntity = await _dbContext.CentreAdminPasswordResetTokens
            .FirstOrDefaultAsync(t => t.ResetToken == hashedToken && !t.IsUsed);

        if (tokenEntity != null)
        {
            tokenEntity.IsUsed = true;
            tokenEntity.UsedAtUtc = DateTime.UtcNow;
        }

        // Update password
        var (passwordHash, salt) = HashPassword(newPassword);
        admin.PasswordHash = passwordHash;
        admin.PasswordSalt = salt;
        admin.LastPasswordChangedUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return (true, "Password reset successfully.");
    }

    /// <summary>
    /// Change password for an authenticated administrator.
    /// </summary>
    public async Task<(bool success, string message)> ChangePasswordAsync(int adminId, string currentPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
        {
            return (false, "Password must be at least 8 characters long.");
        }

        var admin = await _dbContext.CentreAdminPortalAccounts.FindAsync(adminId);
        if (admin == null)
        {
            return (false, "Administrator not found.");
        }

        if (!VerifyPasswordHash(currentPassword, admin.PasswordHash, admin.PasswordSalt))
        {
            return (false, "Current password is incorrect.");
        }

        var (passwordHash, salt) = HashPassword(newPassword);
        admin.PasswordHash = passwordHash;
        admin.PasswordSalt = salt;
        admin.LastPasswordChangedUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return (true, "Password changed successfully.");
    }

    /// <summary>
    /// Create another centre administrator account.
    /// </summary>
    public async Task<(bool success, string message)> CreateAdminAccountAsync(string username, string password, string email, string fullName)
    {
        var normalizedUsername = username?.Trim() ?? string.Empty;
        var normalizedEmail = email?.Trim() ?? string.Empty;
        var normalizedFullName = fullName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedUsername) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(normalizedEmail) ||
            string.IsNullOrWhiteSpace(normalizedFullName))
        {
            return (false, "Username, password, email, and full name are required.");
        }

        if (normalizedUsername.Length < 3)
        {
            return (false, "Username must be at least 3 characters long.");
        }

        if (password.Length < 8)
        {
            return (false, "Password must be at least 8 characters long.");
        }

        if (!normalizedEmail.Contains('@'))
        {
            return (false, "Provide a valid email address.");
        }

        var usernameExists = await _dbContext.CentreAdminPortalAccounts
            .AnyAsync(a => a.Username.ToLower() == normalizedUsername.ToLower());

        if (usernameExists)
        {
            return (false, "An administrator with this username already exists.");
        }

        var emailExists = await _dbContext.CentreAdminPortalAccounts
            .AnyAsync(a => a.Email.ToLower() == normalizedEmail.ToLower());

        if (emailExists)
        {
            return (false, "An administrator with this email already exists.");
        }

        var (passwordHash, passwordSalt) = HashPassword(password);

        _dbContext.CentreAdminPortalAccounts.Add(new CentreAdminPortalAccountEntity
        {
            Username = normalizedUsername,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Email = normalizedEmail,
            FullName = normalizedFullName,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            LastPasswordChangedUtc = DateTime.UtcNow,
            FailedLoginAttempts = 0,
            LockedUntilUtc = null
        });

        await _dbContext.SaveChangesAsync();
        return (true, "Administrator account created successfully.");
    }

    /// <summary>
    /// Hash a password using PBKDF2-SHA256.
    /// </summary>
    private (string hash, string salt) HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password: System.Text.Encoding.UTF8.GetBytes(password),
            salt: salt,
            iterations: 10000,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: 32);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    /// <summary>
    /// Verify a password against its hash.
    /// </summary>
    private bool VerifyPasswordHash(string password, string hash, string salt)
    {
        try
        {
            var saltBytes = Convert.FromBase64String(salt);
            var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                password: System.Text.Encoding.UTF8.GetBytes(password),
                salt: saltBytes,
                iterations: 10000,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: 32);
            
            var hashBytes = Convert.FromBase64String(hash);
            return computedHash.SequenceEqual(hashBytes);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generate a secure random token.
    /// </summary>
    private string GenerateSecureToken(int length)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(length);
        return Convert.ToBase64String(tokenBytes).Replace("/", "").Replace("+", "").Substring(0, Math.Min(length, Convert.ToBase64String(tokenBytes).Length));
    }

    /// <summary>
    /// Hash a reset token for storage.
    /// </summary>
    private string HashToken(string token)
    {
        using (var sha256 = SHA256.Create())
        {
            var tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(tokenBytes);
            return Convert.ToBase64String(hash);
        }
    }
}