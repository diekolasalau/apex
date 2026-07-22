using StudyMgt.Components;
using StudyMgt.Data;
using StudyMgt.Data.Entities;
using StudyMgt.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

Microsoft.AspNetCore.Hosting.StaticWebAssets.StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<StudentOnboardingService>();
builder.Services.AddScoped<TutorOnboardingService>();
builder.Services.AddScoped<TutorPortalService>();
builder.Services.AddScoped<TutorPortalSession>();
builder.Services.AddScoped<CarerOnboardingService>();
builder.Services.AddScoped<CarerPortalService>();
builder.Services.AddScoped<CarerPortalSession>();
builder.Services.AddScoped<CentreAdminAccessService>();
builder.Services.AddScoped<CentreAdminSession>();
builder.Services.AddScoped<AppAuditLogService>();
builder.Services.AddScoped<CouncilRequestService>();
builder.Services.AddScoped<CouncilPortalAccessService>();
builder.Services.AddScoped<CouncilPortalSession>();
builder.Services.AddScoped<TutorTimesheetExportService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<StudyMgtDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 7247;
});

var app = builder.Build();

var configuredUrls = builder.Configuration["ASPNETCORE_URLS"]
    ?? builder.Configuration["urls"]
    ?? string.Empty;

var hasConfiguredHttpsEndpoint = configuredUrls
    .Split(';', StringSplitOptions.RemoveEmptyEntries)
    .Any(url => url.Trim().StartsWith("https://", StringComparison.OrdinalIgnoreCase));

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StudyMgtDbContext>();
    dbContext.Database.Migrate();

    await DemoDataSeeder.SeedSystemAccountsAsync(dbContext);

    if (builder.Configuration.GetValue<bool>("DemoData:SeedTutorTimesheetSample"))
    {
        await DemoDataSeeder.SeedTutorTimesheetSampleAsync(dbContext);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
if (!app.Environment.IsEnvironment("Testing") && hasConfiguredHttpsEndpoint)
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

var staticAssetsManifestPath = Path.Combine(AppContext.BaseDirectory, $"{app.Environment.ApplicationName}.staticwebassets.endpoints.json");
if (File.Exists(staticAssetsManifestPath))
{
    app.MapStaticAssets();
}
else
{
    app.UseStaticFiles();
}
app.MapGet("/api/db-roundtrip", async (StudyMgtDbContext dbContext) =>
{
    var row = new DbRoundTripLog
    {
        Message = "Round-trip test from endpoint"
    };

    dbContext.DbRoundTripLogs.Add(row);
    await dbContext.SaveChangesAsync();

    var totalRows = await dbContext.DbRoundTripLogs.CountAsync();

    return Results.Ok(new
    {
        row.Id,
        row.CreatedUtc,
        row.Message,
        totalRows
    });
});

app.MapGet("/api/timesheets/tutor/{tutorOnboardingId:int}/pdf", async (int tutorOnboardingId, TutorPortalService tutorPortal, TutorTimesheetExportService exportService) =>
{
    var timesheets = await tutorPortal.GetTutorMonthlyTimesheetsAsync(tutorOnboardingId);
    if (timesheets.Count == 0)
    {
        return Results.NotFound("No monthly timesheets are available for this tutor.");
    }

    var pdfBytes = exportService.CreatePdf($"Tutor Timesheet - {timesheets[0].TutorName}", timesheets);
    var fileName = exportService.BuildFileName("tutor-timesheets", timesheets[0].TutorName, "pdf");
    return Results.File(pdfBytes, "application/pdf", fileName);
});

app.MapGet("/api/timesheets/tutor/{tutorOnboardingId:int}/excel", async (int tutorOnboardingId, TutorPortalService tutorPortal, TutorTimesheetExportService exportService) =>
{
    var timesheets = await tutorPortal.GetTutorMonthlyTimesheetsAsync(tutorOnboardingId);
    if (timesheets.Count == 0)
    {
        return Results.NotFound("No monthly timesheets are available for this tutor.");
    }

    var excelBytes = exportService.CreateExcel($"Tutor Timesheet - {timesheets[0].TutorName}", timesheets);
    var fileName = exportService.BuildFileName("tutor-timesheets", timesheets[0].TutorName, "xlsx");
    return Results.File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
});

app.MapGet("/api/timesheets/tutor/{tutorOnboardingId:int}/{year:int}/{month:int}/pdf", async (int tutorOnboardingId, int year, int month, TutorPortalService tutorPortal, TutorTimesheetExportService exportService) =>
{
    var timesheet = await tutorPortal.GetTutorMonthlyTimesheetAsync(tutorOnboardingId, year, month);
    if (timesheet == null)
    {
        return Results.NotFound("No monthly timesheet was found for the requested tutor and month.");
    }

    var monthLabel = new DateTime(timesheet.Year, timesheet.Month, 1).ToString("MMM yyyy");
    var pdfBytes = exportService.CreatePdf($"Tutor Timesheet - {timesheet.TutorName} - {monthLabel}", timesheet);
    var fileName = exportService.BuildFileName("tutor-timesheet", $"{timesheet.TutorName}-{monthLabel}", "pdf");
    return Results.File(pdfBytes, "application/pdf", fileName);
});

app.MapGet("/api/timesheets/tutor/{tutorOnboardingId:int}/{year:int}/{month:int}/excel", async (int tutorOnboardingId, int year, int month, TutorPortalService tutorPortal, TutorTimesheetExportService exportService) =>
{
    var timesheet = await tutorPortal.GetTutorMonthlyTimesheetAsync(tutorOnboardingId, year, month);
    if (timesheet == null)
    {
        return Results.NotFound("No monthly timesheet was found for the requested tutor and month.");
    }

    var monthLabel = new DateTime(timesheet.Year, timesheet.Month, 1).ToString("MMM yyyy");
    var excelBytes = exportService.CreateExcel($"Tutor Timesheet - {timesheet.TutorName} - {monthLabel}", timesheet);
    var fileName = exportService.BuildFileName("tutor-timesheet", $"{timesheet.TutorName}-{monthLabel}", "xlsx");
    return Results.File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
});

app.MapGet("/api/timesheets/all/pdf", async (TutorPortalService tutorPortal, TutorTimesheetExportService exportService) =>
{
    var timesheets = await tutorPortal.GetAllTutorMonthlyTimesheetsAsync(120);
    if (timesheets.Count == 0)
    {
        return Results.NotFound("No tutor monthly timesheets are available.");
    }

    var pdfBytes = exportService.CreatePdf("All Tutor Monthly Timesheets", timesheets);
    return Results.File(pdfBytes, "application/pdf", exportService.BuildFileName("all-tutor-timesheets", DateTime.UtcNow.ToString("yyyy-MM-dd"), "pdf"));
});

app.MapGet("/api/timesheets/all/excel", async (TutorPortalService tutorPortal, TutorTimesheetExportService exportService) =>
{
    var timesheets = await tutorPortal.GetAllTutorMonthlyTimesheetsAsync(120);
    if (timesheets.Count == 0)
    {
        return Results.NotFound("No tutor monthly timesheets are available.");
    }

    var excelBytes = exportService.CreateExcel("All Tutor Monthly Timesheets", timesheets);
    return Results.File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", exportService.BuildFileName("all-tutor-timesheets", DateTime.UtcNow.ToString("yyyy-MM-dd"), "xlsx"));
});

app.MapGet("/api/audit-logs/export/pdf", async (
    DateTime? fromUtc,
    DateTime? toUtc,
    string? actorRole,
    string? action,
    string? pagePath,
    bool? success,
    int? take,
    AppAuditLogService auditLogService) =>
{
    var logs = await auditLogService.GetLogsAsync(
        fromUtc: fromUtc,
        toUtc: toUtc,
        actorRole: actorRole,
        action: action,
        pagePath: pagePath,
        success: success,
        take: take ?? 200);

    if (logs.Count == 0)
    {
        return Results.NotFound("No audit logs are available for the selected filters.");
    }

    var pdfBytes = auditLogService.CreatePdf("Audit Logs Report", logs);
    return Results.File(pdfBytes, "application/pdf", auditLogService.BuildFileName("audit-logs-report", DateTime.UtcNow.ToString("yyyy-MM-dd"), "pdf"));
});

app.MapGet("/api/audit-logs/export/excel", async (
    DateTime? fromUtc,
    DateTime? toUtc,
    string? actorRole,
    string? action,
    string? pagePath,
    bool? success,
    int? take,
    AppAuditLogService auditLogService) =>
{
    var logs = await auditLogService.GetLogsAsync(
        fromUtc: fromUtc,
        toUtc: toUtc,
        actorRole: actorRole,
        action: action,
        pagePath: pagePath,
        success: success,
        take: take ?? 200);

    if (logs.Count == 0)
    {
        return Results.NotFound("No audit logs are available for the selected filters.");
    }

    var excelBytes = auditLogService.CreateExcel("Audit Logs Report", logs);
    return Results.File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", auditLogService.BuildFileName("audit-logs-report", DateTime.UtcNow.ToString("yyyy-MM-dd"), "xlsx"));
});

app.MapGet("/api/audit-logs/{id:long}/pdf", async (long id, AppAuditLogService auditLogService) =>
{
    var log = await auditLogService.GetLogByIdAsync(id);
    if (log is null)
    {
        return Results.NotFound("The requested audit log entry was not found.");
    }

    var pdfBytes = auditLogService.CreatePdf($"Audit Log Entry #{log.Id}", log);
    return Results.File(pdfBytes, "application/pdf", auditLogService.BuildFileName("audit-log", log.Id.ToString(), "pdf"));
});

app.MapGet("/api/audit-logs/{id:long}/excel", async (long id, AppAuditLogService auditLogService) =>
{
    var log = await auditLogService.GetLogByIdAsync(id);
    if (log is null)
    {
        return Results.NotFound("The requested audit log entry was not found.");
    }

    var excelBytes = auditLogService.CreateExcel($"Audit Log Entry #{log.Id}", log);
    return Results.File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", auditLogService.BuildFileName("audit-log", log.Id.ToString(), "xlsx"));
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
