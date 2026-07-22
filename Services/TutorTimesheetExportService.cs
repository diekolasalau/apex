using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace StudyMgt.Services;

public class TutorTimesheetExportService
{
    private static readonly string[] LogoCandidates =
    {
        Path.Combine("wwwroot", "images", "apex-logo.webp"),
        Path.Combine("wwwroot", "images", "apex-logo.png"),
        Path.Combine("wwwroot", "favicon.png")
    };

    private static readonly XLColor ApexBlue = XLColor.FromHtml("#066AAB");
    private static readonly XLColor ApexGreen = XLColor.FromHtml("#008A20");
    private static readonly XLColor ApexSoft = XLColor.FromHtml("#EDF6FB");

    static TutorTimesheetExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] CreatePdf(string title, TutorMonthlyTimesheetView timesheet)
        => CreatePdf(title, new[] { timesheet });

    public byte[] CreateExcel(string title, TutorMonthlyTimesheetView timesheet)
        => CreateExcel(title, new[] { timesheet });

    public byte[] CreatePdf(string title, IReadOnlyList<TutorMonthlyTimesheetView> timesheets)
    {
        if (timesheets.Count == 1)
        {
            return CreateTemplatePdf(timesheets[0]);
        }

        using var stream = new MemoryStream();
        var logoBytes = TryReadLogoBytes();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(header =>
                {
                    header.Item().Background("#066AAB").Height(6);

                    header.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Apex Education Services").SemiBold().FontSize(11).FontColor("#008A20");
                            col.Item().Text(title).SemiBold().FontSize(18).FontColor("#123042");
                            col.Item().Text($"Generated UTC: {DateTime.UtcNow:dd MMM yyyy HH:mm:ss}").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });

                        if (logoBytes is not null)
                        {
                            row.ConstantItem(120).Height(32).AlignRight().Image(logoBytes).FitArea();
                        }
                    });
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2.2f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.4f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#066AAB").Padding(5).Text("Tutor").SemiBold().FontColor(Colors.White);
                        header.Cell().Background("#066AAB").Padding(5).Text("Month").SemiBold().FontColor(Colors.White);
                        header.Cell().Background("#066AAB").Padding(5).Text("Sessions").SemiBold().FontColor(Colors.White);
                        header.Cell().Background("#066AAB").Padding(5).Text("Total Hours").SemiBold().FontColor(Colors.White);
                        header.Cell().Background("#066AAB").Padding(5).Text("Generated UTC").SemiBold().FontColor(Colors.White);
                    });

                    foreach (var item in timesheets)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.TutorName);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(new DateTime(item.Year, item.Month, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.SessionCount.ToString(CultureInfo.InvariantCulture));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.TotalHours.ToString("0.##", CultureInfo.InvariantCulture));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.GeneratedAtUtc.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture));
                    }
                });

                page.Footer().Column(footer =>
                {
                    footer.Item().Background("#EDF6FB").PaddingVertical(4).PaddingHorizontal(6).Row(row =>
                    {
                        row.RelativeItem().Text("Apex Education Services · Tutor Monthly Timesheet").FontSize(8).FontColor("#123042");
                        row.ConstantItem(80).AlignRight().Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span("/");
                            text.TotalPages();
                        });
                    });
                });
            });
        }).GeneratePdf(stream);

        return stream.ToArray();
    }

    private byte[] CreateTemplatePdf(TutorMonthlyTimesheetView timesheet)
    {
        using var stream = new MemoryStream();
        var logoBytes = TryReadLogoBytes();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(column =>
                {
                    column.Spacing(8);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Apex Education Services").FontSize(14).SemiBold().FontColor("#666666");

                        if (logoBytes is not null)
                        {
                            row.ConstantItem(140).Height(38).AlignRight().Image(logoBytes).FitArea();
                        }
                    });

                    column.Item().PaddingTop(4).AlignCenter().Text($"Tutoring Timesheet - {new DateTime(timesheet.Year, timesheet.Month, 1):MMMM yyyy}")
                        .FontSize(16)
                        .SemiBold()
                        .Underline();

                    column.Item().PaddingTop(14).Text($"Tutor Name: {timesheet.TutorName}").FontSize(12).SemiBold();

                    var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
                    var daysInMonth = DateTime.DaysInMonth(timesheet.Year, timesheet.Month);
                    var firstDate = new DateTime(timesheet.Year, timesheet.Month, 1);
                    var endDate = firstDate.AddDays(daysInMonth - 1);

                    var monthSpanText = $"{firstDate:dd MMM yyyy} - {endDate:dd MMM yyyy}";

                    column.Item().PaddingTop(12).Text($"Week 1 ({monthSpanText})").FontSize(11).SemiBold();

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.2f); // Date
                            columns.RelativeColumn(1.3f); // Day
                            columns.RelativeColumn(1.25f); // Student
                            columns.RelativeColumn(1.2f); // Hours per Day
                            columns.RelativeColumn(1.2f); // Rate
                            columns.RelativeColumn(1.2f); // Pay
                        });

                        var headerBg = "#F0F0F0";
                        var borderColor = "#222222";

                        table.Header(header =>
                        {
                            header.Cell().Border(1).BorderColor(borderColor).Background(headerBg).Padding(4).Text("Date").SemiBold();
                            header.Cell().Border(1).BorderColor(borderColor).Background(headerBg).Padding(4).Text("Day").SemiBold();
                            header.Cell().Border(1).BorderColor(borderColor).Background(headerBg).Padding(4).Text("Student").SemiBold();
                            header.Cell().Border(1).BorderColor(borderColor).Background(headerBg).Padding(4).Text("Hours per Day").SemiBold();
                            header.Cell().Border(1).BorderColor(borderColor).Background(headerBg).Padding(4).Text("Rate (GBP)").SemiBold();
                            header.Cell().Border(1).BorderColor(borderColor).Background(headerBg).Padding(4).Text("Pay (GBP) Rate * hours").SemiBold();
                        });

                        foreach (var day in days)
                        {
                            table.Cell().Border(1).BorderColor(borderColor).Padding(4).Height(24).Text(string.Empty);
                            table.Cell().Border(1).BorderColor(borderColor).Padding(4).Height(24).Text(day);
                            table.Cell().Border(1).BorderColor(borderColor).Padding(4).Height(24).Text(string.Empty);
                            table.Cell().Border(1).BorderColor(borderColor).Padding(4).Height(24).Text(string.Empty);
                            table.Cell().Border(1).BorderColor(borderColor).Padding(4).Height(24).Text(string.Empty);
                            table.Cell().Border(1).BorderColor(borderColor).Padding(4).Height(24).Text(string.Empty);
                        }

                        table.Cell().ColumnSpan(2).Border(1).BorderColor(borderColor).Padding(4).Text("Weekly Total:").SemiBold();
                        table.Cell().Border(1).BorderColor(borderColor).Padding(4).Text(string.Empty);
                        table.Cell().Border(1).BorderColor(borderColor).Padding(4).Text(string.Empty);
                        table.Cell().Border(1).BorderColor(borderColor).Padding(4).Text(string.Empty);
                        table.Cell().Border(1).BorderColor(borderColor).Padding(4).Text(string.Empty);
                    });

                    column.Item().PaddingTop(10).Text($"Monthly Sessions: {timesheet.SessionCount}   Monthly Hours: {timesheet.TotalHours:0.##}")
                        .FontSize(10)
                        .SemiBold();
                });
            });
        }).GeneratePdf(stream);

        return stream.ToArray();
    }

    public byte[] CreateExcel(string title, IReadOnlyList<TutorMonthlyTimesheetView> timesheets)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Timesheets");
        var logoPath = TryResolveLogoPath();

        if (!string.IsNullOrWhiteSpace(logoPath))
        {
            try
            {
                using var logoStream = File.OpenRead(logoPath);
                var picture = worksheet.AddPicture(logoStream)
                    .MoveTo(worksheet.Cell(1, 1))
                    .Scale(0.5);
                picture.Name = "ApexLogo";
            }
            catch
            {
                // Continue without image if the runtime image codec does not support the source format.
            }
        }

        worksheet.Cell(1, 3).Value = "Apex Education Services";
        worksheet.Range(1, 3, 1, 6).Merge().Style.Font.Bold = true;
        worksheet.Range(1, 3, 1, 6).Style.Font.FontColor = ApexGreen;
        worksheet.Range(1, 3, 1, 6).Style.Font.FontSize = 12;

        worksheet.Cell(2, 3).Value = title;
        worksheet.Cell(3, 3).Value = $"Generated UTC: {DateTime.UtcNow:dd MMM yyyy HH:mm}";
        worksheet.Range(2, 3, 2, 6).Merge().Style.Font.Bold = true;
        worksheet.Range(2, 3, 2, 6).Style.Font.FontSize = 14;
        worksheet.Range(3, 3, 3, 6).Merge().Style.Font.FontColor = XLColor.Gray;

        worksheet.Range(2, 1, 2, 6).Style.Fill.BackgroundColor = ApexSoft;
        worksheet.Range(2, 1, 2, 6).Style.Border.TopBorder = XLBorderStyleValues.Thick;
        worksheet.Range(2, 1, 2, 6).Style.Border.TopBorderColor = ApexBlue;

        const int headerRow = 5;
        worksheet.Cell(headerRow, 1).Value = "Tutor";
        worksheet.Cell(headerRow, 2).Value = "Username";
        worksheet.Cell(headerRow, 3).Value = "Month";
        worksheet.Cell(headerRow, 4).Value = "Sessions";
        worksheet.Cell(headerRow, 5).Value = "Total Hours";
        worksheet.Cell(headerRow, 6).Value = "Generated UTC";

        var headerRange = worksheet.Range(headerRow, 1, headerRow, 6);
        headerRange.Style.Font.Bold = true;
    headerRange.Style.Fill.BackgroundColor = ApexBlue;
    headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var row = headerRow + 1;
        foreach (var item in timesheets)
        {
            worksheet.Cell(row, 1).Value = item.TutorName;
            worksheet.Cell(row, 2).Value = item.Username;
            worksheet.Cell(row, 3).Value = new DateTime(item.Year, item.Month, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture);
            worksheet.Cell(row, 4).Value = item.SessionCount;
            worksheet.Cell(row, 5).Value = item.TotalHours;
            worksheet.Cell(row, 6).Value = item.GeneratedAtUtc.ToString("dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            row++;
        }

        if (row > headerRow + 1)
        {
            worksheet.Range(headerRow + 1, 1, row - 1, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range(headerRow + 1, 1, row - 1, 6).Style.Border.InsideBorder = XLBorderStyleValues.Hair;
        }

        worksheet.Columns().AdjustToContents();
        worksheet.Column(1).Width = Math.Max(worksheet.Column(1).Width, 22);
        worksheet.Column(2).Width = Math.Max(worksheet.Column(2).Width, 18);

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

    private static string? TryResolveLogoPath()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var candidates = new List<string>();

        foreach (var relative in LogoCandidates)
        {
            candidates.Add(Path.Combine(baseDirectory, relative));
            candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), relative));
        }

        var parent = Directory.GetParent(baseDirectory);
        for (var i = 0; i < 4 && parent is not null; i++)
        {
            foreach (var relative in LogoCandidates)
            {
                candidates.Add(Path.Combine(parent.FullName, relative));
            }

            parent = parent.Parent;
        }

        return candidates.FirstOrDefault(File.Exists);
    }

    private static byte[]? TryReadLogoBytes()
    {
        var logoPath = TryResolveLogoPath();
        if (string.IsNullOrWhiteSpace(logoPath))
        {
            return null;
        }

        try
        {
            return File.ReadAllBytes(logoPath);
        }
        catch
        {
            return null;
        }
    }
}