using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LeaveController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("types")]
        public async Task<ActionResult<IEnumerable<LeaveTypeDto>>> GetLeaveTypes()
        {
            var types = await _context.LeaveTypes.Where(t => t.IsActive).ToListAsync();
            return Ok(types.Select(t => new LeaveTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                YearlyLimit = t.YearlyLimit,
                IsCarryForward = t.IsCarryForward,
                Description = t.Description
            }));
        }

        [HttpGet("applications")]
        public async Task<ActionResult<IEnumerable<LeaveApplicationDto>>> GetApplications(
            [FromQuery] int? employeeId,
            [FromQuery] string? status)
        {
            var query = _context.LeaveApplications
                .Include(l => l.Employee)
                .ThenInclude(e => e!.Department)
                .Include(l => l.LeaveType)
                .AsQueryable();

            if (employeeId.HasValue) query = query.Where(l => l.EmployeeId == employeeId.Value);
            if (!string.IsNullOrEmpty(status)) query = query.Where(l => l.Status == status);

            var records = await query.OrderByDescending(l => l.AppliedDate).ToListAsync();

            return Ok(records.Select(l => new LeaveApplicationDto
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                EmployeeIdCard = l.Employee?.EmployeeId ?? "",
                EmployeeName = l.Employee?.FullNameEn ?? "",
                Department = l.Employee?.Department?.NameEn ?? "",
                LeaveTypeId = l.LeaveTypeId,
                LeaveTypeName = l.LeaveType?.Name ?? "",
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                TotalDays = l.TotalDays,
                Reason = l.Reason,
                Status = l.Status,
                AppliedDate = l.AppliedDate,
                Remarks = l.Remarks,
                AttachmentUrl = l.AttachmentUrl
            }));
        }

        [HttpPost("apply")]
        public async Task<ActionResult> ApplyLeave([FromBody] CreateLeaveApplicationDto dto)
        {
            var totalDays = (decimal)(dto.EndDate - dto.StartDate).TotalDays + 1;
            if (totalDays <= 0) return BadRequest("Invalid date range");

            var application = new LeaveApplication
            {
                EmployeeId = dto.EmployeeId,
                LeaveTypeId = dto.LeaveTypeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TotalDays = totalDays,
                Reason = dto.Reason,
                Status = "Pending",
                AppliedDate = DateTime.UtcNow,
                AttachmentUrl = dto.AttachmentUrl
            };

            _context.LeaveApplications.Add(application);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Leave application submitted successfully." });
        }

        [HttpPost("action")]
        public async Task<ActionResult> ActionLeave([FromBody] LeaveActionDto dto)
        {
            var application = await _context.LeaveApplications.FindAsync(dto.Id);
            if (application == null) return NotFound();

            application.Status = dto.Status;
            application.Remarks = dto.Remarks;
            application.ActionDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Leave application {dto.Status} successfully." });
        }

        [HttpGet("balance/{employeeId}")]
        public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetLeaveBalance(int employeeId)
        {
            var types = await _context.LeaveTypes.Where(t => t.IsActive).ToListAsync();
            var applications = await _context.LeaveApplications
                .Where(l => l.EmployeeId == employeeId && l.Status == "Approved" && l.StartDate.Year == DateTime.Now.Year)
                .ToListAsync();

            var balances = types.Select(t => new LeaveBalanceDto
            {
                LeaveTypeId = t.Id,
                LeaveTypeName = t.Name,
                TotalAllocated = t.YearlyLimit,
                TotalTaken = applications.Where(a => a.LeaveTypeId == t.Id).Sum(a => a.TotalDays),
                Balance = t.YearlyLimit - applications.Where(a => a.LeaveTypeId == t.Id).Sum(a => a.TotalDays)
            }).ToList();

            return Ok(balances);
        }

        [HttpPost("seed")]
        public async Task<ActionResult> SeedLeaveTypes()
        {
            if (await _context.LeaveTypes.AnyAsync()) return Ok(new { message = "Leave types already seeded." });

            var types = new List<LeaveType>
            {
                new() { Name = "Sick Leave", Code = "SL", YearlyLimit = 14, IsCarryForward = false },
                new() { Name = "Casual Leave", Code = "CL", YearlyLimit = 10, IsCarryForward = false },
                new() { Name = "Earned Leave", Code = "EL", YearlyLimit = 20, IsCarryForward = true },
                new() { Name = "Maternity Leave", Code = "ML", YearlyLimit = 112, IsCarryForward = false },
            };

            _context.LeaveTypes.AddRange(types);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Leave types seeded successfully." });
        }

        [HttpGet("monthly-report")]
        public async Task<ActionResult> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var applications = await _context.LeaveApplications
                .Include(l => l.Employee)
                .ThenInclude(e => e!.Department)
                .Include(l => l.LeaveType)
                .Where(l => l.Status == "Approved" &&
                           ((l.StartDate >= startDate && l.StartDate <= endDate) ||
                            (l.EndDate >= startDate && l.EndDate <= endDate)))
                .ToListAsync();

            var report = applications
                .GroupBy(l => new { l.EmployeeId, l.Employee!.FullNameEn, EmployeeIdCard = l.Employee.EmployeeId, Dept = l.Employee.Department?.NameEn ?? "N/A" })
                .Select(g => new
                {
                    EmployeeId = g.Key.EmployeeId,
                    EmployeeIdCard = g.Key.EmployeeIdCard,
                    EmployeeName = g.Key.FullNameEn,
                    Department = g.Key.Dept,
                    SickLeave = g.Where(x => x.LeaveType!.Code == "SL").Sum(x => x.TotalDays),
                    CasualLeave = g.Where(x => x.LeaveType!.Code == "CL").Sum(x => x.TotalDays),
                    EarnedLeave = g.Where(x => x.LeaveType!.Code == "EL").Sum(x => x.TotalDays),
                    OtherLeave = g.Where(x => x.LeaveType!.Code != "SL" && x.LeaveType!.Code != "CL" && x.LeaveType!.Code != "EL").Sum(x => x.TotalDays),
                    TotalDays = g.Sum(x => x.TotalDays)
                })
                .ToList();

            return Ok(report);
        }
    }
}
