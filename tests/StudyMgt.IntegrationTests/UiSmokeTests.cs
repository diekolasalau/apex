using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Playwright;

namespace StudyMgt.IntegrationTests;

public class UiSmokeTests
{
    private const string AdminUsername = "admin";
    private const string AdminPassword = "Admin1234";

    [Fact]
    public async Task Key_Onboarding_Pages_Load()
    {
        if (!IsPlaywrightSmokeEnabled())
        {
            return;
        }

        await using var host = await TestAppHost.StartAsync();
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{host.BaseUrl}/student-onboarding");
        await ExpectVisibleTextAsync(page, "Centre Administrator Access");

        await page.GotoAsync($"{host.BaseUrl}/tutor-onboarding");
        await ExpectVisibleTextAsync(page, "Tutor Onboarding");

        await page.GotoAsync($"{host.BaseUrl}/carer-onboarding");
        await ExpectVisibleTextAsync(page, "Parent/Guardian Onboarding");
    }

    [Fact]
    public async Task Student_Submission_Appears_In_Admin_Review()
    {
        if (!IsPlaywrightSmokeEnabled())
        {
            return;
        }

        await using var host = await TestAppHost.StartAsync();
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        var page = await browser.NewPageAsync();
        var token = Guid.NewGuid().ToString("N")[..8];
        var firstName = $"Smoke{token}";
        var lastName = "Student";

        await page.GotoAsync($"{host.BaseUrl}/student-onboarding");
        await LoginAsAdminAsync(page);
        await page.GotoAsync($"{host.BaseUrl}/student-onboarding");

        await page.FillAsync("#firstName", firstName);
        await page.FillAsync("#lastName", lastName);
        await page.FillAsync("#dob", "2012-06-01");
        await page.SelectOptionAsync("#gender", "Female");
        await page.FillAsync("#studentId", $"SMK-{token}");
        await page.FillAsync("#emergencyContactName", "Smoke Parent");
        await page.FillAsync("#emergencyContactPhone", "07123456789");
        await page.FillAsync("#emergencyContactEmail", "smoke.parent@example.com");
        await page.SelectOptionAsync("#relationship", "Parent");
        await page.FillAsync("#senIndicators", "Dyslexia support required");
        await page.SelectOptionAsync("#ehcpStatus", "EHCP Active");
        await page.CheckAsync("#communicationSupport");
        await page.FillAsync("#safeguardingNotes", "No known concerns");
        await page.FillAsync("#medicalInfo", "None");
        await page.FillAsync("#riskAssessment", "Low risk");
        await page.CheckAsync("#privacyAcknowledged");
        await page.CheckAsync("#dataSharing");
        await page.CheckAsync("#emailCommunication");
        await page.CheckAsync("#smsCommunication");
        await page.SelectOptionAsync("#preferredContact", "Email");
        await page.CheckAsync("#declaration");

        await page.ClickAsync("button:has-text('Record Student Onboarding')");
        await page.WaitForURLAsync("**/centre-administrators*");

        await page.GotoAsync($"{host.BaseUrl}/admin-review");
        await ExpectVisibleTextAsync(page, firstName);
        await ExpectVisibleTextAsync(page, lastName);
    }

    private static async Task LoginAsAdminAsync(IPage page)
    {
        await page.Locator("input.form-control").First.FillAsync(AdminUsername);
        await page.Locator("input[type='password']").FillAsync(AdminPassword);
        await page.ClickAsync("button:has-text('Sign In')");
        await page.WaitForURLAsync("**/centre-administrators*");
    }

    private static async Task ExpectVisibleTextAsync(IPage page, string text)
    {
        var locator = page.Locator($"text={text}");
        await locator.First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });
    }

    private static bool IsPlaywrightSmokeEnabled()
    {
        return string.Equals(
            Environment.GetEnvironmentVariable("RUN_PLAYWRIGHT_SMOKE"),
            "1",
            StringComparison.OrdinalIgnoreCase);
    }

    private sealed class TestAppHost : IAsyncDisposable
    {
        private readonly Process _process;
        private readonly ConcurrentQueue<string> _logs = new();

        private TestAppHost(Process process, string baseUrl)
        {
            _process = process;
            BaseUrl = baseUrl;
            _process.OutputDataReceived += CaptureOutput;
            _process.ErrorDataReceived += CaptureOutput;
        }

        public string BaseUrl { get; }

        public static async Task<TestAppHost> StartAsync()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
            var port = GetFreeTcpPort();
            var baseUrl = $"http://127.0.0.1:{port}";

            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project StudyMgt.csproj --no-build -- --urls {baseUrl}",
                WorkingDirectory = repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            psi.Environment["ASPNETCORE_ENVIRONMENT"] = "Testing";

            var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
            if (!process.Start())
            {
                throw new InvalidOperationException("Failed to start StudyMgt host process.");
            }

            var host = new TestAppHost(process, baseUrl);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await host.WaitForReadyAsync();
            return host;
        }

        public ValueTask DisposeAsync()
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
            }

            _process.Dispose();
            return ValueTask.CompletedTask;
        }

        private async Task WaitForReadyAsync()
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                Timeout = TimeSpan.FromSeconds(5)
            };

            while (!timeout.IsCancellationRequested)
            {
                if (_process.HasExited)
                {
                    throw new InvalidOperationException($"StudyMgt host exited with code {_process.ExitCode}.{Environment.NewLine}{GetRecentLogs()}");
                }

                try
                {
                    var response = await client.GetAsync(BaseUrl, timeout.Token);
                    if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 400)
                    {
                        return;
                    }
                }
                catch
                {
                    // Retry until timeout.
                }

                try
                {
                    await Task.Delay(500, timeout.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            throw new TimeoutException($"Timed out waiting for StudyMgt host at {BaseUrl}.{Environment.NewLine}{GetRecentLogs()}");
        }

        private void CaptureOutput(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                _logs.Enqueue(e.Data);
            }
        }

        private string GetRecentLogs()
        {
            var lines = _logs.ToArray();
            if (lines.Length == 0)
            {
                return "No process logs captured.";
            }

            var tail = lines.Skip(Math.Max(0, lines.Length - 30));
            return "Recent process output:" + Environment.NewLine + string.Join(Environment.NewLine, tail);
        }

        private static int GetFreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
