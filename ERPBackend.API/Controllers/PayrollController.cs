using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PayrollController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("monthly-sheet")]
        public async Task<ActionResult<IEnumerable<MonthlySalarySheetDto>>> GetMonthlySalarySheet(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] int? departmentId,
            [FromQuery] string? searchTerm)
        {
            var query = _context.MonthlySalarySheets
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Department)
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Designation)
                .Where(s => s.Year == year && s.Month == month);

            if (departmentId.HasValue)
            {
                query = query.Where(s => s.Employee!.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.Employee!.FullNameEn.Contains(searchTerm) || s.Employee.EmployeeId.Contains(searchTerm));
            }

            var records = await query.ToListAsync();

            var result = records.Select(s => new MonthlySalarySheetDto
            {
                Id = s.Id,
                EmployeeId = s.EmployeeId,
                EmployeeIdCard = s.Employee?.EmployeeId ?? "",
                EmployeeName = s.Employee?.FullNameEn ?? "",
                Department = s.Employee?.Department?.NameEn ?? "",
                Designation = s.Employee?.Designation?.NameEn ?? "",
                Year = s.Year,
                Month = s.Month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(s.Month),
                GrossSalary = s.GrossSalary,
                BasicSalary = s.BasicSalary,
                TotalDays = s.TotalDays,
                PresentDays = s.PresentDays,
                AbsentDays = s.AbsentDays,
                LeaveDays = s.LeaveDays,
                Holidays = s.Holidays,
                WeekendDays = s.WeekendDays,
                OTHours = s.OTHours,
                OTAmount = s.OTAmount,
                AttendanceBonus = s.AttendanceBonus,
                OtherAllowances = s.OtherAllowances,
                TotalEarning = s.TotalEarning,
                TotalDeduction = s.TotalDeduction,
                NetPayable = s.NetPayable,
                Status = s.Status
            }).ToList();

            return Ok(result);
        }

        [HttpGet("daily-sheet")]
        public async Task<ActionResult<IEnumerable<DailySalarySheetDto>>> GetDailySalarySheet(
            [FromQuery] DateTime date,
            [FromQuery] int? departmentId,
            [FromQuery] string? searchTerm)
        {
            var query = _context.DailySalarySheets
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Department)
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Designation)
                .Where(s => s.Date.Date == date.Date);

            if (departmentId.HasValue)
            {
                query = query.Where(s => s.Employee!.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.Employee!.FullNameEn.Contains(searchTerm) || s.Employee.EmployeeId.Contains(searchTerm));
            }

            var records = await query.ToListAsync();

            var result = records.Select(s => new DailySalarySheetDto
            {
                Id = s.Id,
                EmployeeId = s.EmployeeId,
                EmployeeIdCard = s.Employee?.EmployeeId ?? "",
                EmployeeName = s.Employee?.FullNameEn ?? "",
                Department = s.Employee?.Department?.NameEn ?? "",
                Designation = s.Employee?.Designation?.NameEn ?? "",
                Date = s.Date,
                GrossSalary = s.GrossSalary,
                PerDaySalary = s.PerDaySalary,
                AttendanceStatus = s.AttendanceStatus,
                OTHours = s.OTHours,
                OTAmount = s.OTAmount,
                TotalEarning = s.TotalEarning,
                Deduction = s.Deduction,
                NetPayable = s.NetPayable
            }).ToList();

            return Ok(result);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<SalarySummaryDto>> GetSalarySummary([FromQuery] int year, [FromQuery] int month)
        {
            var records = await _context.MonthlySalarySheets
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Department)
                .Where(s => s.Year == year && s.Month == month)
                .ToListAsync();

            if (!records.Any()) return Ok(new SalarySummaryDto());

            var summary = new SalarySummaryDto
            {
                TotalGrossSalary = records.Sum(s => s.GrossSalary),
                TotalOTAmount = records.Sum(s => s.OTAmount),
                TotalDeductions = records.Sum(s => s.TotalDeduction),
                TotalNetPayable = records.Sum(s => s.NetPayable),
                TotalEmployees = records.Count,
                DepartmentSummaries = records
                    .GroupBy(r => r.Employee?.Department?.NameEn ?? "Unknown")
                    .Select(g => new DepartmentSalarySummaryDto
                    {
                        DepartmentName = g.Key,
                        TotalAmount = g.Sum(x => x.NetPayable),
                        EmployeeCount = g.Count()
                    }).ToList()
            };

            return Ok(summary);
        }

        [HttpGet("payslip/{id}")]
        public async Task<ActionResult<PayslipDto>> GetPayslip(int id)
        {
            var s = await _context.MonthlySalarySheets
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Department)
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Designation)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (s == null) return NotFound();

            var result = new PayslipDto
            {
                Id = s.Id,
                EmployeeId = s.EmployeeId,
                EmployeeIdCard = s.Employee?.EmployeeId ?? "",
                EmployeeName = s.Employee?.FullNameEn ?? "",
                Department = s.Employee?.Department?.NameEn ?? "",
                Designation = s.Employee?.Designation?.NameEn ?? "",
                Year = s.Year,
                Month = s.Month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(s.Month),
                GrossSalary = s.GrossSalary,
                BasicSalary = s.BasicSalary,
                TotalDays = s.TotalDays,
                PresentDays = s.PresentDays,
                AbsentDays = s.AbsentDays,
                LeaveDays = s.LeaveDays,
                Holidays = s.Holidays,
                WeekendDays = s.WeekendDays,
                OTHours = s.OTHours,
                OTAmount = s.OTAmount,
                AttendanceBonus = s.AttendanceBonus,
                OtherAllowances = s.OtherAllowances,
                TotalEarning = s.TotalEarning,
                TotalDeduction = s.TotalDeduction,
                NetPayable = s.NetPayable,
                Status = s.Status,
                JoinedDate = s.Employee?.JoinDate.ToString("dd MMM yyyy") ?? "",
                BankAccountNo = s.Employee?.BankAccountNo ?? "N/A"
            };

            return Ok(result);
        }

        [HttpPost("process")]
        public async Task<ActionResult> ProcessSalary([FromBody] SalaryProcessRequestDto request)
        {
            // Simple mock processing logic
            // In reality, this would query attendance, leaves, OT deductions, etc.
            
            var employeesQuery = _context.Employees.Where(e => e.Status == "Active");
            
            if (request.EmployeeId.HasValue)
                employeesQuery = employeesQuery.Where(e => e.Id == request.EmployeeId.Value);
            
            if (request.DepartmentId.HasValue)
                employeesQuery = employeesQuery.Where(e => e.DepartmentId == request.DepartmentId.Value);

            var employees = await employeesQuery.ToListAsync();
            
            var existingSheets = await _context.MonthlySalarySheets
                .Where(s => s.Year == request.Year && s.Month == request.Month)
                .ToListAsync();

            foreach (var emp in employees)
            {
                var sheet = existingSheets.FirstOrDefault(s => s.EmployeeId == emp.Id) ?? new MonthlySalarySheet();
                
                sheet.EmployeeId = emp.Id;
                sheet.Year = request.Year;
                sheet.Month = request.Month;
                sheet.GrossSalary = emp.GrossSalary ?? 0;
                sheet.BasicSalary = emp.BasicSalary ?? 0;
                
                // Mock numbers for demonstration
                sheet.TotalDays = DateTime.DaysInMonth(request.Year, request.Month);
                sheet.PresentDays = 22; // Random mock
                sheet.AbsentDays = (sheet.TotalDays > 26) ? 2 : 0;
                sheet.LeaveDays = 1;
                sheet.Holidays = 2;
                sheet.WeekendDays = 4;
                
                sheet.OTHours = 10; // Mock
                sheet.OTRate = (sheet.BasicSalary / 208) * 2; // Mock OT rate
                sheet.OTAmount = sheet.OTHours * sheet.OTRate;
                
                sheet.AttendanceBonus = (sheet.AbsentDays == 0) ? 500 : 0;
                sheet.OtherAllowances = 1000;
                
                sheet.TotalEarning = sheet.GrossSalary + sheet.OTAmount + sheet.AttendanceBonus + sheet.OtherAllowances;
                
                decimal perDayGross = sheet.GrossSalary / sheet.TotalDays;
                sheet.AbsentDeduction = sheet.AbsentDays * perDayGross;
                sheet.AdvanceDeduction = 0;
                sheet.OTDeduction = 0;
                
                sheet.TotalDeduction = sheet.AbsentDeduction + sheet.AdvanceDeduction + sheet.OTDeduction;
                sheet.NetPayable = sheet.TotalEarning - sheet.TotalDeduction;
                sheet.Status = "Processed";
                sheet.ProcessedAt = DateTime.UtcNow;

                if (sheet.Id == 0)
                    _context.MonthlySalarySheets.Add(sheet);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Processed salary for {employees.Count} employees." });
        }
        [HttpGet("advance-salary")]
        public async Task<ActionResult<IEnumerable<AdvanceSalaryDto>>> GetAdvanceSalaries([FromQuery] int? month, [FromQuery] int? year)
        {
            var query = _context.AdvanceSalaries
                .Include(a => a.Employee)
                .AsQueryable();

            if (month.HasValue) query = query.Where(a => a.RepaymentMonth == month.Value);
            if (year.HasValue) query = query.Where(a => a.RepaymentYear == year.Value);

            var records = await query.ToListAsync();
            return Ok(records.Select(a => new AdvanceSalaryDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeIdCard = a.Employee?.EmployeeId ?? "",
                EmployeeName = a.Employee?.FullNameEn ?? "",
                Amount = a.Amount,
                RequestDate = a.RequestDate,
                RepaymentMonth = a.RepaymentMonth,
                RepaymentYear = a.RepaymentYear,
                Status = a.Status,
                Remarks = a.Remarks
            }));
        }

        [HttpPost("advance-salary")]
        public async Task<ActionResult> CreateAdvanceSalary([FromBody] CreateAdvanceSalaryDto dto)
        {
            var advance = new AdvanceSalary
            {
                EmployeeId = dto.EmployeeId,
                Amount = dto.Amount,
                RequestDate = dto.RequestDate,
                RepaymentMonth = dto.RepaymentMonth,
                RepaymentYear = dto.RepaymentYear,
                Remarks = dto.Remarks,
                Status = "Approved" // Auto approve for demo
            };

            _context.AdvanceSalaries.Add(advance);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Advance salary request created." });
        }

        [HttpGet("increments")]
        public async Task<ActionResult<IEnumerable<SalaryIncrementDto>>> GetIncrements()
        {
            var records = await _context.SalaryIncrements
                .Include(i => i.Employee)
                .OrderByDescending(i => i.EffectiveDate)
                .ToListAsync();

            return Ok(records.Select(i => new SalaryIncrementDto
            {
                Id = i.Id,
                EmployeeId = i.EmployeeId,
                EmployeeIdCard = i.Employee?.EmployeeId ?? "",
                EmployeeName = i.Employee?.FullNameEn ?? "",
                PreviousGrossSalary = i.PreviousGrossSalary,
                IncrementAmount = i.IncrementAmount,
                NewGrossSalary = i.NewGrossSalary,
                EffectiveDate = i.EffectiveDate,
                IncrementType = i.IncrementType,
                IsApplied = i.IsApplied
            }));
        }

        [HttpPost("increment")]
        public async Task<ActionResult> CreateIncrement([FromBody] CreateSalaryIncrementDto dto)
        {
            var employee = await _context.Employees.FindAsync(dto.EmployeeId);
            if (employee == null) return NotFound("Employee not found");

            var increment = new SalaryIncrement
            {
                EmployeeId = dto.EmployeeId,
                PreviousGrossSalary = employee.GrossSalary ?? 0,
                IncrementAmount = dto.IncrementAmount,
                NewGrossSalary = (employee.GrossSalary ?? 0) + dto.IncrementAmount,
                EffectiveDate = dto.EffectiveDate,
                IncrementType = dto.IncrementType,
                Remarks = dto.Remarks,
                IsApplied = true
            };

            employee.GrossSalary = increment.NewGrossSalary;
            employee.BasicSalary = increment.NewGrossSalary * 0.6m; // Re-calculate basic

            _context.SalaryIncrements.Add(increment);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Salary increment applied." });
        }

        [HttpGet("bonuses")]
        public async Task<ActionResult<IEnumerable<BonusDto>>> GetBonuses([FromQuery] int year, [FromQuery] int? month)
        {
            var query = _context.Bonuses
                .Include(b => b.Employee)
                .Where(b => b.Year == year);

            if (month.HasValue) query = query.Where(b => b.Month == month.Value);

            var records = await query.ToListAsync();
            return Ok(records.Select(b => new BonusDto
            {
                Id = b.Id,
                EmployeeId = b.EmployeeId,
                EmployeeIdCard = b.Employee?.EmployeeId ?? "",
                EmployeeName = b.Employee?.FullNameEn ?? "",
                BonusType = b.BonusType,
                Amount = b.Amount,
                Year = b.Year,
                Month = b.Month,
                Status = b.Status
            }));
        }

        [HttpPost("bonus")]
        public async Task<ActionResult> CreateBonus([FromBody] CreateBonusDto dto)
        {
            var bonus = new Bonus
            {
                EmployeeId = dto.EmployeeId,
                BonusType = dto.BonusType,
                Amount = dto.Amount,
                Year = dto.Year,
                Month = dto.Month,
                PaymentDate = DateTime.UtcNow,
                Status = "Approved"
            };

            _context.Bonuses.Add(bonus);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Bonus processed." });
        }
    }
}
