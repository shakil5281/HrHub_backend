using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Drawing;

namespace ERPBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductionAssignmentController : ControllerBase
    {
        private readonly ProductionDbContext _context;

        public ProductionAssignmentController(ProductionDbContext context)
        {
            _context = context;
        }

        // --- Assignments ---

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductionAssignmentDto>>> GetAssignments()
        {
            return await _context.ProductionAssignments
                .Include(a => a.Production)
                .Include(a => a.Line)
                .Select(a => new ProductionAssignmentDto
                {
                    Id = a.Id,
                    ProductionId = a.ProductionId,
                    StyleNo = a.Production != null ? a.Production.StyleNo : "",
                    Buyer = a.Production != null ? a.Production.Buyer : "",
                    LineId = a.LineId,
                    LineName = a.Line != null ? a.Line.LineName : "",
                    TotalTarget = a.TotalTarget,
                    AssignDate = a.AssignDate,
                    Status = a.Status
                })
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<ProductionAssignmentDto>> CreateAssignment(CreateProductionAssignmentDto dto)
        {
            var assignment = new ProductionAssignment
            {
                ProductionId = dto.ProductionId,
                LineId = dto.LineId,
                TotalTarget = dto.TotalTarget,
                Status = dto.Status,
                AssignDate = DateTime.UtcNow
            };

            _context.ProductionAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Refresh to get related data
            var result = await _context.ProductionAssignments
                .Include(a => a.Production)
                .Include(a => a.Line)
                .FirstAsync(a => a.Id == assignment.Id);

            return Ok(new ProductionAssignmentDto
            {
                Id = result.Id,
                ProductionId = result.ProductionId,
                StyleNo = result.Production?.StyleNo ?? "",
                Buyer = result.Production?.Buyer ?? "",
                LineId = result.LineId,
                LineName = result.Line?.LineName ?? "",
                TotalTarget = result.TotalTarget,
                AssignDate = result.AssignDate,
                Status = result.Status
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAssignment(int id, UpdateProductionAssignmentDto dto)
        {
            var assignment = await _context.ProductionAssignments.FindAsync(id);
            if (assignment == null) return NotFound();

            assignment.ProductionId = dto.ProductionId;
            assignment.LineId = dto.LineId;
            assignment.TotalTarget = dto.TotalTarget;
            assignment.Status = dto.Status;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var assignment = await _context.ProductionAssignments.FindAsync(id);
            if (assignment == null) return NotFound();

            _context.ProductionAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Daily/Hourly Production ---

        [HttpGet("daily-record")]
        public async Task<ActionResult<DailyProductionRecordDto>> GetDailyRecord(int assignmentId, DateTime date)
        {
            var targetDate = date.Date;
            var record = await _context.DailyProductionRecords
                .FirstOrDefaultAsync(r => r.AssignmentId == assignmentId && r.Date.Date == targetDate);

            if (record == null)
            {
                // Check if a predefined target exists for this assignment and date
                var predefinedTarget = await _context.ProductionTargets
                    .FirstOrDefaultAsync(t => t.AssignmentId == assignmentId && t.TargetDate.Date == targetDate);

                // Return default values if record doesn't exist yet
                return Ok(new DailyProductionRecordDto
                {
                    AssignmentId = assignmentId,
                    Date = targetDate,
                    DailyTarget = predefinedTarget?.DailyTarget ?? 0,
                    HourlyTarget = predefinedTarget?.HourlyTarget ?? 0
                });
            }

            return Ok(new DailyProductionRecordDto
            {
                Id = record.Id,
                AssignmentId = record.AssignmentId,
                Date = record.Date,
                DailyTarget = record.DailyTarget,
                HourlyTarget = record.HourlyTarget,
                H1 = record.H1,
                H2 = record.H2,
                H3 = record.H3,
                H4 = record.H4,
                H5 = record.H5,
                H6 = record.H6,
                H7 = record.H7,
                H8 = record.H8,
                H9 = record.H9,
                H10 = record.H10,
                H11 = record.H11,
                H12 = record.H12,
                H13 = record.H13,
                H14 = record.H14,
                H15 = record.H15,
                H16 = record.H16,
                H17 = record.H17,
                H18 = record.H18,
                H19 = record.H19,
                TotalCompleted = record.TotalCompleted
            });
        }

        [HttpPost("daily-record")]
        public async Task<ActionResult<DailyProductionRecordDto>> SaveDailyRecord(SaveDailyProductionDto dto)
        {
            var targetDate = dto.Date.Date;
            var record = await _context.DailyProductionRecords
                .FirstOrDefaultAsync(r => r.AssignmentId == dto.AssignmentId && r.Date.Date == targetDate);

            if (record == null)
            {
                record = new DailyProductionRecord
                {
                    AssignmentId = dto.AssignmentId,
                    Date = targetDate
                };
                _context.DailyProductionRecords.Add(record);
            }

            record.DailyTarget = dto.DailyTarget;
            record.HourlyTarget = dto.HourlyTarget;
            record.H1 = dto.H1;
            record.H2 = dto.H2;
            record.H3 = dto.H3;
            record.H4 = dto.H4;
            record.H5 = dto.H5;
            record.H6 = dto.H6;
            record.H7 = dto.H7;
            record.H8 = dto.H8;
            record.H9 = dto.H9;
            record.H10 = dto.H10;
            record.H11 = dto.H11;
            record.H12 = dto.H12;
            record.H13 = dto.H13;
            record.H14 = dto.H14;
            record.H15 = dto.H15;
            record.H16 = dto.H16;
            record.H17 = dto.H17;
            record.H18 = dto.H18;
            record.H19 = dto.H19;

            await _context.SaveChangesAsync();

            return Ok(new DailyProductionRecordDto
            {
                Id = record.Id,
                AssignmentId = record.AssignmentId,
                Date = record.Date,
                DailyTarget = record.DailyTarget,
                HourlyTarget = record.HourlyTarget,
                H1 = record.H1,
                H2 = record.H2,
                H3 = record.H3,
                H4 = record.H4,
                H5 = record.H5,
                H6 = record.H6,
                H7 = record.H7,
                H8 = record.H8,
                H9 = record.H9,
                H10 = record.H10,
                H11 = record.H11,
                H12 = record.H12,
                H13 = record.H13,
                H14 = record.H14,
                H15 = record.H15,
                H16 = record.H16,
                H17 = record.H17,
                H18 = record.H18,
                H19 = record.H19,
                TotalCompleted = record.TotalCompleted
            });
        }

        [HttpDelete("daily-record")]
        public async Task<IActionResult> DeleteDailyRecord([FromQuery] int assignmentId, [FromQuery] DateTime date)
        {
            var targetDate = date.Date;
            var record = await _context.DailyProductionRecords
                .FirstOrDefaultAsync(r => r.AssignmentId == assignmentId && r.Date.Date == targetDate);

            if (record == null) return NotFound();

            _context.DailyProductionRecords.Remove(record);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("report/monthly")]
        public async Task<ActionResult<IEnumerable<MonthlyReportItemDto>>> GetMonthlyReport(int? year, int? month)
        {
            var targetYear = year ?? DateTime.Today.Year;
            var targetMonth = month ?? DateTime.Today.Month;

            var records = await _context.DailyProductionRecords
                .Include(r => r.Assignment)
                    .ThenInclude(a => a!.Line)
                .Include(r => r.Assignment)
                    .ThenInclude(a => a!.Production)
                .Where(r => r.Date.Year == targetYear && (month == null || r.Date.Month == targetMonth))
                .ToListAsync();

            var monthlyReport = records
                .GroupBy(r => new { r.Date.Year, r.Date.Month, LineName = r.Assignment?.Line?.LineName ?? "Unknown" })
                .Select(g => new MonthlyReportItemDto
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                    Year = g.Key.Year,
                    LineName = g.Key.LineName,
                    TotalTarget = g.Sum(r => r.DailyTarget),
                    TotalCompleted = g.Sum(r => r.H1 + r.H2 + r.H3 + r.H4 + r.H5 + r.H6 + r.H7 + r.H8 + r.H9 + r.H10 + r.H11 + r.H12 + r.H13 + r.H14 + r.H15 + r.H16 + r.H17 + r.H18 + r.H19),
                    AvgAchievement = g.Sum(r => r.DailyTarget) > 0 
                        ? (double)g.Sum(r => r.H1 + r.H2 + r.H3 + r.H4 + r.H5 + r.H6 + r.H7 + r.H8 + r.H9 + r.H10 + r.H11 + r.H12 + r.H13 + r.H14 + r.H15 + r.H16 + r.H17 + r.H18 + r.H19) / g.Sum(r => r.DailyTarget) * 100 
                        : 0,
                    WorkingDays = g.Count(),
                    TopStyle = g.Select(r => r.Assignment?.Production?.StyleNo ?? "")
                                .GroupBy(s => s)
                                .OrderByDescending(group => group.Count())
                                .Select(group => group.Key)
                                .FirstOrDefault() ?? ""
                })
                .OrderByDescending(r => r.Year)
                .ThenByDescending(r => new DateTime(2000, DateTime.ParseExact(r.Month, "MMMM", null).Month, 1))
                .ToList();

            return Ok(monthlyReport);
        }

        // --- Reporting ---

        [HttpGet("report/daily")]
        public async Task<ActionResult<IEnumerable<DailyReportItemDto>>> GetDailyReport([FromQuery] ProductionFilterDto filters)
        {
            var query = GetDailyReportQuery(filters);
            return await query.ToListAsync();
        }

        [HttpGet("report/daily/export/excel")]
        public async Task<IActionResult> ExportDailyReportToExcel([FromQuery] ProductionFilterDto filters)
        {
            return await ExportGroupedReportToExcel(filters);
        }

        [HttpGet("record/hourly/export/excel")]
        public async Task<IActionResult> ExportHourlyBreakdownToExcel([FromQuery] ProductionFilterDto filters)
        {
            try
            {
                var query = GetDailyReportQuery(filters);
                var data = await query.ToListAsync();

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Hourly Production Breakdown");
                    var date = filters.Date ?? DateTime.Today;

                    // UI Setup
                    worksheet.View.ShowGridLines = false;
                    worksheet.PrinterSettings.PaperSize = ePaperSize.A4;
                    worksheet.PrinterSettings.Orientation = eOrientation.Landscape;
                    worksheet.PrinterSettings.FitToPage = true;
                    worksheet.PrinterSettings.FitToWidth = 1;

                    var borderColor = System.Drawing.Color.Black;

                    // Header
                    worksheet.Cells["A1:AC1"].Merge = true;
                    worksheet.Cells["A1"].Value = "Hourly Production Breakdown Report";
                    worksheet.Cells["A1"].Style.Font.Size = 18;
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Row(1).Height = 35;

                    worksheet.Cells["A2:AC2"].Merge = true;
                    worksheet.Cells["A2"].Value = $"Date: {date:dd MMMM yyyy}";
                    worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Row(2).Height = 25;

                    // Table Headers
                    int row = 4;
                    worksheet.Row(row).Height = 35;
                    string[] headers = { 
                        "Line", "Buyer", "Style", "Item", "Daily Target", "Hourly Target", "Total Target",
                        "08-09", "09-10", "10-11", "11-12", "12-01", 
                        "02-03", "03-04", "04-05", "05-06", "06-07", "07-08",
                        "08-09", "09-10", "10-11", "11-12", "12-01", "01-02", "02-03",
                        "Total Production", "Average", "Remarks"
                    };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = worksheet.Cells[row, i + 1];
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(240, 240, 240));
                        cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        cell.Style.Border.Top.Color.SetColor(borderColor);
                        cell.Style.Border.Bottom.Color.SetColor(borderColor);
                        cell.Style.Border.Left.Color.SetColor(borderColor);
                        cell.Style.Border.Right.Color.SetColor(borderColor);
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cell.Style.WrapText = true;
                    }

                    int startDataRow = row + 1;
                    row++;

                    foreach (var item in data)
                    {
                        worksheet.Row(row).Height = 22;
                        worksheet.Cells[row, 1].Value = item.LineName;
                        worksheet.Cells[row, 2].Value = item.Buyer;
                        worksheet.Cells[row, 3].Value = item.StyleNo;
                        worksheet.Cells[row, 4].Value = item.Item;
                        worksheet.Cells[row, 5].Value = (double)item.DailyTarget;
                        worksheet.Cells[row, 6].Value = (double)item.HourlyTarget;
                        worksheet.Cells[row, 7].Value = (double)item.TotalAssignedTarget;

                        // Hours H1-H5
                        worksheet.Cells[row, 8].Value = (double)item.H1;
                        worksheet.Cells[row, 9].Value = (double)item.H2;
                        worksheet.Cells[row, 10].Value = (double)item.H3;
                        worksheet.Cells[row, 11].Value = (double)item.H4;
                        worksheet.Cells[row, 12].Value = (double)item.H5;

                        // Hours H7-H19 (Skipping H6)
                        worksheet.Cells[row, 13].Value = (double)item.H7;
                        worksheet.Cells[row, 14].Value = (double)item.H8;
                        worksheet.Cells[row, 15].Value = (double)item.H9;
                        worksheet.Cells[row, 16].Value = (double)item.H10;
                        worksheet.Cells[row, 17].Value = (double)item.H11;
                        worksheet.Cells[row, 18].Value = (double)item.H12;
                        worksheet.Cells[row, 19].Value = (double)item.H13;
                        worksheet.Cells[row, 20].Value = (double)item.H14;
                        worksheet.Cells[row, 21].Value = (double)item.H15;
                        worksheet.Cells[row, 22].Value = (double)item.H16;
                        worksheet.Cells[row, 23].Value = (double)item.H17;
                        worksheet.Cells[row, 24].Value = (double)item.H18;
                        worksheet.Cells[row, 25].Value = (double)item.H19;

                        worksheet.Cells[row, 26].Value = (double)item.Completed;
                        worksheet.Cells[row, 27].Value = (double)item.Completed / 18.0;
                        worksheet.Cells[row, 27].Style.Numberformat.Format = "0.0";

                        using (var range = worksheet.Cells[row, 1, row, 28])
                        {
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Top.Color.SetColor(borderColor);
                            range.Style.Border.Bottom.Color.SetColor(borderColor);
                            range.Style.Border.Left.Color.SetColor(borderColor);
                            range.Style.Border.Right.Color.SetColor(borderColor);
                        }
                        row++;
                    }

                    // Total Row
                    worksheet.Row(row).Height = 35;
                    worksheet.Cells[row, 1, row, 4].Merge = true;
                    worksheet.Cells[row, 1].Value = "Total";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    for (int c = 5; c <= 26; c++)
                    {
                        var cell = worksheet.Cells[row, c];
                        string colLetter = GetColumnLetter(c);
                        cell.Formula = $"SUM({colLetter}{startDataRow}:{colLetter}{row - 1})";
                        cell.Style.Font.Bold = true;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    using (var range = worksheet.Cells[row, 1, row, 28])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(230, 230, 230));
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Top.Color.SetColor(borderColor);
                        range.Style.Border.Bottom.Color.SetColor(borderColor);
                        range.Style.Border.Left.Color.SetColor(borderColor);
                        range.Style.Border.Right.Color.SetColor(borderColor);
                    }

                    worksheet.Cells.AutoFitColumns();
                    var content = package.GetAsByteArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"HourlyBreakdown_{date:yyyyMMdd}.xlsx");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Export failed", error = ex.Message });
            }
        }

        private string GetColumnLetter(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }



        private async Task<IActionResult> ExportGroupedReportToExcel(ProductionFilterDto filters)
        {
            try
            {
                var query = GetDailyReportQuery(filters);
                var data = await query.ToListAsync();

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Daily Production Report");
                    var date = filters.Date ?? DateTime.Today;

                    // UI Setup
                    worksheet.View.ShowGridLines = false;
                    worksheet.PrinterSettings.PaperSize = ePaperSize.A4;
                    worksheet.PrinterSettings.Orientation = eOrientation.Landscape;
                    worksheet.PrinterSettings.FitToPage = true;
                    worksheet.PrinterSettings.FitToWidth = 1;

                    var borderColor = System.Drawing.Color.Black;
                    var headerColor = System.Drawing.Color.FromArgb(0, 150, 100);

                    // Header
                    worksheet.Cells["A1:N1"].Merge = true;
                    worksheet.Cells["A1"].Value = "Daily Production Report";
                    worksheet.Cells["A1"].Style.Font.Size = 18;
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Row(1).Height = 35;

                    worksheet.Cells["A2:N2"].Merge = true;
                    worksheet.Cells["A2"].Value = $"Date: {date:dd MMMM yyyy}";
                    worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Row(2).Height = 25;

                    // Table Headers
                    int row = 4;
                    worksheet.Row(row).Height = 35;
                    string[] headers = { "LINE", "P/COD", "BUYER", "ART/NO", "OR/QTY", "ITEM", "DAILY TARGET", "DAILY PRODUCTION", "UNIT PRICE", "TOTAL PRICE", "%", "% Dollar", "Taka", "Remarks" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = worksheet.Cells[row, i + 1];
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.Color.SetColor(headerColor);
                    }

                    using (var range = worksheet.Cells[row, 1, row, 14])
                    {
                        range.Style.WrapText = true;
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Top.Color.SetColor(borderColor);
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Color.SetColor(borderColor);
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Color.SetColor(borderColor);
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Color.SetColor(borderColor);
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    int startDataRow = row + 1;
                    row++;
                    decimal exchangeRate = 120;

                    var groupedData = data.GroupBy(d => d.LineName).OrderBy(g => g.Key).ToList();

                    foreach (var group in groupedData)
                    {
                        int groupStartRow = row;
                        var items = group.ToList();
                        int totalGroupTarget = items.Sum(i => i.DailyTarget);
                        double averageAchievement = items.Any() ? items.Average(i => i.Achievement) : 0;

                        for (int i = 0; i < items.Count; i++)
                        {
                            var item = items[i];
                            decimal totalPrice = item.Completed * item.UnitPrice;
                            worksheet.Row(row).Height = 22;

                            worksheet.Cells[row, 1].Value = item.LineName;
                            worksheet.Cells[row, 2].Value = item.ProgramCode;
                            worksheet.Cells[row, 3].Value = item.Buyer;
                            worksheet.Cells[row, 4].Value = item.StyleNo;
                            worksheet.Cells[row, 5].Value = item.OrderQty;
                            worksheet.Cells[row, 6].Value = item.Item;
                            worksheet.Cells[row, 7].Value = item.DailyTarget;
                            worksheet.Cells[row, 8].Value = item.Completed;
                            worksheet.Cells[row, 9].Value = (double)item.UnitPrice;
                            worksheet.Cells[row, 10].Value = (double)totalPrice;
                            worksheet.Cells[row, 11].Value = (double)(item.Achievement / 100);
                            worksheet.Cells[row, 11].Style.Numberformat.Format = "0%";
                            decimal percentDollar = totalPrice * (decimal)(item.Achievement / 100);
                            worksheet.Cells[row, 12].Value = (double)percentDollar;
                            worksheet.Cells[row, 13].Value = (double)(totalPrice * exchangeRate);

                            worksheet.Cells[row, 9, row, 10].Style.Numberformat.Format = "#,##0.00";
                            worksheet.Cells[row, 12, row, 13].Style.Numberformat.Format = "#,##0.00";

                            using (var range = worksheet.Cells[row, 1, row, 14])
                            {
                                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                range.Style.WrapText = true;
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Top.Color.SetColor(borderColor);
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Color.SetColor(borderColor);
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Color.SetColor(borderColor);
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Color.SetColor(borderColor);
                            }
                            row++;
                        }

                        if (items.Count > 1)
                        {
                            worksheet.Cells[groupStartRow, 1, row - 1, 1].Merge = true;
                            worksheet.Cells[groupStartRow, 7, row - 1, 7].Merge = true;
                            worksheet.Cells[groupStartRow, 7].Value = totalGroupTarget;
                            worksheet.Cells[groupStartRow, 11, row - 1, 11].Merge = true;
                            worksheet.Cells[groupStartRow, 11].Value = averageAchievement / 100;
                        }
                    }

                    worksheet.Row(row).Height = 35;
                    worksheet.Cells[row, 1, row, 4].Merge = true;
                    worksheet.Cells[row, 1].Value = "Total";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    worksheet.Cells[row, 5].Formula = $"SUM(E{startDataRow}:E{row - 1})";
                    worksheet.Cells[row, 7].Formula = $"SUM(G{startDataRow}:G{row - 1})";
                    worksheet.Cells[row, 8].Formula = $"SUM(H{startDataRow}:H{row - 1})";
                    worksheet.Cells[row, 10].Formula = $"SUM(J{startDataRow}:J{row - 1})";
                    worksheet.Cells[row, 12].Formula = $"SUM(L{startDataRow}:L{row - 1})";
                    worksheet.Cells[row, 13].Formula = $"SUM(M{startDataRow}:M{row - 1})";

                    using (var range = worksheet.Cells[row, 1, row, 14])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(230, 230, 230));
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Top.Color.SetColor(borderColor);
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Color.SetColor(borderColor);
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Color.SetColor(borderColor);
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Color.SetColor(borderColor);
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }
                    worksheet.Cells[row, 10, row, 13].Style.Numberformat.Format = "#,##0";

                    row += 4;
                    worksheet.Cells[row, 1, row, 3].Merge = true;
                    worksheet.Cells[row, 1].Value = "__________________________";
                    worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row + 1, 1, row + 1, 3].Merge = true;
                    worksheet.Cells[row + 1, 1].Value = "Production manager";
                    worksheet.Cells[row + 1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row + 1, 1].Style.Font.Bold = true;

                    worksheet.Cells[row, 12, row, 14].Merge = true;
                    worksheet.Cells[row, 12].Value = "__________________________";
                    worksheet.Cells[row, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row + 1, 12, row + 1, 14].Merge = true;
                    worksheet.Cells[row + 1, 12].Value = "Asst. general manager";
                    worksheet.Cells[row + 1, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row + 1, 12].Style.Font.Bold = true;

                    worksheet.Cells.AutoFitColumns();
                    worksheet.Column(13).Width += 5;
                    worksheet.Column(8).Width = 15;

                    var content = package.GetAsByteArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DailyProductionReport_{date:yyyyMMdd}.xlsx");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Export failed", error = ex.Message });
            }
        }

        private IQueryable<DailyReportItemDto> GetDailyReportQuery(ProductionFilterDto filters)
        {
            var targetDate = (filters.Date ?? DateTime.Today).Date;
            
            // Start from all active assignments
            var query = _context.ProductionAssignments
                .Include(a => a.Production)
                .Include(a => a.Line)
                .Where(a => a.Status == "Active");

            if (filters.LineId.HasValue)
                query = query.Where(a => a.LineId == filters.LineId.Value);

            if (!string.IsNullOrEmpty(filters.Buyer))
                query = query.Where(a => a.Production!.Buyer.Contains(filters.Buyer));

            if (!string.IsNullOrEmpty(filters.StyleNo))
                query = query.Where(a => a.Production!.StyleNo.Contains(filters.StyleNo));

            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                var term = filters.SearchTerm.ToLower();
                query = query.Where(a => 
                    a.Line!.LineName.ToLower().Contains(term) ||
                    a.Production!.StyleNo.ToLower().Contains(term) ||
                    a.Production!.Buyer.ToLower().Contains(term));
            }

            return query.Select(a => new DailyReportItemDto
            {
                AssignmentId = a.Id,
                LineName = a.Line!.LineName,
                StyleNo = a.Production!.StyleNo,
                ProgramCode = a.Production!.ProgramCode,
                OrderQty = a.Production!.OrderQty,
                Item = a.Production!.Item,
                UnitPrice = a.Production!.UnitPrice,
                Buyer = a.Production!.Buyer,
                
                // Fetch target from record, or fallback to predefined target
                DailyTarget = _context.DailyProductionRecords
                    .Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate)
                    .Select(r => (int?)r.DailyTarget)
                    .FirstOrDefault() ?? _context.ProductionTargets
                        .Where(t => t.AssignmentId == a.Id && t.TargetDate.Date == targetDate)
                        .Select(t => (int?)t.DailyTarget)
                        .FirstOrDefault() ?? 0,

                HourlyTarget = _context.DailyProductionRecords
                    .Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate)
                    .Select(r => (int?)r.HourlyTarget)
                    .FirstOrDefault() ?? _context.ProductionTargets
                        .Where(t => t.AssignmentId == a.Id && t.TargetDate.Date == targetDate)
                        .Select(t => (int?)t.HourlyTarget)
                        .FirstOrDefault() ?? 0,

                TotalAssignedTarget = a.TotalTarget,

                Completed = _context.DailyProductionRecords
                    .Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate)
                    .Select(r => r.H1 + r.H2 + r.H3 + r.H4 + r.H5 + r.H7 + r.H8 + r.H9 + r.H10 + r.H11 + r.H12 + r.H13 + r.H14 + r.H15 + r.H16 + r.H17 + r.H18 + r.H19)
                    .FirstOrDefault(),

                Achievement = _context.DailyProductionRecords
                    .Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate)
                    .Select(r => r.DailyTarget)
                    .FirstOrDefault() > 0 
                        ? (double)_context.DailyProductionRecords
                            .Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate)
                            .Select(r => r.H1 + r.H2 + r.H3 + r.H4 + r.H5 + r.H7 + r.H8 + r.H9 + r.H10 + r.H11 + r.H12 + r.H13 + r.H14 + r.H15 + r.H16 + r.H17 + r.H18 + r.H19)
                            .FirstOrDefault() / 
                          _context.DailyProductionRecords
                            .Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate)
                            .Select(r => r.DailyTarget)
                            .FirstOrDefault() * 100 
                        : 0,

                H1 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H1).FirstOrDefault(),
                H2 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H2).FirstOrDefault(),
                H3 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H3).FirstOrDefault(),
                H4 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H4).FirstOrDefault(),
                H5 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H5).FirstOrDefault(),
                H6 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H6).FirstOrDefault(),
                H7 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H7).FirstOrDefault(),
                H8 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H8).FirstOrDefault(),
                H9 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H9).FirstOrDefault(),
                H10 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H10).FirstOrDefault(),
                H11 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H11).FirstOrDefault(),
                H12 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H12).FirstOrDefault(),
                H13 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H13).FirstOrDefault(),
                H14 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H14).FirstOrDefault(),
                H15 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H15).FirstOrDefault(),
                H16 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H16).FirstOrDefault(),
                H17 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H17).FirstOrDefault(),
                H18 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H18).FirstOrDefault(),
                H19 = _context.DailyProductionRecords.Where(r => r.AssignmentId == a.Id && r.Date.Date == targetDate).Select(r => r.H19).FirstOrDefault()
            })
            // Only show if there's either a target set up or some production has been recorded
            .Where(r => r.DailyTarget > 0 || r.Completed > 0);
        }
    }
}
