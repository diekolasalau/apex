using StudyMgt.Data;
using StudyMgt.Data.Entities;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace StudyMgt.Services;

public class AppAuditLogService
{
    private readonly IServiceScopeFactory _scopeFactory;

    static AppAuditLogService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public AppAuditLogService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task LogAsync(
        string eventType,
        string action,
        string? pagePath = null,
        string? actorRole = null,
        string? actorUsername = null,
        string? entityType = null,
        string? entityId = null,
        bool success = true,
        string? details = null)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<StudyMgtDbContext>();

            db.ApplicationAuditLogs.Add(new ApplicationAuditLogEntity
            {
                OccurredAtUtc = DateTime.UtcNow,
                EventType = string.IsNullOrWhiteSpace(eventType) ? "Action" : eventType,
                Action = action,
                PagePath = pagePath,
                ActorRole = actorRole,
                ActorUsername = actorUsername,
                EntityType = entityType,
                EntityId = entityId,
                Success = success,
                Details = details
            });

            await db.SaveChangesAsync();
        }
        catch
        {
            // Auditing should never block end-user workflows.
        }
    }

    public async Task<List<ApplicationAuditLogEntity>> GetLogsAsync(
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        string? actorRole = null,
        string? action = null,
        string? pagePath = null,
        bool? success = null,
        int take = 200)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StudyMgtDbContext>();

        var query = db.ApplicationAuditLogs.AsNoTracking().AsQueryable();

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.OccurredAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.OccurredAtUtc <= toUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(actorRole))
        {
            query = query.Where(x => x.ActorRole == actorRole);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(x => x.Action.Contains(action));
        }

        if (!string.IsNullOrWhiteSpace(pagePath))
        {
            query = query.Where(x => x.PagePath != null && x.PagePath.Contains(pagePath));
        }

        if (success.HasValue)
        {
            query = query.Where(x => x.Success == success.Value);
        }

        var safeTake = take < 1 ? 1 : Math.Min(take, 1000);

        return await query
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(safeTake)
            .ToListAsync();
    }

    public async Task<ApplicationAuditLogEntity?> GetLogByIdAsync(long id)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StudyMgtDbContext>();

        return await db.ApplicationAuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public byte[] CreatePdf(string title, ApplicationAuditLogEntity log)
        => CreatePdf(title, new[] { log });

    public byte[] CreateExcel(string title, ApplicationAuditLogEntity log)
        => CreateExcel(title, new[] { log });

    public byte[] CreatePdf(string title, IReadOnlyList<ApplicationAuditLogEntity> logs)
    {
        using var stream = new MemoryStream();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(header =>
                {
                    header.Item().Text("Apex Education Services").SemiBold().FontSize(11).FontColor("#008A20");
                    header.Item().Text(title).SemiBold().FontSize(16).FontColor("#123042");
                    header.Item().Text($"Generated UTC: {DateTime.UtcNow:dd MMM yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.2f); // UTC
                        columns.RelativeColumn(1.0f); // Event
                        columns.RelativeColumn(1.3f); // Action
                        columns.RelativeColumn(0.9f); // Role
                        columns.RelativeColumn(1.0f); // User
                        columns.RelativeColumn(1.1f); // Page
                        columns.RelativeColumn(1.0f); // Entity
                        columns.RelativeColumn(0.6f); // Success
                        columns.RelativeColumn(2.1f); // Details
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#066AAB").Padding(4).Text("UTC Time").SemiBold().FontColor(Colors.White);
                        header.Cell().Background("#066AAB").Padding(4).Text("Event Type").SemiBold().FontColor(Colors.White);
                        header.Cell().Background("#066AAB").Padding(4).Text("Action").SemiBold().FontColor(Colors.White);
                        header.Cell().Background("#066AAB").Padding(4).Text("Role").SemiBold().FontColor(Colors.White);
                        header.Cell().Background("#066AAB").Padding(4).Text("User").SemiBold().FontColor(Colors.White);
                        header.Cell().Background("#066AAB").Padding(4).Text("Page").SemiBold().FontColor(Colors.White);
                        header.Cell().Background("#066AAB").Padding(4).Text("Entity").SemiBold().FontColor(Colors.White);
                        header.Cell().Background("#066AAB").Padding(4).Text("Success").SemiBold().FontColor(Colors.White);
                        header.Cell().Background("#066AAB").Padding(4).Text("Details").SemiBold().FontColor(Colors.White);
                    });

                    foreach (var log in logs)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(log.OccurredAtUtc.ToString("yyyy-MM-dd HH:mm:ss"));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(log.EventType);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(log.Action);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(log.ActorRole ?? "-");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(log.ActorUsername ?? "-");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(log.PagePath ?? "-");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(FormatEntity(log));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(log.Success ? "Yes" : "No");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(log.Details ?? "-");
                    }

                    if (logs.Count == 0)
                    {
                        table.Cell().ColumnSpan(9).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("No audit logs were found for the selected filters.");
                    }
                });

                page.Footer().AlignRight().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span("/");
                    text.TotalPages();
                });
            });
        }).GeneratePdf(stream);

        return stream.ToArray();
    }

    public byte[] CreateExcel(string title, IReadOnlyList<ApplicationAuditLogEntity> logs)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Audit Logs");

        worksheet.Cell(1, 1).Value = "Apex Education Services";
        worksheet.Cell(2, 1).Value = title;
        worksheet.Cell(3, 1).Value = $"Generated UTC: {DateTime.UtcNow:dd MMM yyyy HH:mm}";
        worksheet.Range(1, 1, 1, 9).Merge().Style.Font.Bold = true;
        worksheet.Range(2, 1, 2, 9).Merge().Style.Font.Bold = true;
        worksheet.Range(2, 1, 2, 9).Style.Font.FontSize = 14;
        worksheet.Range(3, 1, 3, 9).Merge().Style.Font.FontColor = XLColor.Gray;

        const int headerRow = 5;
        worksheet.Cell(headerRow, 1).Value = "UTC Time";
        worksheet.Cell(headerRow, 2).Value = "Event Type";
        worksheet.Cell(headerRow, 3).Value = "Action";
        worksheet.Cell(headerRow, 4).Value = "Role";
        worksheet.Cell(headerRow, 5).Value = "User";
        worksheet.Cell(headerRow, 6).Value = "Page";
        worksheet.Cell(headerRow, 7).Value = "Entity";
        worksheet.Cell(headerRow, 8).Value = "Success";
        worksheet.Cell(headerRow, 9).Value = "Details";

        var headerRange = worksheet.Range(headerRow, 1, headerRow, 9);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#066AAB");
        headerRange.Style.Font.FontColor = XLColor.White;

        var row = headerRow + 1;
        foreach (var log in logs)
        {
            worksheet.Cell(row, 1).Value = log.OccurredAtUtc.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cell(row, 2).Value = log.EventType;
            worksheet.Cell(row, 3).Value = log.Action;
            worksheet.Cell(row, 4).Value = log.ActorRole ?? "-";
            worksheet.Cell(row, 5).Value = log.ActorUsername ?? "-";
            worksheet.Cell(row, 6).Value = log.PagePath ?? "-";
            worksheet.Cell(row, 7).Value = FormatEntity(log);
            worksheet.Cell(row, 8).Value = log.Success ? "Yes" : "No";
            worksheet.Cell(row, 9).Value = log.Details ?? "-";
            row++;
        }

        if (row > headerRow + 1)
        {
            worksheet.Range(headerRow + 1, 1, row - 1, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range(headerRow + 1, 1, row - 1, 9).Style.Border.InsideBorder = XLBorderStyleValues.Hair;
        }

        worksheet.Column(1).Width = 21;
        worksheet.Column(2).Width = 14;
        worksheet.Column(3).Width = 28;
        worksheet.Column(4).Width = 14;
        worksheet.Column(5).Width = 16;
        worksheet.Column(6).Width = 26;
        worksheet.Column(7).Width = 20;
        worksheet.Column(8).Width = 10;
        worksheet.Column(9).Width = 45;
        worksheet.Columns().AdjustToContents(1, 8);
        worksheet.Column(9).Style.Alignment.WrapText = true;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public string BuildFileName(string prefix, string label, string extension)
    {
        var safeLabel = new string(label
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray())
            .Trim('-');

        return $"{prefix}-{safeLabel}-{DateTime.UtcNow:yyyyMMddHHmmss}.{extension}";
    }

    private static string FormatEntity(ApplicationAuditLogEntity log)
    {
        if (string.IsNullOrWhiteSpace(log.EntityType) && string.IsNullOrWhiteSpace(log.EntityId))
        {
            return "-";
        }

        return string.IsNullOrWhiteSpace(log.EntityType)
            ? log.EntityId ?? "-"
            : string.IsNullOrWhiteSpace(log.EntityId)
                ? log.EntityType
                : $"{log.EntityType}:{log.EntityId}";
    }

}
