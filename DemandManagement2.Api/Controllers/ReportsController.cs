using System.Text;
using ClosedXML.Excel;
using DemandManagement2.Api.Dtos;
using DemandManagement2.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DemandManagement2.Api.Controllers;

[Authorize(Roles = "Admin,Assessor")]
[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ReportsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReportRowDto>>> Get([FromQuery] ReportFilterDto filter)
    {
        var rows = await BuildQuery(filter).ToListAsync();
        return Ok(rows);
    }

    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] ReportFilterDto filter)
    {
        var rows = await BuildQuery(filter).ToListAsync();
        var sb = new StringBuilder();
        sb.AppendLine("Title,Type,Status,BusinessUnit,RequestedBy,Urgency,Effort,Score,InitialCost,AnnualBenefit,NPV,Approval,Created,TargetDate");

        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(",",
                Escape(r.Title), r.Type, r.Status, Escape(r.BusinessUnit), Escape(r.RequestedBy),
                r.Urgency, r.EstimatedEffort, r.WeightedScore?.ToString("F1") ?? "",
                r.InitialCost?.ToString("F2") ?? "", r.AnnualBenefit?.ToString("F2") ?? "",
                r.CalculatedNPV?.ToString("F2") ?? "", r.ApprovalStatus ?? "",
                r.CreatedAtUtc.ToString("yyyy-MM-dd"), r.TargetDate?.ToString("yyyy-MM-dd") ?? ""));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "demands-report.csv");
    }

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] ReportFilterDto filter)
    {
        var rows = await BuildQuery(filter).ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Demands Report");

        var headers = new[] { "Title", "Type", "Status", "Business Unit", "Requested By",
            "Urgency", "Effort", "Score", "Initial Cost", "Annual Benefit", "NPV",
            "Approval", "Created", "Target Date" };

        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1A1A1A");
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.FromHtml("#FFCD11");
        }

        for (int row = 0; row < rows.Count; row++)
        {
            var r = rows[row];
            ws.Cell(row + 2, 1).Value = r.Title;
            ws.Cell(row + 2, 2).Value = r.Type;
            ws.Cell(row + 2, 3).Value = r.Status;
            ws.Cell(row + 2, 4).Value = r.BusinessUnit;
            ws.Cell(row + 2, 5).Value = r.RequestedBy;
            ws.Cell(row + 2, 6).Value = r.Urgency;
            ws.Cell(row + 2, 7).Value = r.EstimatedEffort;
            ws.Cell(row + 2, 8).Value = r.WeightedScore ?? 0;
            ws.Cell(row + 2, 9).Value = r.InitialCost ?? 0;
            ws.Cell(row + 2, 10).Value = r.AnnualBenefit ?? 0;
            ws.Cell(row + 2, 11).Value = r.CalculatedNPV ?? 0;
            ws.Cell(row + 2, 12).Value = r.ApprovalStatus ?? "";
            ws.Cell(row + 2, 13).Value = r.CreatedAtUtc.ToString("yyyy-MM-dd");
            ws.Cell(row + 2, 14).Value = r.TargetDate?.ToString("yyyy-MM-dd") ?? "";
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "demands-report.xlsx");
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] ReportFilterDto filter)
    {
        var rows = await BuildQuery(filter).ToListAsync();

        var logoPath = Path.Combine(_env.ContentRootPath, "Assets", "barloworld-logo.png");
        byte[]? logoBytes = System.IO.File.Exists(logoPath)
            ? System.IO.File.ReadAllBytes(logoPath)
            : null;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().PaddingBottom(8).BorderBottom(2).BorderColor("#FFCD11").Row(row =>
                {
                    if (logoBytes != null)
                    {
                        row.ConstantItem(160).Image(logoBytes);
                        row.ConstantItem(16); // spacer
                    }

                    row.RelativeItem().AlignMiddle().Column(col =>
                    {
                        col.Item().Text("Demand Management Report")
                            .Bold().FontSize(18).FontColor("#1A1A1A");
                        col.Item().Text($"Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC")
                            .FontSize(8).FontColor("#555555");
                    });
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3); // Title
                        cols.RelativeColumn(1.2f); // Type
                        cols.RelativeColumn(1.2f); // Status
                        cols.RelativeColumn(1.5f); // BusinessUnit
                        cols.RelativeColumn(1.5f); // RequestedBy
                        cols.RelativeColumn(0.7f); // Urgency
                        cols.RelativeColumn(1); // Score
                        cols.RelativeColumn(1.2f); // NPV
                        cols.RelativeColumn(1); // Approval
                        cols.RelativeColumn(1.2f); // Created
                    });

                    var headerCols = new[] { "Title", "Type", "Status", "Business Unit",
                        "Requested By", "Urg", "Score", "NPV", "Approval", "Created" };

                    foreach (var h in headerCols)
                    {
                        table.Cell().Background("#1A1A1A").Padding(4)
                            .Text(h).FontColor("#FFCD11").Bold();
                    }

                    foreach (var r in rows)
                    {
                        var bg = rows.IndexOf(r) % 2 == 0 ? "#F5F5F5" : Colors.White;
                        table.Cell().Background(bg).Padding(4).Text(r.Title);
                        table.Cell().Background(bg).Padding(4).Text(r.Type);
                        table.Cell().Background(bg).Padding(4).Text(r.Status);
                        table.Cell().Background(bg).Padding(4).Text(r.BusinessUnit);
                        table.Cell().Background(bg).Padding(4).Text(r.RequestedBy);
                        table.Cell().Background(bg).Padding(4).Text(r.Urgency.ToString());
                        table.Cell().Background(bg).Padding(4).Text(r.WeightedScore?.ToString("F1") ?? "-");
                        table.Cell().Background(bg).Padding(4).Text(r.CalculatedNPV?.ToString("C0") ?? "-");
                        table.Cell().Background(bg).Padding(4).Text(r.ApprovalStatus ?? "-");
                        table.Cell().Background(bg).Padding(4).Text(r.CreatedAtUtc.ToString("yyyy-MM-dd"));
                    }
                });

                page.Footer().PaddingTop(6).BorderTop(2).BorderColor("#FFCD11").Row(row =>
                {
                    row.RelativeItem().Text("Barloworld Equipment | Confidential")
                        .FontSize(8).FontColor("#1A1A1A");
                    row.RelativeItem().AlignRight().Text(x =>
                    {
                        x.Span("Page ").FontSize(8).FontColor("#1A1A1A");
                        x.CurrentPageNumber().FontSize(8).FontColor("#1A1A1A");
                        x.Span(" of ").FontSize(8).FontColor("#1A1A1A");
                        x.TotalPages().FontSize(8).FontColor("#1A1A1A");
                    });
                });
            });
        });

        var pdfBytes = document.GeneratePdf();
        return File(pdfBytes, "application/pdf", "demands-report.pdf");
    }

    private IQueryable<ReportRowDto> BuildQuery(ReportFilterDto filter)
    {
        var query = _db.DemandRequests
            .Include(d => d.Assessment)
            .Include(d => d.Approval)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(d => d.Status.ToString() == filter.Status);
        if (!string.IsNullOrEmpty(filter.Type))
            query = query.Where(d => d.Type.ToString() == filter.Type);
        if (!string.IsNullOrEmpty(filter.BusinessUnit))
            query = query.Where(d => d.BusinessUnit.Contains(filter.BusinessUnit));
        if (filter.FromDate.HasValue)
            query = query.Where(d => d.CreatedAtUtc >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(d => d.CreatedAtUtc <= filter.ToDate.Value);

        return query.OrderByDescending(d => d.CreatedAtUtc)
            .Select(d => new ReportRowDto(
                d.Id,
                d.Title,
                d.Type.ToString(),
                d.Status.ToString(),
                d.BusinessUnit,
                d.RequestedBy,
                d.Urgency,
                d.EstimatedEffort,
                d.Assessment != null ? d.Assessment.WeightedScore : null,
                d.Assessment != null ? d.Assessment.InitialCost : null,
                d.Assessment != null ? d.Assessment.AnnualBenefit : null,
                d.Assessment != null ? d.Assessment.CalculatedNPV : null,
                d.Approval != null ? d.Approval.Status.ToString() : null,
                d.CreatedAtUtc,
                d.TargetDate
            ));
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
