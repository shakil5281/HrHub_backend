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
                // Return default values if record doesn't exist yet
                return Ok(new DailyProductionRecordDto
                {
                    AssignmentId = assignmentId,
                    Date = targetDate,
                    DailyTarget = 0,
                    HourlyTarget = 0
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
                TotalCompleted = record.TotalCompleted
            });
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
                    TotalCompleted = g.Sum(r => r.H1 + r.H2 + r.H3 + r.H4 + r.H5 + r.H6 + r.H7 + r.H8 + r.H9 + r.H10 + r.H11 + r.H12),
                    AvgAchievement = g.Sum(r => r.DailyTarget) > 0 
                        ? (double)g.Sum(r => r.H1 + r.H2 + r.H3 + r.H4 + r.H5 + r.H6 + r.H7 + r.H8 + r.H9 + r.H10 + r.H11 + r.H12) / g.Sum(r => r.DailyTarget) * 100 
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
            try
            {
                var query = GetDailyReportQuery(filters);
                var data = await query.ToListAsync();

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Daily Production Report");
                    var date = filters.Date ?? DateTime.Today;

                    // Header
                    worksheet.Cells["A1:F1"].Merge = true;
                    worksheet.Cells["A1"].Value = "Daily Production Report";
                    worksheet.Cells["A1"].Style.Font.Size = 16;
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    worksheet.Cells["A2:F2"].Merge = true;
                    worksheet.Cells["A2"].Value = $"Date: {date:dd MMMM yyyy}";
                    worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Table Headers
                    int row = 4;
                    worksheet.Cells[row, 1].Value = "Line Name";
                    worksheet.Cells[row, 2].Value = "Style No";
                    worksheet.Cells[row, 3].Value = "Buyer";
                    worksheet.Cells[row, 4].Value = "Daily Target";
                    worksheet.Cells[row, 5].Value = "Completed";
                    worksheet.Cells[row, 6].Value = "Achievement %";

                    using (var range = worksheet.Cells[row, 1, row, 6])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    row++;
                    foreach (var item in data)
                    {
                        worksheet.Cells[row, 1].Value = item.LineName;
                        worksheet.Cells[row, 2].Value = item.StyleNo;
                        worksheet.Cells[row, 3].Value = item.Buyer;
                        worksheet.Cells[row, 4].Value = item.DailyTarget;
                        worksheet.Cells[row, 5].Value = item.Completed;
                        worksheet.Cells[row, 6].Value = item.Achievement;
                        worksheet.Cells[row, 6].Style.Numberformat.Format = "0.0\"%\"";

                        for (int i = 1; i <= 6; i++)
                        {
                            worksheet.Cells[row, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        }
                        row++;
                    }

                    worksheet.Cells.AutoFitColumns();
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
            var query = _context.DailyProductionRecords
                .Include(r => r.Assignment)
                    .ThenInclude(a => a!.Production)
                .Include(r => r.Assignment)
                    .ThenInclude(a => a!.Line)
                .Where(r => r.Date.Date == targetDate);

            if (filters.LineId.HasValue)
                query = query.Where(r => r.Assignment!.LineId == filters.LineId.Value);

            if (!string.IsNullOrEmpty(filters.Buyer))
                query = query.Where(r => r.Assignment!.Production!.Buyer.Contains(filters.Buyer));

            if (!string.IsNullOrEmpty(filters.StyleNo))
                query = query.Where(r => r.Assignment!.Production!.StyleNo.Contains(filters.StyleNo));

            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                var term = filters.SearchTerm.ToLower();
                query = query.Where(r => 
                    r.Assignment!.Line!.LineName.ToLower().Contains(term) ||
                    r.Assignment!.Production!.StyleNo.ToLower().Contains(term) ||
                    r.Assignment!.Production!.Buyer.ToLower().Contains(term));
            }

            return query.Select(r => new DailyReportItemDto
            {
                AssignmentId = r.AssignmentId,
                LineName = r.Assignment!.Line!.LineName,
                StyleNo = r.Assignment!.Production!.StyleNo,
                Buyer = r.Assignment!.Production!.Buyer,
                DailyTarget = r.DailyTarget,
                HourlyTarget = r.HourlyTarget,
                Completed = r.H1 + r.H2 + r.H3 + r.H4 + r.H5 + r.H6 + r.H7 + r.H8 + r.H9 + r.H10 + r.H11 + r.H12,
                Achievement = r.DailyTarget > 0 
                    ? (double)(r.H1 + r.H2 + r.H3 + r.H4 + r.H5 + r.H6 + r.H7 + r.H8 + r.H9 + r.H10 + r.H11 + r.H12) / r.DailyTarget * 100 
                    : 0
            });
        }
    }
}
