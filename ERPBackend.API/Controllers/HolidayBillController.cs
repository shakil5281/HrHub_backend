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
    public class HolidayBillController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HolidayBillController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<HolidayBillResponseDto>> GetHolidayBills(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? employeeId,
            [FromQuery] int? departmentId,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm)
        {
            try
            {
                var query = _context.HolidayBills
                    .Include(i => i.Employee).ThenInclude(e => e!.Department)
                    .Include(i => i.Employee).ThenInclude(e => e!.Company)
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

                var records = await query
                    .OrderByDescending(i => i.Date)
                    .Select(i => new HolidayBillDto
                    {
                        Id = i.Id,
                        EmployeeCard = i.EmployeeId,
                        EmployeeId = i.Employee!.EmployeeId,
                        EmployeeName = i.Employee!.FullNameEn,
                        Department = i.Employee!.Department!.NameEn,
                        Designation = i.Employee!.Designation!.NameEn,
                        Date = i.Date,
                        Amount = i.Amount,
                        Status = i.Status,
                        CreatedAt = i.CreatedAt,
                        ShiftName = i.Shift != null ? i.Shift.NameEn : "N/A",
                        CompanyName = i.Employee.Company != null ? i.Employee.Company.CompanyNameEn : ""
                    })
                    .ToListAsync();

                var summary = new HolidayBillSummaryDto
                {
                    TotalAmount = records.Sum(r => r.Amount),
                    TotalEmployees = records.Select(r => r.EmployeeId).Distinct().Count(),
                    TotalRecords = records.Count
                };

                return Ok(new HolidayBillResponseDto { Summary = summary, Records = records });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching holiday bills", error = ex.Message });
            }
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessHolidayBills([FromBody] BillProcessRequestDto request)
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
                int processedCount = 0;

                var employees = await _context.Employees
                    .Where(e => e.IsActive && (!request.DepartmentId.HasValue || e.DepartmentId == request.DepartmentId))
                    .Include(e => e.Shift)
                    .Include(e => e.Designation)
                    .ToListAsync();

                var attendanceRecords = await _context.Attendances
                    .Where(a => a.Date >= request.FromDate.Date && a.Date <= request.ToDate.Date && (a.Status.StartsWith("Present") || a.Status == "Late"))
                    .ToListAsync();

                var holidays = await _context.Holidays
                    .Where(h => h.StartDate <= request.ToDate.Date && h.EndDate >= request.FromDate.Date)
                    .ToListAsync();

                var existingBills = await _context.HolidayBills
                    .Where(i => i.Date >= request.FromDate.Date && i.Date <= request.ToDate.Date)
                    .ToListAsync();

                var formulas = new List<HolidayBill>();

                foreach (var att in attendanceRecords)
                {
                    var emp = employees.FirstOrDefault(e => e.Id == att.EmployeeCard);
                    if (emp == null || emp.Shift == null) continue;

                    bool isHoliday = holidays.Any(h => att.Date.Date >= h.StartDate.Date && att.Date.Date <= h.EndDate.Date && (h.CompanyId == null || h.CompanyId == emp.CompanyId));
                    if (!isHoliday && !string.IsNullOrEmpty(emp.Shift.Weekends))
                    {
                        var dayName = att.Date.DayOfWeek.ToString();
                        isHoliday = emp.Shift.Weekends.Split(',').Any(d => d.Trim().Equals(dayName, StringComparison.OrdinalIgnoreCase));
                    }

                    if (isHoliday)
                    {
                        var existing = existingBills.FirstOrDefault(i => i.EmployeeId == emp.Id && i.Date.Date == att.Date.Date);
                        if (existing == null)
                        {
                            formulas.Add(new HolidayBill
                            {
                                EmployeeId = emp.Id,
                                Date = att.Date,
                                Amount = emp.Designation?.HolidayBill ?? 0,
                                ShiftId = emp.Shift.Id,
                                CompanyId = emp.CompanyId,
                                Status = "Approved",
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = userName
                            });
                            processedCount++;
                        }
                    }
                }

                if (formulas.Any())
                {
                    await _context.HolidayBills.AddRangeAsync(formulas);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = $"Successfully processed {processedCount} Holiday Bill records." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing holiday bills", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHolidayBill(int id)
        {
            try
            {
                var record = await _context.HolidayBills.FindAsync(id);
                if (record == null) return NotFound();

                _context.HolidayBills.Remove(record);
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
                var records = await _context.HolidayBills.Where(r => ids.Contains(r.Id)).ToListAsync();
                if (!records.Any()) return NotFound();

                _context.HolidayBills.RemoveRange(records);
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
                var query = _context.HolidayBills
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
                var worksheet = package.Workbook.Worksheets.Add("Holiday Bills");

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
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Holiday_Bills_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error exporting data", error = ex.Message });
            }
        }
    }
}
