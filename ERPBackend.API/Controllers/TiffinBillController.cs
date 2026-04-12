using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TiffinBillController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TiffinBillController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<TiffinBillResponseDto>> GetTiffinBills(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? employeeId,
            [FromQuery] int? departmentId,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm,
            [FromQuery] string? employeeType)
        {
            try
            {
                var queryDateFrom = fromDate?.Date;
                var queryDateTo = toDate?.Date;

                // First get the bills
                var baseQuery = _context.TiffinBills
                    .Include(i => i.Employee).ThenInclude(e => e!.Department)
                    .Include(i => i.Employee).ThenInclude(e => e!.Company)
                    .Include(i => i.Employee).ThenInclude(e => e!.Designation)
                    .Include(i => i.Shift)
                    .Where(b => (queryDateFrom == null || b.Date >= queryDateFrom) &&
                                (queryDateTo == null || b.Date <= queryDateTo) &&
                                (employeeId == null || b.EmployeeId == employeeId) &&
                                (departmentId == null || b.Employee!.DepartmentId == departmentId) &&
                                (string.IsNullOrEmpty(status) || b.Status == status) &&
                                (string.IsNullOrEmpty(searchTerm) || b.Employee!.EmployeeId.Contains(searchTerm) || b.Employee!.FullNameEn.Contains(searchTerm)));

                var bills = await baseQuery.ToListAsync();

                // Get attendance for all those employees and dates to fill gaps
                var empCodes = bills.Select(b => b.Employee?.EmployeeId).Where(id => id != null).Distinct().ToList();
                var dates = bills.Select(b => b.Date.Date).Distinct().ToList();

                var endDateLimit = queryDateTo?.AddDays(1) ?? DateTime.MaxValue;
                var attendanceList = await _context.Attendances
                    .Where(a => empCodes.Contains(a.EmployeeId) && (queryDateFrom == null || a.Date >= queryDateFrom) && a.Date <= endDateLimit)
                    .ToListAsync();

                var records = bills.Select(b => {
                    var att = attendanceList.FirstOrDefault(a => a.EmployeeId == b.Employee?.EmployeeId && a.Date.Date == b.Date.Date);
                    return new TiffinBillDto
                    {
                        Id = b.Id,
                        EmployeeCard = b.EmployeeId,
                        EmployeeId = b.Employee?.EmployeeId ?? "",
                        EmployeeName = b.Employee?.FullNameEn ?? "",
                        Department = b.Employee?.Department?.NameEn ?? "",
                        Designation = b.Employee?.Designation?.NameEn ?? "",
                        Date = b.Date,
                        Amount = b.Amount,
                        TiffinCount = b.TiffinCount > 0 ? b.TiffinCount : (att != null ? CalculateTiffinCount(att) : 0),
                        InTime = b.InTime ?? att?.InTime,
                        OutTime = b.OutTime ?? att?.OutTime,
                        Status = b.Status,
                        CreatedAt = b.CreatedAt,
                        ShiftName = b.Shift != null ? b.Shift.NameEn : "N/A",
                        CompanyName = b.Employee?.Company != null ? b.Employee.Company.CompanyNameEn : ""
                    };
                }).OrderBy(r => r.EmployeeId).ToList();

                if (!string.IsNullOrEmpty(employeeType))
                {
                    if (employeeType.ToLower() == "staff")
                        records = records.Where(r => _context.Employees.Include(e => e.Designation).Any(e => e.EmployeeId == r.EmployeeId && e.Designation!.IsStaff)).ToList();
                    else if (employeeType.ToLower() == "worker")
                        records = records.Where(r => _context.Employees.Include(e => e.Designation).Any(e => e.EmployeeId == r.EmployeeId && !e.Designation!.IsStaff)).ToList();
                }

                var summary = new TiffinBillSummaryDto
                {
                    TotalAmount = records.Sum(r => r.Amount),
                    TotalEmployees = records.Select(r => r.EmployeeCard).Distinct().Count(),
                    TotalRecords = records.Count
                };

                return Ok(new TiffinBillResponseDto { Summary = summary, Records = records });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching tiffin bills", error = ex.Message });
            }
        }

        private static int CalculateTiffinCount(Attendance att)
        {
            if (att.OutTime == null) return 0;
            var outTime = att.OutTime.Value;
            var baseDate = att.Date.Date;
            
            if (outTime > baseDate.AddDays(1).AddHours(5)) return 3;
            if (outTime > baseDate.AddDays(1)) return 2;
            if (outTime > baseDate.AddHours(22)) return 1;
            return 0;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessTiffinBills([FromBody] BillProcessRequestDto request)
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
                int processedCount = 0;

                var fromDate = request.FromDate.Date;
                var toDate = request.ToDate.Date.AddDays(1).AddSeconds(-1);

                var employees = await _context.Employees
                    .Where(e => e.IsActive && (!request.DepartmentId.HasValue || e.DepartmentId == request.DepartmentId))
                    .Include(e => e.Shift)
                    .Include(e => e.Designation)
                    .ToListAsync();

                var attendanceRecords = await _context.Attendances
                    .Where(a => a.Date >= fromDate && a.Date <= toDate && a.OTHours >= 2)
                    .ToListAsync();

                var existingBills = await _context.TiffinBills
                    .Where(i => i.Date >= fromDate && i.Date <= toDate)
                    .ToListAsync();

                var formulas = new List<TiffinBill>();

                foreach (var att in attendanceRecords)
                {
                    var emp = employees.FirstOrDefault(e => e.Id == att.EmployeeCard);
                    if (emp == null || att.OutTime == null) continue;

                    // Tiffin Logic
                    int tiffinCount = 0;
                    var outTime = att.OutTime.Value;
                    var baseDate = att.Date.Date;
                    
                    if (outTime > baseDate.AddDays(1).AddHours(5)) // Next day 05:00 AM
                        tiffinCount = 3;
                    else if (outTime > baseDate.AddDays(1)) // Midnight 00:00 AM
                        tiffinCount = 2;
                    else if (outTime > baseDate.AddHours(22)) // Tonight 10:00 PM
                        tiffinCount = 1;

                    if (tiffinCount == 0) continue;

                    var existing = existingBills.FirstOrDefault(i => i.EmployeeId == emp.Id && i.Date.Date == att.Date.Date);
                    if (existing == null)
                    {
                        var rate = emp.Designation?.TiffinBill ?? 10; // Default rate if not set
                        formulas.Add(new TiffinBill
                        {
                            EmployeeId = emp.Id,
                            Date = att.Date,
                            Amount = tiffinCount * rate,
                            TiffinCount = tiffinCount,
                            InTime = att.InTime,
                            OutTime = att.OutTime,
                            ShiftId = emp.ShiftId,
                            CompanyId = emp.CompanyId,
                            Status = "Approved",
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userName
                        });
                        processedCount++;
                    }
                }

                if (formulas.Any())
                {
                    await _context.TiffinBills.AddRangeAsync(formulas);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = $"Successfully processed {processedCount} Tiffin Bill records." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing tiffin bills", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTiffinBill(int id)
        {
            try
            {
                var record = await _context.TiffinBills.FindAsync(id);
                if (record == null) return NotFound();

                _context.TiffinBills.Remove(record);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Record deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting record", error = ex.Message });
            }
        }

        [HttpPost("delete-multiple")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            try
            {
                var records = await _context.TiffinBills.Where(r => ids.Contains(r.Id)).ToListAsync();
                if (!records.Any()) return NotFound();

                _context.TiffinBills.RemoveRange(records);
                await _context.SaveChangesAsync();
                return Ok(new { message = $"Successfully deleted {records.Count} records" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting records", error = ex.Message });
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportExcel(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? employeeId,
            [FromQuery] int? departmentId,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm)
        {
            try
            {
                var query = _context.TiffinBills
                    .Include(i => i.Employee).ThenInclude(e => e!.Department)
                    .Include(i => i.Employee).ThenInclude(e => e!.Designation)
                    .Include(i => i.Shift)
                    .AsQueryable();

                if (fromDate.HasValue) query = query.Where(i => i.Date.Date >= fromDate.Value.Date);
                if (toDate.HasValue) query = query.Where(i => i.Date.Date <= toDate.Value.Date);
                if (employeeId.HasValue) query = query.Where(i => i.EmployeeId == employeeId.Value);
                if (departmentId.HasValue) query = query.Where(i => i.Employee!.DepartmentId == departmentId.Value);
                if (!string.IsNullOrWhiteSpace(status)) query = query.Where(i => i.Status == status);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                    query = query.Where(o => o.Employee!.EmployeeId.Contains(searchTerm) || o.Employee!.FullNameEn.Contains(searchTerm));

                var records = await query.OrderByDescending(i => i.Date).ToListAsync();

                using var package = new OfficeOpenXml.ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Tiffin Bills");

                var headers = new[] { "SL", "ID", "Name", "Department", "Designation", "Date", "Shift", "Amount", "Status" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                int row = 2;
                foreach (var item in records)
                {
                    worksheet.Cells[row, 1].Value = row - 1;
                    worksheet.Cells[row, 2].Value = item.Employee?.EmployeeId;
                    worksheet.Cells[row, 3].Value = item.Employee?.FullNameEn;
                    worksheet.Cells[row, 4].Value = item.Employee?.Department?.NameEn;
                    worksheet.Cells[row, 5].Value = item.Employee?.Designation?.NameEn;
                    worksheet.Cells[row, 6].Value = item.Date.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 7].Value = item.Shift?.NameEn;
                    worksheet.Cells[row, 8].Value = item.Amount;
                    worksheet.Cells[row, 9].Value = item.Status;
                    row++;
                }

                worksheet.Cells.AutoFitColumns();
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Tiffin_Bills_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error exporting data", error = ex.Message });
            }
        }
    }
}
