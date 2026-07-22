using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StudyMgt.Data;

namespace StudyMgt.Services;

public class CouncilPortalSession
{
    public bool IsAuthenticated { get; private set; }
    public string? Username { get; private set; }
    public string? DisplayName { get; private set; }
    public string? Email { get; private set; }

    public void SignIn(string username, string displayName, string email)
    {
        IsAuthenticated = true;
        Username = username;
        DisplayName = displayName;
        Email = email;
    }

    public void SignOut()
    {
        IsAuthenticated = false;
        Username = null;
        DisplayName = null;
        Email = null;
    }
}

public class CouncilPortalAuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public int? RemainingLoginAttempts { get; set; }
    public bool IsLockedOut { get; set; }
    public int? LockoutRemainingMinutes { get; set; }
}

public class CouncilPortalAccessService
{
    private const int FailedAttemptLockoutThreshold = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly StudyMgtDbContext _dbContext;

    public CouncilPortalAccessService(StudyMgtDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CouncilPortalAuthResult> CreateAccountAsync(string email, string displayName, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return new CouncilPortalAuthResult
            {
                Success = false,
                Message = "Email, username, and password are required."
            };
        }

        if (password.Length < 8)
        {
            return new CouncilPortalAuthResult
            {
                Success = false,
                Message = "Password must be at least 8 characters."
            };
        }

        var normalizedEmail = email.Trim();
        var normalizedUsername = username.Trim();
        var normalizedDisplayName = string.IsNullOrWhiteSpace(displayName)
            ? normalizedUsername
            : displayName.Trim();

        var usernameTaken = await _dbContext.CouncilPortalAccounts.AnyAsync(x =>
            x.Username.ToLower() == normalizedUsername.ToLower());

        if (usernameTaken)
        {
            return new CouncilPortalAuthResult
            {
                Success = false,
                Message = "Username is already taken."
            };
        }

        var emailInUse = await _dbContext.CouncilPortalAccounts.AnyAsync(x =>
            x.Email.ToLower() == normalizedEmail.ToLower());

        if (emailInUse)
        {
            return new CouncilPortalAuthResult
            {
                Success = false,
                Message = "An account already exists for this email address."
            };
        }

        CreatePasswordHash(password, out var hash, out var salt);

        _dbContext.CouncilPortalAccounts.Add(new Data.Entities.CouncilPortalAccountEntity
        {
            Username = normalizedUsername,
            PasswordHash = hash,
            PasswordSalt = salt,
            Email = normalizedEmail,
            DisplayName = normalizedDisplayName,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();

        return new CouncilPortalAuthResult
        {
            Success = true,
            Message = "Council portal account created successfully.",
            Username = normalizedUsername,
            DisplayName = normalizedDisplayName,
            Email = normalizedEmail
        };
    }

    public async Task<CouncilPortalAuthResult> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return new CouncilPortalAuthResult
            {
                Success = false,
                Message = "Username and password are required."
            };
        }

        var normalizedUsername = username.Trim();
        var account = await _dbContext.CouncilPortalAccounts
            .FirstOrDefaultAsync(x => x.IsActive && x.Username.ToLower() == normalizedUsername.ToLower());

        if (account is null)
        {
            return new CouncilPortalAuthResult
            {
                Success = false,
                Message = "Invalid username or password."
            };
        }

        if (account.LockedUntilUtc.HasValue && account.LockedUntilUtc > DateTime.UtcNow)
        {
            var remainingMinutes = Math.Max(1, (int)Math.Ceiling((account.LockedUntilUtc.Value - DateTime.UtcNow).TotalMinutes));
            return new CouncilPortalAuthResult
            {
                Success = false,
                IsLockedOut = true,
                LockoutRemainingMinutes = remainingMinutes,
                RemainingLoginAttempts = 0,
                Message = $"Your account is temporarily locked due to repeated failed login attempts. Try again in {remainingMinutes} minute(s)."
            };
        }

        if (!VerifyPasswordHash(password, account.PasswordHash, account.PasswordSalt))
        {
            account.FailedLoginAttempts += 1;

            if (account.FailedLoginAttempts >= FailedAttemptLockoutThreshold)
            {
                account.LockedUntilUtc = DateTime.UtcNow.Add(LockoutDuration);
                await _dbContext.SaveChangesAsync();

                var lockoutMinutes = Math.Max(1, (int)Math.Ceiling(LockoutDuration.TotalMinutes));
                return new CouncilPortalAuthResult
                {
                    Success = false,
                    IsLockedOut = true,
                    LockoutRemainingMinutes = lockoutMinutes,
                    RemainingLoginAttempts = 0,
                    Message = $"Your account is temporarily locked due to repeated failed login attempts. Try again in {lockoutMinutes} minute(s)."
                };
            }

            await _dbContext.SaveChangesAsync();
            var remainingAttempts = Math.Max(0, FailedAttemptLockoutThreshold - account.FailedLoginAttempts);
            return new CouncilPortalAuthResult
            {
                Success = false,
                RemainingLoginAttempts = remainingAttempts,
                Message = $"Invalid username or password. {remainingAttempts} login attempt(s) remaining before temporary lockout."
            };
        }

        account.FailedLoginAttempts = 0;
        account.LockedUntilUtc = null;
        account.LastLoginAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return new CouncilPortalAuthResult
        {
            Success = true,
            Message = "Login successful.",
            Username = account.Username.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(account.DisplayName) ? account.Username.Trim() : account.DisplayName.Trim(),
            Email = account.Email.Trim()
        };
    }

    private static void CreatePasswordHash(string password, out string hash, out string salt)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            saltBytes,
            10_000,
            HashAlgorithmName.SHA256,
            32);

        salt = Convert.ToBase64String(saltBytes);
        hash = Convert.ToBase64String(hashBytes);
    }

    private static bool VerifyPasswordHash(string password, string hash, string salt)
    {
        try
        {
            var saltBytes = Convert.FromBase64String(salt);
            var hashBytes = Convert.FromBase64String(hash);
            var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                saltBytes,
                10_000,
                HashAlgorithmName.SHA256,
                32);

            return computedHash.SequenceEqual(hashBytes);
        }
        catch
        {
            return false;
        }
    }
}
