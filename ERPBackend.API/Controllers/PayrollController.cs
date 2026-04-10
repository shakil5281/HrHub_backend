using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Core.Entities;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

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

        private string GetJobAge(DateTime? joinDate)
        {
            if (!joinDate.HasValue) return "N/A";
            var today = DateTime.Today;
            int years = today.Year - joinDate.Value.Year;
            int months = today.Month - joinDate.Value.Month;
            if (months < 0) { years--; months += 12; }
            return $"{years} year {months} month";
        }

        [HttpGet("monthly-sheet")]
        public async Task<ActionResult<IEnumerable<MonthlySalarySheetDto>>> GetMonthlySalarySheet(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] int? companyId,
            [FromQuery] int? departmentId,
            [FromQuery] string? searchTerm)
        {
            var query = _context.MonthlySalarySheets
                .Include(s => s.Company)
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Department)
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Designation)
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Company)
                .Where(s => s.Year == year && s.Month == month);

            if (companyId.HasValue && companyId > 0)
            {
                query = query.Where(s => s.Employee!.CompanyId == companyId.Value || s.CompanyId == companyId.Value);
            }

            if (departmentId.HasValue)
            {
                query = query.Where(s => s.Employee!.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s =>
                    s.Employee!.FullNameEn.Contains(searchTerm) || s.Employee.EmployeeId.Contains(searchTerm));
            }

            var records = await query.ToListAsync();

            var result = records.Select(s => new MonthlySalarySheetDto
            {
                Id = s.Id,
                EmployeeId = s.Employee?.EmployeeId ?? "",
                CompanyId = s.Employee?.CompanyId ?? s.CompanyId ?? 0,
                EmployeeName = s.Employee?.FullNameEn ?? "",
                Department = s.Employee?.Department?.NameEn ?? "",
                Designation = s.Employee?.Designation?.NameEn ?? "",
                Year = s.Year,
                Month = s.Month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(s.Month),
                GrossSalary = s.GrossSalary,
                BasicSalary = s.BasicSalary,
                HouseRent = s.Employee?.HouseRent ?? 0,
                MedicalAllowance = s.Employee?.MedicalAllowance ?? 0,
                FoodAllowance = s.Employee?.FoodAllowance ?? 0,
                Conveyance = s.Employee?.Conveyance ?? 0,
                TotalDays = s.TotalDays,
                PresentDays = s.PresentDays,
                AbsentDays = s.AbsentDays,
                LeaveDays = s.LeaveDays,
                Holidays = s.Holidays,
                WeekendDays = s.WeekendDays,
                OTHours = s.OTHours,
                OTRate = s.OTRate,
                OTAmount = s.OTAmount,
                AttendanceBonus = s.AttendanceBonus,
                OtherAllowances = s.OtherAllowances,
                TotalEarning = s.TotalEarning,
                AbsentDeduction = s.AbsentDeduction,
                TotalDeduction = s.TotalDeduction,
                NetPayable = s.NetPayable,
                Status = s.Status,
                CompanyName = s.Employee?.Company?.CompanyNameEn ?? s.Company?.CompanyNameEn ?? "",
                JoinedDate = s.Employee?.JoinDate.ToString("yyyy-MM-dd"),
                BankAccountNo = s.Employee?.BankAccountNo
            }).ToList();

            return Ok(result);
        }

        [HttpGet("daily-sheet")]
        public async Task<ActionResult<IEnumerable<DailySalarySheetDto>>> GetDailySalarySheet(
            [FromQuery] DateTime date,
            [FromQuery] int? companyId,
            [FromQuery] int? departmentId,
            [FromQuery] string? searchTerm)
        {
            var query = _context.DailySalarySheets
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Department)
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Designation)
                .Include(s => s.Company)
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Company)
                .Where(s => s.Date.Date == date.Date);

            if (companyId.HasValue && companyId > 0)
            {
                query = query.Where(s => s.Employee!.CompanyId == companyId.Value || s.CompanyId == companyId.Value);
            }

            if (departmentId.HasValue)
            {
                query = query.Where(s => s.Employee!.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s =>
                    s.Employee!.FullNameEn.Contains(searchTerm) || s.Employee.EmployeeId.Contains(searchTerm));
            }

            var records = await query.ToListAsync();

            var result = records.Select(s => new DailySalarySheetDto
            {
                Id = s.Id,
                EmployeeId = s.Employee?.EmployeeId ?? "",
                CompanyId = s.Employee?.CompanyId ?? s.CompanyId ?? 0,
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
                NetPayable = s.NetPayable,
                CompanyName = s.Employee?.Company?.CompanyNameEn ?? s.Company?.CompanyNameEn ?? ""
            }).ToList();

            return Ok(result);
        }

        [HttpGet("bank-sheet")]
        public async Task<ActionResult<IEnumerable<BankSheetDto>>> GetBankSheet(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] int? companyId,
            [FromQuery] int? departmentId,
            [FromQuery] string? searchTerm)
        {
            var query = _context.MonthlySalarySheets
                .Include(s => s.Company)
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Department)
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Company)
                .Where(s => s.Year == year && s.Month == month && s.Employee != null &&
                            !string.IsNullOrEmpty(s.Employee.BankAccountNo));

            if (companyId.HasValue && companyId > 0)
            {
                query = query.Where(s => s.Employee!.CompanyId == companyId.Value || s.CompanyId == companyId.Value);
            }

            if (departmentId.HasValue)
            {
                query = query.Where(s => s.Employee!.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s =>
                    s.Employee!.FullNameEn.Contains(searchTerm) || s.Employee.EmployeeId.Contains(searchTerm));
            }

            var records = await query.ToListAsync();

            var result = records.Select(s => new BankSheetDto
            {
                Id = s.Id,
                EmployeeId = s.Employee?.EmployeeId ?? "",
                CompanyId = s.Employee?.CompanyId ?? s.CompanyId ?? 0,
                EmployeeName = s.Employee?.FullNameEn ?? "",
                Department = s.Employee?.Department?.NameEn ?? "",
                BankName = s.Employee?.BankName ?? "",
                BankAccountNo = s.Employee?.BankAccountNo ?? "",
                BankBranchName = s.Employee?.BankBranchName ?? "",
                NetPayable = s.NetPayable,
                Status = s.Status,
                CompanyName = s.Employee?.Company?.CompanyNameEn ?? s.Company?.CompanyNameEn ?? ""
            }).ToList();

            return Ok(result);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<SalarySummaryDto>> GetSalarySummary(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] int? companyId)
        {
            var query = _context.MonthlySalarySheets
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Department)
                .Where(s => s.Year == year && s.Month == month);

            if (companyId.HasValue && companyId > 0)
            {
                query = query.Where(s => s.Employee!.CompanyId == companyId.Value || s.CompanyId == companyId.Value);
            }

            var records = await query.ToListAsync();

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
                .Include(s => s.Company)
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Company)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (s == null) return NotFound();

            var result = new PayslipDto
            {
                Id = s.Id,
                EmployeeId = s.Employee?.EmployeeId ?? "",
                CompanyId = s.Employee?.CompanyId ?? s.CompanyId ?? 0,
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
                BankAccountNo = s.Employee?.BankAccountNo ?? "N/A",
                CompanyName = s.Employee?.Company?.CompanyNameEn ?? s.Company?.CompanyNameEn ?? ""
            };

            return Ok(result);
        }

        [HttpPost("process")]
        public async Task<ActionResult> ProcessSalary([FromBody] SalaryProcessRequestDto request)
        {
            // Simple mock processing logic
            // In reality, this would query attendance, leaves, OT deductions, etc.

            var employeesQuery = _context.Employees.Where(e => e.Status == "Active");

            if (request.CompanyId.HasValue && request.CompanyId > 0)
                employeesQuery = employeesQuery.Where(e => e.CompanyId == request.CompanyId.Value);

            if (!string.IsNullOrEmpty(request.EmployeeId))
                employeesQuery = employeesQuery.Where(e => e.EmployeeId == request.EmployeeId);

            if (request.DepartmentId.HasValue)
                employeesQuery = employeesQuery.Where(e => e.DepartmentId == request.DepartmentId.Value);

            var employees = await employeesQuery.ToListAsync();

            var startDate = new DateTime(request.Year, request.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var existingSheets = await _context.MonthlySalarySheets
                .Where(s => s.Year == request.Year && s.Month == request.Month)
                .ToListAsync();

            var monthAttendances = await _context.Attendances
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .ToListAsync();

            var monthAdvances = await _context.AdvanceSalaries
                .Where(a => a.RepaymentMonth == request.Month && a.RepaymentYear == request.Year && a.Status == "Approved")
                .ToListAsync();

            foreach (var emp in employees)
            {
                var sheet = existingSheets.FirstOrDefault(s => s.EmployeeId == emp.Id) ?? new MonthlySalarySheet();
                var empAttendances = monthAttendances.Where(a => a.EmployeeCard == emp.Id).ToList();
                var empAdvances = monthAdvances.Where(a => a.EmployeeId == emp.Id).ToList();

                sheet.EmployeeId = emp.Id;
                sheet.CompanyId = emp.CompanyId;
                sheet.Year = request.Year;
                sheet.Month = request.Month;
                sheet.GrossSalary = emp.GrossSalary ?? 0;
                sheet.BasicSalary = emp.BasicSalary ?? 0;

                sheet.TotalDays = DateTime.DaysInMonth(request.Year, request.Month);
                sheet.PresentDays = empAttendances.Count(a => a.Status == "Present" || a.Status == "Late");
                sheet.AbsentDays = empAttendances.Count(a => a.Status == "Absent");
                sheet.LeaveDays = empAttendances.Count(a => a.Status == "On Leave");
                sheet.Holidays = empAttendances.Count(a => a.Status == "Holiday");
                sheet.WeekendDays = empAttendances.Count(a => a.Status == "Off Day");

                // Use actual OTHours from attendances (already calculated with 45-min rounding)
                sheet.OTHours = empAttendances.Sum(a => a.OTHours); 
                sheet.OTRate = (sheet.BasicSalary / 208) * 2; 
                sheet.OTAmount = sheet.OTHours * sheet.OTRate;

                sheet.AttendanceBonus = (sheet.AbsentDays == 0 && sheet.PresentDays > 0) ? 500 : 0;
                sheet.OtherAllowances = 1000;

                sheet.TotalEarning = sheet.GrossSalary + sheet.OTAmount + sheet.AttendanceBonus + sheet.OtherAllowances;

                decimal perDayGross = sheet.TotalDays > 0 ? sheet.GrossSalary / sheet.TotalDays : 0;
                sheet.AbsentDeduction = sheet.AbsentDays * perDayGross;
                sheet.AdvanceDeduction = empAdvances.Sum(a => a.Amount);
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
        public async Task<ActionResult<IEnumerable<AdvanceSalaryDto>>> GetAdvanceSalaries(
            [FromQuery] int? companyId,
            [FromQuery] int? month,
            [FromQuery] int? year)
        {
            var query = _context.AdvanceSalaries
                .Include(a => a.Employee)
                .ThenInclude(e => e!.Company)
                .Include(a => a.Company)
                .AsQueryable();

            if (companyId.HasValue && companyId > 0)
                query = query.Where(a => a.Employee!.CompanyId == companyId.Value || a.CompanyId == companyId.Value);

            if (month.HasValue) query = query.Where(a => a.RepaymentMonth == month.Value);
            if (year.HasValue) query = query.Where(a => a.RepaymentYear == year.Value);

            var records = await query.ToListAsync();
            
            var m = month ?? DateTime.Now.Month;
            var y = year ?? DateTime.Now.Year;
            
            var startOfMonth = new DateTime(y, m, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var employeeIds = records.Select(r => r.EmployeeId).Distinct().ToList();

            var attendances = await _context.Attendances
                .Where(a => employeeIds.Contains(a.EmployeeCard) && a.Date.Date >= startOfMonth && a.Date.Date <= endOfMonth)
                .ToListAsync();

            return Ok(records.Select(a => {
                var empAttendances = attendances.Where(att => att.EmployeeCard == a.Employee?.Id).ToList();
                int currentPresent = empAttendances.Count(att => att.Status == "Present" || att.Status == "Late");
                int currentAbsent = empAttendances.Count(att => att.Status == "Absent");

                return new AdvanceSalaryDto
                {
                    Id = a.Id,
                    EmployeeId = a.Employee?.EmployeeId ?? "",
                    CompanyId = a.Employee?.CompanyId ?? a.CompanyId ?? 0,
                    EmployeeName = a.Employee?.FullNameEn ?? "",
                    Designation = a.Employee?.Designation?.NameEn ?? "",
                    JoiningDate = a.Employee?.JoinDate,
                    Grade = a.Grade ?? a.Employee?.Grade ?? "N/A",
                    Amount = a.Amount,
                    RequestDate = a.RequestDate,
                    RepaymentMonth = a.RepaymentMonth,
                    RepaymentYear = a.RepaymentYear,
                    Status = a.Status,
                    Remarks = a.Remarks,
                    CompanyName = a.Employee?.Company?.CompanyNameEn ?? a.Company?.CompanyNameEn ?? "",
                    
                    BasicSalary = a.BasicSalary > 0 ? a.BasicSalary : (a.Employee?.BasicSalary ?? 0),
                    HouseRent = a.HouseRent > 0 ? a.HouseRent : (a.Employee?.HouseRent ?? 0),
                    MedicalAllowance = a.MedicalAllowance > 0 ? a.MedicalAllowance : (a.Employee?.MedicalAllowance ?? 0),
                    FoodAllowance = a.FoodAllowance > 0 ? a.FoodAllowance : (a.Employee?.FoodAllowance ?? 0),
                    TransportAllowance = a.TransportAllowance > 0 ? a.TransportAllowance : (a.Employee?.Conveyance ?? 0),
                    GrossSalary = a.GrossSalary > 0 ? a.GrossSalary : (a.Employee?.GrossSalary ?? 0),
                    
                    PresentDays = a.PresentDays > 0 ? a.PresentDays : currentPresent,
                    AbsentDays = a.AbsentDays > 0 ? a.AbsentDays : currentAbsent,
                    
                    AbsentDeduction = a.AbsentDeduction,
                    TotalPayableWages = a.TotalPayableWages,
                    
                    OTHours = a.OTHours,
                    OTRate = a.OTRate,
                    OTAmount = a.OTAmount,
                    
                    BankAccountNo = a.Employee?.BankAccountNo,
                    PaymentMethod = a.Employee?.BankName ?? "Cash",
                    NetPayable = a.NetPayable > 0 ? a.NetPayable : a.Amount
                };
            }));
        }

        [HttpPost("advance-salary")]
        public async Task<ActionResult> CreateAdvanceSalary([FromBody] CreateAdvanceSalaryDto dto)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == dto.EmployeeId && e.CompanyId == dto.CompanyId);

            if (employee == null) return NotFound("Employee not found");

            // Calculate current month's attendance to date
            var startOfMonth = new DateTime(dto.RepaymentYear, dto.RepaymentMonth, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            if (endOfMonth > DateTime.Now) endOfMonth = DateTime.Now;

            var empAttendances = await _context.Attendances
                .Where(a => a.EmployeeCard == employee.Id && a.Date >= startOfMonth && a.Date <= endOfMonth)
                .ToListAsync();

            int presentDays = empAttendances.Count(a => a.Status == "Present" || a.Status == "Late");
            int absentDays = empAttendances.Count(a => a.Status == "Absent");
            decimal otHours = empAttendances.Sum(a => a.OTHours);

            int totalDaysInMonth = DateTime.DaysInMonth(dto.RepaymentYear, dto.RepaymentMonth);
            decimal gross = employee.GrossSalary ?? 0;
            decimal perDay = totalDaysInMonth > 0 ? (gross / totalDaysInMonth) : 0;
            
            decimal basic = employee.BasicSalary ?? 0;
            decimal otRate = totalDaysInMonth > 0 ? (basic / 208) * 2 : 0;
            decimal otAmount = Math.Round(otHours * otRate, 2);
            
            decimal absentDeduction = Math.Round(absentDays * perDay, 2);
            decimal totalPayableWages = Math.Round(gross - absentDeduction, 2);
            decimal netPayable = Math.Round(totalPayableWages + otAmount, 2);

            var advance = new AdvanceSalary
            {
                EmployeeId = employee.Id,
                CompanyId = employee.CompanyId,
                Amount = dto.Amount,
                RequestDate = dto.RequestDate,
                RepaymentMonth = dto.RepaymentMonth,
                RepaymentYear = dto.RepaymentYear,
                Remarks = dto.Remarks,
                Status = "Approved",
                Grade = employee.Grade,
                
                BasicSalary = basic,
                HouseRent = employee.HouseRent ?? 0,
                MedicalAllowance = employee.MedicalAllowance ?? 0,
                FoodAllowance = employee.FoodAllowance ?? 0,
                TransportAllowance = employee.Conveyance ?? 0,
                GrossSalary = gross,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                AbsentDeduction = absentDeduction,
                TotalPayableWages = totalPayableWages,
                OTHours = otHours,
                OTRate = otRate,
                OTAmount = otAmount,
                NetPayable = netPayable
            };

            _context.AdvanceSalaries.Add(advance);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Advance salary request created." });
        }

        [HttpPost("batch-advance-salary")]
        public async Task<ActionResult> BatchCreateAdvanceSalary([FromBody] BatchCreateAdvanceSalaryDto dto)
        {
            if (dto.EmployeeIds == null || !dto.EmployeeIds.Any())
                return BadRequest("No employees selected");

            var employees = await _context.Employees
                .Include(e => e.Shift)
                .Where(e => dto.EmployeeIds.Contains(e.EmployeeId) && e.CompanyId == dto.CompanyId)
                .ToListAsync();

            if (!employees.Any()) return NotFound("No employees found matching the criteria");

            var employeeIdsList = employees.Select(e => e.Id).ToList();
            var existingAdvances = await _context.AdvanceSalaries
                .Where(a => employeeIdsList.Contains(a.EmployeeId) && a.RepaymentMonth == dto.RepaymentMonth && a.RepaymentYear == dto.RepaymentYear)
                .ToListAsync();

            int createdCount = 0;
            int updatedCount = 0;

            foreach (var employee in employees)
            {
                decimal advanceAmount = dto.Amount;
                
                // Fetch attendance for the period if date range is provided
                int presentDays = 0;
                int absentDays = 0;
                decimal otHours = 0;
                
                if (dto.IsDateRange && dto.FromDate.HasValue && dto.ToDate.HasValue)
                {
                    var empAttendances = await _context.Attendances
                        .Include(a => a.Shift)
                        .Where(a => a.EmployeeCard == employee.Id && a.Date >= dto.FromDate.Value && a.Date <= dto.ToDate.Value)
                        .ToListAsync();
                    
                    int totalRangeDays = (dto.ToDate.Value - dto.FromDate.Value).Days + 1;
                    absentDays = empAttendances.Count(a => a.Status == "Absent");
                    presentDays = totalRangeDays - absentDays; 

                    // Calculate OT with 45-min rule
                    foreach (var att in empAttendances)
                    {
                        if (employee.IsOtEnabled && att.InTime.HasValue && att.OutTime.HasValue)
                        {
                            var s = att.Shift ?? employee.Shift;
                            if (s != null && TimeSpan.TryParse(s.OutTime, out var sOut))
                            {
                                var limit = att.Date.Date.Add(sOut);
                                if (s.ActualInTime != null && TimeSpan.TryParse(s.ActualInTime, out var aIn) && sOut < aIn) 
                                    limit = limit.AddDays(1);

                                if (att.OutTime.Value > limit)
                                {
                                    double mins = (att.OutTime.Value - limit).TotalMinutes;
                                    int h = (int)(mins / 60);
                                    if ((mins % 60) >= 45) h++;
                                    otHours += h;
                                }
                            }
                        }
                    }
                }
                else
                {
                    var startOfMonth = new DateTime(dto.RepaymentYear, dto.RepaymentMonth, 1);
                    var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                    
                    var empAttendances = await _context.Attendances
                        .Include(a => a.Shift)
                        .Where(a => a.EmployeeCard == employee.Id && a.Date >= startOfMonth && a.Date <= endOfMonth)
                        .ToListAsync();
                        
                    absentDays = empAttendances.Count(a => a.Status == "Absent");
                    int totalMonthDays = DateTime.DaysInMonth(dto.RepaymentYear, dto.RepaymentMonth);
                    presentDays = totalMonthDays - absentDays;

                    foreach (var att in empAttendances)
                    {
                        if (employee.IsOtEnabled && att.InTime.HasValue && att.OutTime.HasValue)
                        {
                            var s = att.Shift ?? employee.Shift;
                            if (s != null && TimeSpan.TryParse(s.OutTime, out var sOut))
                            {
                                var limit = att.Date.Date.Add(sOut);
                                if (s.ActualInTime != null && TimeSpan.TryParse(s.ActualInTime, out var aIn) && sOut < aIn) 
                                    limit = limit.AddDays(1);

                                if (att.OutTime.Value > limit)
                                {
                                    double mins = (att.OutTime.Value - limit).TotalMinutes;
                                    int h = (int)(mins / 60);
                                    if ((mins % 60) >= 45) h++;
                                    otHours += h;
                                }
                            }
                        }
                    }
                }

                int daysInMonth = DateTime.DaysInMonth(dto.RepaymentYear, dto.RepaymentMonth);
                decimal gross = employee.GrossSalary ?? 0;
                decimal perDay = daysInMonth > 0 ? (gross / daysInMonth) : 0;
                
                decimal basic = employee.BasicSalary ?? 0;
                decimal otRate = daysInMonth > 0 ? (basic / 208) * 2 : 0; 
                decimal otAmount = Math.Round(otHours * otRate, 2);
                
                decimal absentDeduction = Math.Round(absentDays * perDay, 2);
                decimal totalPayableWages = Math.Round(presentDays * perDay, 2); // Calculate based on present days
                decimal netPayable = Math.Round(totalPayableWages + otAmount, 2);

                // Use calculated netPayable as the advance amount
                advanceAmount = netPayable;

                var existingAdvance = existingAdvances.FirstOrDefault(a => a.EmployeeId == employee.Id);

                if (existingAdvance != null)
                {
                    existingAdvance.Amount = advanceAmount;
                    existingAdvance.RequestDate = dto.RequestDate;
                    existingAdvance.Remarks = dto.Remarks;
                    existingAdvance.Grade = employee.Grade;
                    
                    existingAdvance.BasicSalary = basic;
                    existingAdvance.HouseRent = employee.HouseRent ?? 0;
                    existingAdvance.MedicalAllowance = employee.MedicalAllowance ?? 0;
                    existingAdvance.FoodAllowance = employee.FoodAllowance ?? 0;
                    existingAdvance.TransportAllowance = employee.Conveyance ?? 0;
                    existingAdvance.GrossSalary = gross;
                    existingAdvance.PresentDays = presentDays;
                    existingAdvance.AbsentDays = absentDays;
                    existingAdvance.AbsentDeduction = absentDeduction;
                    existingAdvance.TotalPayableWages = totalPayableWages;
                    existingAdvance.OTHours = otHours;
                    existingAdvance.OTRate = otRate;
                    existingAdvance.OTAmount = otAmount;
                    existingAdvance.NetPayable = netPayable;
                    
                    _context.AdvanceSalaries.Update(existingAdvance);
                    updatedCount++;
                }
                else
                {
                    var advance = new AdvanceSalary
                    {
                        EmployeeId = employee.Id,
                        CompanyId = employee.CompanyId,
                        Amount = advanceAmount,
                        RequestDate = dto.RequestDate,
                        RepaymentMonth = dto.RepaymentMonth,
                        RepaymentYear = dto.RepaymentYear,
                        Remarks = dto.Remarks,
                        Status = "Approved",
                        Grade = employee.Grade,
                        
                        BasicSalary = basic,
                        HouseRent = employee.HouseRent ?? 0,
                        MedicalAllowance = employee.MedicalAllowance ?? 0,
                        FoodAllowance = employee.FoodAllowance ?? 0,
                        TransportAllowance = employee.Conveyance ?? 0,
                        GrossSalary = gross,
                        PresentDays = presentDays,
                        AbsentDays = absentDays,
                        AbsentDeduction = absentDeduction,
                        TotalPayableWages = totalPayableWages,
                        OTHours = otHours,
                        OTRate = otRate,
                        OTAmount = otAmount,
                        NetPayable = netPayable
                    };
                    _context.AdvanceSalaries.Add(advance);
                    createdCount++;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Successfully processed advance salary requests: {createdCount} created, {updatedCount} updated." });
        }

        [HttpPost("batch-delete-advance-salary")]
        public async Task<ActionResult> BatchDeleteAdvanceSalary([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("No records selected");

            var records = await _context.AdvanceSalaries
                .Where(a => ids.Contains(a.Id))
                .ToListAsync();

            if (!records.Any()) return NotFound("No records found to delete");

            _context.AdvanceSalaries.RemoveRange(records);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Successfully deleted {records.Count} advance salary records." });
        }

        [HttpGet("advance-salary-summary")]
        public async Task<ActionResult<AdvanceSalarySummaryDto>> GetAdvanceSalarySummary(
            [FromQuery] int? companyId,
            [FromQuery] int? month,
            [FromQuery] int? year)
        {
            var query = _context.AdvanceSalaries
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Department)
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Section)
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Line)
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Designation)
                .AsQueryable();

            if (companyId.HasValue && companyId > 0)
                query = query.Where(a => a.Employee!.CompanyId == companyId.Value || a.CompanyId == companyId.Value);

            if (month.HasValue) query = query.Where(a => a.RepaymentMonth == month.Value);
            if (year.HasValue) query = query.Where(a => a.RepaymentYear == year.Value);

            var records = await query.ToListAsync();

            if (!records.Any()) return Ok(new AdvanceSalarySummaryDto());

            var summary = new AdvanceSalarySummaryDto
            {
                TotalAdvanceDisbursed = records.Where(r => r.Status == "Approved").Sum(r => r.Amount),
                TotalPendingRequests = records.Count(r => r.Status == "Pending"),
                TotalPendingAmount = records.Where(r => r.Status == "Pending").Sum(r => r.Amount),
                TotalRepaid = records.Where(r => r.Status == "Paid" || r.Status == "Completed").Sum(r => r.Amount),
                TotalEmployees = records.Select(r => r.EmployeeId).Distinct().Count(),
                DepartmentSummaries = records
                    .GroupBy(r => r.Employee?.Department?.NameEn ?? "Unknown")
                    .Select(g => new DepartmentAdvanceSummaryDto
                    {
                        DepartmentName = g.Key,
                        EmployeeCount = g.Select(x => x.EmployeeId).Distinct().Count(),
                        BasicSalary = g.Sum(x => x.BasicSalary),
                        GrossSalary = g.Sum(x => x.GrossSalary),
                        AbsentDays = g.Sum(x => x.AbsentDays),
                        AbsentDeduction = g.Sum(x => x.AbsentDeduction),
                        TotalPayableWages = g.Sum(x => x.TotalPayableWages),
                        OTHours = g.Sum(x => x.OTHours),
                        OTAmount = g.Sum(x => x.OTAmount),
                        NetPayable = g.Sum(x => x.NetPayable)
                    }).OrderByDescending(x => x.NetPayable).ToList(),
                SectionSummaries = records
                    .GroupBy(r => r.Employee?.Section?.NameEn ?? "Unknown")
                    .Select(g => new SectionAdvanceSummaryDto
                    {
                        SectionName = g.Key,
                        EmployeeCount = g.Select(x => x.EmployeeId).Distinct().Count(),
                        BasicSalary = g.Sum(x => x.BasicSalary),
                        GrossSalary = g.Sum(x => x.GrossSalary),
                        AbsentDays = g.Sum(x => x.AbsentDays),
                        AbsentDeduction = g.Sum(x => x.AbsentDeduction),
                        TotalPayableWages = g.Sum(x => x.TotalPayableWages),
                        OTHours = g.Sum(x => x.OTHours),
                        OTAmount = g.Sum(x => x.OTAmount),
                        NetPayable = g.Sum(x => x.NetPayable)
                    }).OrderByDescending(x => x.NetPayable).ToList(),
                LineSummaries = records
                    .GroupBy(r => r.Employee?.Line?.NameEn ?? "Unknown")
                    .Select(g => new LineAdvanceSummaryDto
                    {
                        LineName = g.Key,
                        EmployeeCount = g.Select(x => x.EmployeeId).Distinct().Count(),
                        BasicSalary = g.Sum(x => x.BasicSalary),
                        GrossSalary = g.Sum(x => x.GrossSalary),
                        AbsentDays = g.Sum(x => x.AbsentDays),
                        AbsentDeduction = g.Sum(x => x.AbsentDeduction),
                        TotalPayableWages = g.Sum(x => x.TotalPayableWages),
                        OTHours = g.Sum(x => x.OTHours),
                        OTAmount = g.Sum(x => x.OTAmount),
                        NetPayable = g.Sum(x => x.NetPayable)
                    }).OrderByDescending(x => x.NetPayable).ToList(),
                DesignationSummaries = records
                    .GroupBy(r => r.Employee?.Designation?.NameEn ?? "Unknown")
                    .Select(g => new DesignationAdvanceSummaryDto
                    {
                        DesignationName = g.Key,
                        EmployeeCount = g.Select(x => x.EmployeeId).Distinct().Count(),
                        BasicSalary = g.Sum(x => x.BasicSalary),
                        GrossSalary = g.Sum(x => x.GrossSalary),
                        AbsentDays = g.Sum(x => x.AbsentDays),
                        AbsentDeduction = g.Sum(x => x.AbsentDeduction),
                        TotalPayableWages = g.Sum(x => x.TotalPayableWages),
                        OTHours = g.Sum(x => x.OTHours),
                        OTAmount = g.Sum(x => x.OTAmount),
                        NetPayable = g.Sum(x => x.NetPayable)
                    }).OrderByDescending(x => x.NetPayable).ToList()
            };

            return Ok(summary);
        }

        [HttpGet("export-advance-salary-summary")]
        public async Task<IActionResult> ExportAdvanceSalarySummary(
            [FromQuery] int? companyId,
            [FromQuery] int? month,
            [FromQuery] int? year)
        {
            var query = _context.AdvanceSalaries
                .Include(a => a.Employee).ThenInclude(e => e!.Department)
                .Include(a => a.Employee).ThenInclude(e => e!.Section)
                .Include(a => a.Employee).ThenInclude(e => e!.Line)
                .Include(a => a.Employee).ThenInclude(e => e!.Designation)
                .AsQueryable();

            if (companyId.HasValue && companyId > 0)
                query = query.Where(a => a.Employee!.CompanyId == companyId.Value || a.CompanyId == companyId.Value);

            if (month.HasValue) query = query.Where(a => a.RepaymentMonth == month.Value);
            if (year.HasValue) query = query.Where(a => a.RepaymentYear == year.Value);

            var records = await query.ToListAsync();
            var company = companyId > 0 ? await _context.Companies.FindAsync(companyId) : await _context.Companies.FirstOrDefaultAsync();

            using var package = new ExcelPackage();

            // Helper to add a summary sheet
            void AddSummarySheet(string sheetName, IEnumerable<IGrouping<string, ERPBackend.Core.Models.AdvanceSalary>> groups, string categoryLabel)
            {
                var ws = package.Workbook.Worksheets.Add(sheetName);
                ws.Cells.Style.Font.Name = "Arial";
                ws.Cells.Style.Font.Size = 10;

                // Header
                ws.Cells[1, 1, 1, 11].Merge = true;
                ws.Cells[1, 1].Value = company?.CompanyNameEn ?? "HR HUB";
                ws.Cells[1, 1].Style.Font.Size = 14;
                ws.Cells[1, 1].Style.Font.Bold = true;
                ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells[2, 1, 2, 11].Merge = true;
                ws.Cells[2, 1].Value = $"ADVANCE SALARY SUMMARY BY {sheetName.ToUpper()}";
                ws.Cells[2, 1].Style.Font.Bold = true;
                ws.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells[3, 1, 3, 11].Merge = true;
                ws.Cells[3, 1].Value = $"FOR {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month ?? 1).ToUpper()} {year}";
                ws.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                string[] headers = { "SL", categoryLabel, "Emp", "Basic", "Gross", "Abs", "Abs.Ded", "Payable", "OT Hr", "OT Pay", "Net Pay" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cells[5, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                int rowValue = 6;
                int sl = 1;
                foreach (var g in groups.OrderByDescending(x => x.Sum(r => r.NetPayable)))
                {
                    ws.Cells[rowValue, 1].Value = sl++;
                    ws.Cells[rowValue, 2].Value = g.Key;
                    ws.Cells[rowValue, 3].Value = g.Select(x => x.EmployeeId).Distinct().Count();
                    ws.Cells[rowValue, 4].Value = g.Sum(x => x.BasicSalary);
                    ws.Cells[rowValue, 5].Value = g.Sum(x => x.GrossSalary);
                    ws.Cells[rowValue, 6].Value = g.Sum(x => x.AbsentDays);
                    ws.Cells[rowValue, 7].Value = g.Sum(x => x.AbsentDeduction);
                    ws.Cells[rowValue, 8].Value = g.Sum(x => x.TotalPayableWages);
                    ws.Cells[rowValue, 9].Value = g.Sum(x => x.OTHours);
                    ws.Cells[rowValue, 10].Value = g.Sum(x => x.OTAmount);
                    ws.Cells[rowValue, 11].Value = g.Sum(x => x.NetPayable);

                    // Formatting
                    ws.Cells[rowValue, 4, rowValue, 11].Style.Numberformat.Format = "#,##0.00";
                    for (int i = 1; i <= 11; i++)
                    {
                        ws.Cells[rowValue, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    rowValue++;
                }

                // Totals
                ws.Cells[rowValue, 1, rowValue, 2].Merge = true;
                ws.Cells[rowValue, 1].Value = "GRAND TOTAL";
                ws.Cells[rowValue, 1].Style.Font.Bold = true;
                ws.Cells[rowValue, 3].Formula = $"SUM(C6:C{rowValue - 1})";
                ws.Cells[rowValue, 4].Formula = $"SUM(D6:D{rowValue - 1})";
                ws.Cells[rowValue, 5].Formula = $"SUM(E6:E{rowValue - 1})";
                ws.Cells[rowValue, 6].Formula = $"SUM(F6:F{rowValue - 1})";
                ws.Cells[rowValue, 7].Formula = $"SUM(G6:G{rowValue - 1})";
                ws.Cells[rowValue, 8].Formula = $"SUM(H6:H{rowValue - 1})";
                ws.Cells[rowValue, 9].Formula = $"SUM(I6:I{rowValue - 1})";
                ws.Cells[rowValue, 10].Formula = $"SUM(J6:J{rowValue - 1})";
                ws.Cells[rowValue, 11].Formula = $"SUM(K6:K{rowValue - 1})";
                ws.Cells[rowValue, 3, rowValue, 11].Style.Font.Bold = true;
                ws.Cells[rowValue, 4, rowValue, 11].Style.Numberformat.Format = "#,##0.00";
                for (int i = 3; i <= 11; i++) ws.Cells[rowValue, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                ws.Cells.AutoFitColumns();
            }

            AddSummarySheet("Department", records.GroupBy(r => r.Employee?.Department?.NameEn ?? "Unknown"), "Department Name");
            AddSummarySheet("Section", records.GroupBy(r => r.Employee?.Section?.NameEn ?? "Unknown"), "Section Name");
            AddSummarySheet("Line", records.GroupBy(r => r.Employee?.Line?.NameEn ?? "Unknown"), "Line Name");
            AddSummarySheet("Designation", records.GroupBy(r => r.Employee?.Designation?.NameEn ?? "Unknown"), "Designation Title");

            var fileContent = package.GetAsByteArray();
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"Advance_Salary_Summary_{month}_{year}.xlsx");
        }

        [HttpGet("export-advance-salary-bank-sheet")]
        public async Task<IActionResult> ExportAdvanceSalaryBankSheet(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] int? companyId,
            [FromQuery] int? departmentId,
            [FromQuery] string? searchTerm)
        {
            var query = _context.AdvanceSalaries
                .Include(b => b.Employee).ThenInclude(e => e!.Department)
                .Include(b => b.Employee).ThenInclude(e => e!.Designation)
                .Include(b => b.Employee).ThenInclude(e => e!.Group)
                .Where(b => b.RepaymentYear == year && b.RepaymentMonth == month && b.Employee != null && b.Status == "Approved");

            if (companyId.HasValue && companyId > 0)
                query = query.Where(b => b.Employee!.CompanyId == companyId.Value || b.CompanyId == companyId.Value);

            if (departmentId.HasValue)
                query = query.Where(b => b.Employee!.DepartmentId == departmentId.Value);

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(b => b.Employee!.FullNameEn.Contains(searchTerm) || b.Employee!.EmployeeId.Contains(searchTerm));

            var allRecords = await query.ToListAsync();

            using var package = new ExcelPackage();

            // Define classification logic
            bool IsmCash(Employee? e)
            {
                if (e == null) return false;
                var bankName = e.BankName ?? "";
                var accountType = e.BankAccountType ?? "";
                var accountNo = e.BankAccountNo ?? "";

                if (accountType.Equals("mCash", StringComparison.OrdinalIgnoreCase) && accountNo.StartsWith("01"))
                    return true;

                if (bankName.Contains("Nagad", StringComparison.OrdinalIgnoreCase) ||
                    bankName.Contains("Rocket", StringComparison.OrdinalIgnoreCase) ||
                    bankName.Contains("Bkash", StringComparison.OrdinalIgnoreCase) ||
                    bankName.Contains("mCash", StringComparison.OrdinalIgnoreCase) ||
                    bankName.Contains("Upay", StringComparison.OrdinalIgnoreCase))
                    return true;

                return false;
            }

            bool IsCard(Employee? e)
            {
                if (e == null) return false;
                var accountType = e.BankAccountType ?? "";
                var accountNo = e.BankAccountNo ?? "";

                var cardTypes = new[] { "Card", "Bank", "Savings", "Current", "Salary" };
                if (cardTypes.Any(t => accountType.Equals(t, StringComparison.OrdinalIgnoreCase)))
                    return true;

                if (accountNo.StartsWith("2050"))
                    return true;

                return false;
            }

            bool IsStaff(Employee? e) => e?.Group?.NameEn?.Contains("Staff", StringComparison.OrdinalIgnoreCase) == true;

            var activeRecords = allRecords.Where(s => s.Employee!.Status == "Active" && s.Employee.IsActive).ToList();

            var staffMcash = activeRecords.Where(s => IsStaff(s.Employee) && IsmCash(s.Employee)).Select(s => new MonthlySalarySheet { Employee = s.Employee, NetPayable = s.Amount }).ToList();
            var staffCard = activeRecords.Where(s => IsStaff(s.Employee) && IsCard(s.Employee)).Select(s => new MonthlySalarySheet { Employee = s.Employee, NetPayable = s.Amount }).ToList();
            var workerMcash = activeRecords.Where(s => !IsStaff(s.Employee) && IsmCash(s.Employee)).Select(s => new MonthlySalarySheet { Employee = s.Employee, NetPayable = s.Amount }).ToList();
            var workerCard = activeRecords.Where(s => !IsStaff(s.Employee) && IsCard(s.Employee)).Select(s => new MonthlySalarySheet { Employee = s.Employee, NetPayable = s.Amount }).ToList();

            // 1. Summary Sheet
            var summarySheet = package.Workbook.Worksheets.Add("Summary");
            CreateSummarySheet(summarySheet, year, month, staffMcash, staffCard, workerMcash, workerCard, new List<MonthlySalarySheet>(), new List<MonthlySalarySheet>());

            // 2-5. Individual Sheets
            AddDataSheet(package, "Staff - mCash", staffMcash);
            AddDataSheet(package, "Staff - Card", staffCard);
            AddDataSheet(package, "Worker - mCash", workerMcash);
            AddDataSheet(package, "Worker - Card", workerCard);

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Advance_Salary_Bank_Payment_{monthName}_{year}.xlsx");
        }

        [HttpGet("export-advance-salary-sheet")]
        public async Task<IActionResult> ExportAdvanceSalarySheet(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] int? companyId,
            [FromQuery] int? departmentId,
            [FromQuery] string? searchTerm)
        {
            var query = _context.AdvanceSalaries
                .Include(a => a.Employee).ThenInclude(e => e!.Department)
                .Include(a => a.Employee).ThenInclude(e => e!.Designation)
                .Where(a => a.RepaymentYear == year && a.RepaymentMonth == month);

            if (companyId.HasValue && companyId > 0)
                query = query.Where(a => a.Employee!.CompanyId == companyId.Value || a.CompanyId == companyId.Value);

            if (departmentId.HasValue)
                query = query.Where(a => a.Employee!.DepartmentId == departmentId.Value);

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(a =>
                    a.Employee!.FullNameEn.Contains(searchTerm) || a.Employee.EmployeeId.Contains(searchTerm));

            var records = await query.OrderBy(a => a.Employee!.EmployeeId).ToListAsync();

            var company = companyId > 0 ? await _context.Companies.FindAsync(companyId) : await _context.Companies.FirstOrDefaultAsync();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Advance Salary Sheet");

            // Page Setup
            ws.PrinterSettings.Orientation = eOrientation.Landscape;
            ws.PrinterSettings.PaperSize = ePaperSize.Legal;
            ws.PrinterSettings.FitToPage = true;
            ws.PrinterSettings.FitToWidth = 1;
            ws.PrinterSettings.FitToHeight = 0;

            ws.Cells.Style.Font.Name = "Arial";
            ws.Cells.Style.Font.Size = 9;

            // Company Header
            int colCount = 23;
            ws.Cells[1, 1, 1, colCount].Merge = true;
            ws.Cells[1, 1].Value = company?.CompanyNameEn ?? "HR HUB";
            ws.Cells[1, 1].Style.Font.Size = 18;
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            ws.Cells[2, 1, 2, colCount].Merge = true;
            ws.Cells[2, 1].Value = company?.AddressEn ?? "Industrial Area, Dhaka, Bangladesh";
            ws.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            ws.Cells[3, 1, 3, colCount].Merge = true;
            ws.Cells[3, 1].Value = $"ADVANCE SALARY SHEET FOR THE MONTH OF {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month).ToUpper()} {year}";
            ws.Cells[3, 1].Style.Font.Size = 12;
            ws.Cells[3, 1].Style.Font.Bold = true;
            ws.Cells[3, 1].Style.Font.UnderLine = true;
            ws.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Headers
            string[] headers = { 
                "SL.No", "Emp.ID", "Name", "Designation", "Joining Date", "Grade", 
                "Basic", "House Rent", "Medical", "Food", "Transport", "Gross", 
                "Ways", "Abs", "Day", "Absent Deduction", "Total Payable Wages", 
                "OT Hr", "OT Rate", "OT payable", "Bank Account /Bkash", "Net Payble", "Signature" 
            };

            int headerRow = 5;
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cells[headerRow, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(31, 73, 125)); // Dark Blue
                cell.Style.Font.Color.SetColor(Color.White);
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
            ws.Row(headerRow).Height = 25;

            int row = 6;
            foreach (var a in records)
            {
                ws.Cells[row, 1].Value = row - 1; 
                ws.Cells[row, 2].Value = a.Employee?.EmployeeId;
                ws.Cells[row, 3].Value = a.Employee?.FullNameEn;
                ws.Cells[row, 4].Value = a.Employee?.Designation?.NameEn;
                ws.Cells[row, 5].Value = a.Employee?.JoinDate.ToString("dd MMM yyyy");
                ws.Cells[row, 6].Value = a.Grade ?? a.Employee?.Grade;

                // Data Columns
                ws.Cells[row, 7].Value = a.BasicSalary > 0 ? a.BasicSalary : a.Employee?.BasicSalary ?? 0;
                ws.Cells[row, 8].Value = a.HouseRent > 0 ? a.HouseRent : a.Employee?.HouseRent ?? 0;
                ws.Cells[row, 9].Value = a.MedicalAllowance > 0 ? a.MedicalAllowance : a.Employee?.MedicalAllowance ?? 0;
                ws.Cells[row, 10].Value = a.FoodAllowance > 0 ? a.FoodAllowance : a.Employee?.FoodAllowance ?? 0;
                ws.Cells[row, 11].Value = a.TransportAllowance > 0 ? a.TransportAllowance : a.Employee?.Conveyance ?? 0;

                // Formula Columns
                ws.Cells[row, 12].Formula = $"SUM(G{row}:K{row})"; // L (Gross)
                
                int recordDaysInMonth = DateTime.DaysInMonth(a.RepaymentYear, a.RepaymentMonth);
                ws.Cells[row, 13].Value = a.PresentDays + a.AbsentDays; // M (Ways - Range duration)
                ws.Cells[row, 14].Value = a.AbsentDays; // N (Abs)
                ws.Cells[row, 15].Formula = $"M{row}-N{row}"; // O (Day - Present)
                
                ws.Cells[row, 16].Formula = $"(L{row}/{recordDaysInMonth})*N{row}"; // P (Absent Deduction using month rate)
                ws.Cells[row, 17].Formula = $"(L{row}/{recordDaysInMonth})*O{row}"; // Q (Total Payable Wages for range)
                
                ws.Cells[row, 18].Value = a.OTHours; // R (OT Hr)
                ws.Cells[row, 19].Formula = $"IF(208>0, (G{row}/208)*2, 0)"; // S (OT Rate)
                ws.Cells[row, 20].Formula = $"R{row}*S{row}"; // T (OT payable)
                
                ws.Cells[row, 21].Value = a.Employee?.BankAccountNo; // U (Bank Account /Bkash)
                ws.Cells[row, 22].Formula = $"Q{row}+T{row}"; // V (Net Payble)

                // Style numbers
                for (int i = 7; i <= 12; i++) ws.Cells[row, i].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 16].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 17].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 19].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 20].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 22].Style.Numberformat.Format = "#,##0.00";

                for (int i = 1; i <= 23; i++) ws.Cells[row, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                row++;
            }

            ws.Cells.AutoFitColumns();
            ws.Column(1).Width = 5;
            ws.Column(2).Width = 10;
            ws.Column(3).Width = 25;
            ws.Column(4).Width = 20;
            ws.Column(21).Width = 20;
            ws.Column(23).Width = 15;

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Advance_Salary_Sheet_{monthName}_{year}.xlsx");
        }

        [HttpGet("increments")]
        public async Task<ActionResult<IEnumerable<SalaryIncrementDto>>> GetIncrements([FromQuery] int? companyId)
        {
            var query = _context.SalaryIncrements
                .Include(i => i.Employee)
                .ThenInclude(e => e!.Company)
                .Include(i => i.Company)
                .AsQueryable();

            if (companyId.HasValue && companyId > 0)
                query = query.Where(i => i.Employee!.CompanyId == companyId.Value || i.CompanyId == companyId.Value);

            var records = await query.OrderByDescending(i => i.EffectiveDate).ToListAsync();

            return Ok(records.Select(i => new SalaryIncrementDto
            {
                Id = i.Id,
                EmployeeId = i.Employee?.EmployeeId ?? "",
                CompanyId = i.Employee?.CompanyId ?? i.CompanyId ?? 0,
                EmployeeName = i.Employee?.FullNameEn ?? "",
                PreviousGrossSalary = i.PreviousGrossSalary,
                IncrementAmount = i.IncrementAmount,
                NewGrossSalary = i.NewGrossSalary,
                EffectiveDate = i.EffectiveDate,
                IncrementType = i.IncrementType,
                IsApplied = i.IsApplied,
                CompanyName = i.Employee?.Company?.CompanyNameEn ?? i.Company?.CompanyNameEn ?? ""
            }));
        }

        [HttpPost("increment")]
        public async Task<ActionResult> CreateIncrement([FromBody] CreateSalaryIncrementDto dto)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == dto.EmployeeId && e.CompanyId == dto.CompanyId);

            if (employee == null) return NotFound("Employee not found");

            var increment = new SalaryIncrement
            {
                EmployeeId = employee.Id,
                CompanyId = employee.CompanyId,
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
        public async Task<ActionResult<IEnumerable<BonusDto>>> GetBonuses(
            [FromQuery] int? companyId,
            [FromQuery] int? year,
            [FromQuery] int? month)
        {
            var query = _context.Bonuses
                .Include(b => b.Employee)
                .ThenInclude(e => e!.Company)
                .Include(b => b.Company)
                .AsQueryable();

            if (companyId.HasValue && companyId > 0)
                query = query.Where(b => b.Employee!.CompanyId == companyId.Value || b.CompanyId == companyId.Value);

            if (year.HasValue) query = query.Where(b => b.Year == year.Value);
            if (month.HasValue) query = query.Where(b => b.Month == month.Value);

            var records = await query.ToListAsync();
            return Ok(records.Select(b => new BonusDto
            {
                Id = b.Id,
                EmployeeId = b.Employee?.EmployeeId ?? "",
                CompanyId = b.Employee?.CompanyId ?? b.CompanyId ?? 0,
                EmployeeName = b.Employee?.FullNameEn ?? "",
                BonusType = b.BonusType,
                Amount = b.Amount,
                Year = b.Year,
                Month = b.Month,
                Status = b.Status,
                CompanyName = b.Employee?.Company?.CompanyNameEn ?? b.Company?.CompanyNameEn ?? "",
                JoiningDate = b.Employee?.JoinDate,
                GrossSalary = b.Employee?.GrossSalary ?? 0,
                JobAge = GetJobAge(b.Employee?.JoinDate)
            }));
        }

        [HttpGet("export-bonuses")]
        public async Task<IActionResult> ExportBonuses(
            [FromQuery] int? companyId,
            [FromQuery] int? year,
            [FromQuery] int? month)
        {
            var query = _context.Bonuses
                .Include(b => b.Employee)
                .ThenInclude(e => e!.Company)
                .Include(b => b.Company)
                .AsQueryable();

            if (companyId.HasValue && companyId > 0)
                query = query.Where(b => b.Employee!.CompanyId == companyId.Value || b.CompanyId == companyId.Value);

            if (year.HasValue) query = query.Where(b => b.Year == year.Value);
            if (month.HasValue) query = query.Where(b => b.Month == month.Value);

            var records = await query.ToListAsync();

            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Festival Bonus");

            // Page Setup
            worksheet.PrinterSettings.Orientation = OfficeOpenXml.eOrientation.Landscape;
            worksheet.PrinterSettings.FitToPage = true;
            worksheet.PrinterSettings.FitToWidth = 1;
            worksheet.PrinterSettings.FitToHeight = 0;
            worksheet.Cells.Style.Font.Name = "Arial";
            worksheet.Cells.Style.Font.Size = 10;

            var headers = new[]
            {
                "SL", "Employee ID", "Employee Name", "Joining Date", "Gross Salary", "Job Age", "Amount"
            };

            // Style Headers
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cells[1, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightPink);
                cell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            }
            worksheet.Row(1).Height = 30;

            int row = 2;
            foreach (var b in records)
            {
                worksheet.Row(row).Height = 25;
                worksheet.Cells[row, 1].Value = row - 1;
                worksheet.Cells[row, 2].Value = b.Employee?.EmployeeId;
                worksheet.Cells[row, 3].Value = b.Employee?.FullNameEn;
                worksheet.Cells[row, 4].Value = b.Employee?.JoinDate.ToString("dd MMM yyyy");
                worksheet.Cells[row, 5].Value = b.Employee?.GrossSalary;
                worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 6].Value = GetJobAge(b.Employee?.JoinDate);
                worksheet.Cells[row, 7].Value = b.Amount;
                worksheet.Cells[row, 7].Style.Numberformat.Format = "#,##0.00";
                
                // Borders for data rows
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[row, i + 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    worksheet.Cells[row, i + 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                }
                row++;
            }

            worksheet.Cells.AutoFitColumns();
            
            // Add extra width for readability
            for (int i = 1; i <= headers.Length; i++)
            {
                worksheet.Column(i).Width += 5;
            }

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"Festival_Bonus_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet("export-monthly-sheet")]
        public async Task<IActionResult> ExportMonthlySalarySheet(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] int? companyId,
            [FromQuery] int? departmentId,
            [FromQuery] string? searchTerm,
            [FromQuery] string? exportType = "master")
        {
            var query = _context.MonthlySalarySheets
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Department)
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Designation)
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Line)
                .Where(s => s.Year == year && s.Month == month);

            if (companyId.HasValue && companyId > 0)
            {
                query = query.Where(s => s.Employee!.CompanyId == companyId.Value || s.CompanyId == companyId.Value);
            }

            if (departmentId.HasValue)
            {
                query = query.Where(s => s.Employee!.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s =>
                    s.Employee!.FullNameEn.Contains(searchTerm) || s.Employee.EmployeeId.Contains(searchTerm));
            }

            var records = await query.ToListAsync();
            var sortedRecords = records
                .OrderBy(r => r.Employee?.Line?.NameEn ?? "Unknown")
                .ThenBy(r => r.Employee?.EmployeeId ?? "")
                .ToList();

            using var package = new OfficeOpenXml.ExcelPackage();
            var headers = new[]
            {
                "SL", "Name", "Emp. ID", "Designation & Joining Date", "Total days of the month", 
                "Weekly leave", "Leave", "Total Absent", "Total working days",
                "Basic Salary", "Rent Bill", "Medical allowance", "Food allowance", "Travel allowance",
                "Total Salary", "Absence deduction", "Wages payable", 
                "Overtime hours", "Overtime rate", "Overtime pay", "Attendance bonus", 
                "Deduction", "Account No.", "Total payable"
            };

            void FillSheet(ExcelWorksheet worksheet, List<MonthlySalarySheet> sourceRecords)
            {
                worksheet.Cells.Style.Font.Size = 9;
                worksheet.Cells.Style.WrapText = true;
                worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Row(1).Height = 40;
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.PrinterSettings.PaperSize = OfficeOpenXml.ePaperSize.Legal;
                worksheet.PrinterSettings.Orientation = OfficeOpenXml.eOrientation.Landscape;
                worksheet.PrinterSettings.FitToPage = true;
                worksheet.PrinterSettings.FitToWidth = 1;
                worksheet.PrinterSettings.FitToHeight = 0;

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[1, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    if (i == 1) cell.Style.WrapText = false;
                }

                int row = 2;
                foreach (var s in sourceRecords)
                {
                    var emp = s.Employee;
                    worksheet.Cells[row, 1].Value = row - 1;
                    var nameCell = worksheet.Cells[row, 2];
                    nameCell.Value = emp?.FullNameEn;
                    nameCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    nameCell.Style.WrapText = false;
                    worksheet.Cells[row, 3].Value = emp?.EmployeeId;
                    worksheet.Cells[row, 4].Value = $"{emp?.Designation?.NameEn}\n{emp?.JoinDate:dd/MM/yyyy}";
                    worksheet.Cells[row, 5].Value = s.TotalDays;
                    worksheet.Cells[row, 6].Value = s.WeekendDays;
                    worksheet.Cells[row, 7].Value = s.LeaveDays;
                    worksheet.Cells[row, 8].Value = s.AbsentDays;
                    worksheet.Cells[row, 9].Value = s.PresentDays + s.WeekendDays + s.Holidays + s.LeaveDays;
                    worksheet.Cells[row, 10].Value = emp?.BasicSalary ?? 0;
                    worksheet.Cells[row, 11].Value = emp?.HouseRent ?? 0;
                    worksheet.Cells[row, 12].Value = emp?.MedicalAllowance ?? 0;
                    worksheet.Cells[row, 13].Value = emp?.FoodAllowance ?? 0;
                    worksheet.Cells[row, 14].Value = emp?.Conveyance ?? 0;
                    worksheet.Cells[row, 15].Value = s.GrossSalary;
                    worksheet.Cells[row, 16].Value = s.AbsentDeduction;
                    worksheet.Cells[row, 17].Value = s.TotalEarning - s.OTAmount;
                    worksheet.Cells[row, 18].Value = s.OTHours;
                    worksheet.Cells[row, 19].Value = s.OTRate;
                    worksheet.Cells[row, 20].Value = s.OTAmount;
                    worksheet.Cells[row, 21].Value = s.AttendanceBonus;
                    worksheet.Cells[row, 22].Value = s.TotalDeduction;
                    worksheet.Cells[row, 23].Value = emp?.BankAccountNo;
                    worksheet.Cells[row, 24].Value = s.NetPayable;

                    for (int i = 1; i <= headers.Length; i++)
                    {
                        worksheet.Cells[row, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }
                    row++;
                }

                worksheet.Cells.AutoFitColumns();
            }

            string SanitizeSheetName(string rawName)
            {
                var invalid = new[] { "\\", "/", "?", "*", "[", "]", ":" };
                var cleaned = string.IsNullOrWhiteSpace(rawName) ? "Unknown" : rawName.Trim();
                foreach (var ch in invalid)
                {
                    cleaned = cleaned.Replace(ch, "");
                }
                if (cleaned.Length > 31) cleaned = cleaned.Substring(0, 31);
                return string.IsNullOrWhiteSpace(cleaned) ? "Unknown" : cleaned;
            }

            var exportTypeValue = (exportType ?? "master").Trim().ToLowerInvariant();

            if (exportTypeValue == "salary")
            {
                var groupedByLine = sortedRecords
                    .GroupBy(r => r.Employee?.Line?.NameEn ?? "Unknown")
                    .OrderBy(g => g.Key);

                var usedSheetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var group in groupedByLine)
                {
                    var baseName = SanitizeSheetName(group.Key);
                    var sheetName = baseName;
                    int suffix = 1;
                    while (usedSheetNames.Contains(sheetName))
                    {
                        var suffixText = $"_{suffix++}";
                        var maxBaseLength = 31 - suffixText.Length;
                        var truncatedBase = baseName.Length > maxBaseLength ? baseName.Substring(0, maxBaseLength) : baseName;
                        sheetName = $"{truncatedBase}{suffixText}";
                    }
                    usedSheetNames.Add(sheetName);

                    var sheet = package.Workbook.Worksheets.Add(sheetName);
                    FillSheet(sheet, group.ToList());
                }
            }
            else
            {
                var worksheet = package.Workbook.Worksheets.Add($"Master Sheet {month}-{year}");
                FillSheet(worksheet, sortedRecords);
            }

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            var filePrefix = exportTypeValue == "salary" ? "Salary_Sheet_By_Line" : "Master_Salary_Sheet";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{filePrefix}_{monthName}_{year}.xlsx");
        }

        [HttpGet("export-bank-sheet")]
        public async Task<IActionResult> ExportBankSheet(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] int? companyId,
            [FromQuery] int? departmentId,
            [FromQuery] string? searchTerm)
        {
            var query = _context.MonthlySalarySheets
                .Include(s => s.Employee).ThenInclude(e => e!.Department)
                .Include(s => s.Employee).ThenInclude(e => e!.Designation)
                .Include(s => s.Employee).ThenInclude(e => e!.Group)
                .Where(s => s.Year == year && s.Month == month && s.Employee != null);

            if (companyId.HasValue && companyId > 0)
                query = query.Where(s => s.Employee!.CompanyId == companyId.Value || s.CompanyId == companyId.Value);

            if (departmentId.HasValue)
                query = query.Where(s => s.Employee!.DepartmentId == departmentId.Value);

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(s => s.Employee!.FullNameEn.Contains(searchTerm) || s.Employee!.EmployeeId.Contains(searchTerm));

            var allRecords = await query.ToListAsync();

            using var package = new ExcelPackage();
            
            // Define classification logic
            bool IsmCash(Employee? e) 
            {
                if (e == null) return false;
                var bankName = e.BankName ?? "";
                var accountType = e.BankAccountType ?? "";
                var accountNo = e.BankAccountNo ?? "";

                // Account type mCash AND starts with 01
                if (accountType.Equals("mCash", StringComparison.OrdinalIgnoreCase) && accountNo.StartsWith("01"))
                    return true;

                // Existing bank name logic for mCash
                if (bankName.Contains("Nagad", StringComparison.OrdinalIgnoreCase) || 
                    bankName.Contains("Rocket", StringComparison.OrdinalIgnoreCase) || 
                    bankName.Contains("Bkash", StringComparison.OrdinalIgnoreCase) || 
                    bankName.Contains("mCash", StringComparison.OrdinalIgnoreCase) ||
                    bankName.Contains("Upay", StringComparison.OrdinalIgnoreCase))
                    return true;

                return false;
            }

            bool IsCard(Employee? e)
            {
                if (e == null) return false;
                var accountType = e.BankAccountType ?? "";
                var accountNo = e.BankAccountNo ?? "";

                // Card types: Card, Bank, Savings, Current, Salary
                var cardTypes = new[] { "Card", "Bank", "Savings", "Current", "Salary" };
                if (cardTypes.Any(t => accountType.Equals(t, StringComparison.OrdinalIgnoreCase)))
                    return true;

                // Account starts with 2050
                if (accountNo.StartsWith("2050"))
                    return true;

                return false;
            }

            bool IsStaff(Employee? e) => 
                e?.Group?.NameEn?.Contains("Staff", StringComparison.OrdinalIgnoreCase) == true;

            var holdList = allRecords.Where(s => s.Status.Equals("Hold", StringComparison.OrdinalIgnoreCase)).ToList();
            var nonHoldRecords = allRecords.Where(s => !s.Status.Equals("Hold", StringComparison.OrdinalIgnoreCase)).ToList();
            
            var closeList = nonHoldRecords.Where(s => s.Employee!.Status != "Active" || !s.Employee.IsActive).ToList();
            var activeRecords = nonHoldRecords.Where(s => s.Employee!.Status == "Active" && s.Employee.IsActive).ToList();

            var staffMcash = activeRecords.Where(s => IsStaff(s.Employee) && IsmCash(s.Employee)).ToList();
            var staffCard = activeRecords.Where(s => IsStaff(s.Employee) && IsCard(s.Employee)).ToList();
            var workerMcash = activeRecords.Where(s => !IsStaff(s.Employee) && IsmCash(s.Employee)).ToList();
            var workerCard = activeRecords.Where(s => !IsStaff(s.Employee) && IsCard(s.Employee)).ToList();

            // 1. Summary Sheet
            var summarySheet = package.Workbook.Worksheets.Add("Summary");
            CreateSummarySheet(summarySheet, year, month, staffMcash, staffCard, workerMcash, workerCard, holdList, closeList);

            // 2-7. Individual Sheets
            AddDataSheet(package, "Staff - mCash", staffMcash);
            AddDataSheet(package, "Staff - Card", staffCard);
            AddDataSheet(package, "Worker - mCash", workerMcash);
            AddDataSheet(package, "Worker - Card", workerCard);
            AddDataSheet(package, "Hold List", holdList);
            AddDataSheet(package, "Close List", closeList);

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Bank_Payment_{monthName}_{year}.xlsx");
        }

        private void CreateSummarySheet(ExcelWorksheet ws, int year, int month, 
            List<MonthlySalarySheet> sMcash, List<MonthlySalarySheet> sCard, 
            List<MonthlySalarySheet> wMcash, List<MonthlySalarySheet> wCard,
            List<MonthlySalarySheet> hold, List<MonthlySalarySheet> close)
        {
            ws.Cells["A1:D1"].Merge = true;
            ws.Cells["A1"].Value = "BANK PAYMENT SUMMARY REPORT";
            ws.Cells["A1"].Style.Font.Size = 16;
            ws.Cells["A1"].Style.Font.Bold = true;
            ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            ws.Cells["A2:D2"].Merge = true;
            ws.Cells["A2"].Value = $"Period: {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year}";
            ws.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            string[] headers = { "Category", "Sheet Name", "Total Staff", "Total Amount" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[4, i + 1].Value = headers[i];
                ws.Cells[4, i + 1].Style.Font.Bold = true;
                ws.Cells[4, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[4, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                ws.Cells[4, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            var rows = new[]
            {
                new { Cat = "Staff", Name = "Staff - mCash", Count = sMcash.Count, Amount = sMcash.Sum(x => x.NetPayable) },
                new { Cat = "Staff", Name = "Staff - Card", Count = sCard.Count, Amount = sCard.Sum(x => x.NetPayable) },
                new { Cat = "Worker", Name = "Worker - mCash", Count = wMcash.Count, Amount = wMcash.Sum(x => x.NetPayable) },
                new { Cat = "Worker", Name = "Worker - Card", Count = wCard.Count, Amount = wCard.Sum(x => x.NetPayable) },
                new { Cat = "Special", Name = "Hold List", Count = hold.Count, Amount = hold.Sum(x => x.NetPayable) },
                new { Cat = "Special", Name = "Close List", Count = close.Count, Amount = close.Sum(x => x.NetPayable) }
            };

            int rowIdx = 5;
            foreach (var r in rows)
            {
                ws.Cells[rowIdx, 1].Value = r.Cat;
                ws.Cells[rowIdx, 2].Value = r.Name;
                ws.Cells[rowIdx, 3].Value = r.Count;
                ws.Cells[rowIdx, 4].Value = r.Amount;
                ws.Cells[rowIdx, 4].Style.Numberformat.Format = "#,##0.00";
                
                for(int i=1; i<=4; i++) ws.Cells[rowIdx, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                rowIdx++;
            }

            // Grand Total
            ws.Cells[rowIdx, 1, rowIdx, 2].Merge = true;
            ws.Cells[rowIdx, 1].Value = "GRAND TOTAL";
            ws.Cells[rowIdx, 1].Style.Font.Bold = true;
            ws.Cells[rowIdx, 3].Value = rows.Sum(x => x.Count);
            ws.Cells[rowIdx, 4].Value = rows.Sum(x => x.Amount);
            ws.Cells[rowIdx, 3, rowIdx, 4].Style.Font.Bold = true;
            ws.Cells[rowIdx, 4].Style.Numberformat.Format = "#,##0.00";
            for(int i=1; i<=4; i++) ws.Cells[rowIdx, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            ws.Cells.AutoFitColumns();
        }

        private void AddDataSheet(ExcelPackage package, string name, List<MonthlySalarySheet> records)
        {
            var ws = package.Workbook.Worksheets.Add(name);
            string[] headers = { "SL", "Employee ID", "Name", "Department", "Designation", "Bank Name", "Account Number", "Amount" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[1, i + 1].Value = headers[i];
                ws.Cells[1, i + 1].Style.Font.Bold = true;
                ws.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(51, 122, 183));
                ws.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
            }

            int row = 2;
            foreach (var s in records)
            {
                ws.Cells[row, 1].Value = row - 1;
                ws.Cells[row, 2].Value = s.Employee?.EmployeeId;
                ws.Cells[row, 3].Value = s.Employee?.FullNameEn;
                ws.Cells[row, 4].Value = s.Employee?.Department?.NameEn;
                ws.Cells[row, 5].Value = s.Employee?.Designation?.NameEn;
                ws.Cells[row, 6].Value = s.Employee?.BankName;
                ws.Cells[row, 7].Value = s.Employee?.BankAccountNo;
                ws.Cells[row, 8].Value = s.NetPayable;
                ws.Cells[row, 8].Style.Numberformat.Format = "#,##0.00";
                
                for(int i=1; i<=8; i++) ws.Cells[row, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                row++;
            }

            if (records.Any())
            {
                ws.Cells[row, 1, row, 7].Merge = true;
                ws.Cells[row, 1].Value = "Total Amount";
                ws.Cells[row, 1].Style.Font.Bold = true;
                ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells[row, 8].Value = records.Sum(x => x.NetPayable);
                ws.Cells[row, 8].Style.Font.Bold = true;
                ws.Cells[row, 8].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            ws.Cells.AutoFitColumns();
        }


        [HttpPost("bonus")]
        public async Task<ActionResult> CreateBonus([FromBody] CreateBonusDto dto)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == dto.EmployeeId && e.CompanyId == dto.CompanyId);

            if (employee == null) return NotFound("Employee not found");

            var bonus = new Bonus
            {
                EmployeeId = employee.Id,
                CompanyId = employee.CompanyId,
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

        [HttpPost("process-festival-bonus")]
        public async Task<ActionResult<FestivalBonusSummaryDto>> ProcessFestivalBonus([FromBody] FestivalBonusProcessRequestDto request)
        {
            var employeesQuery = _context.Employees
                .Where(e => e.Status == "Active" && e.IsActive);

            if (request.CompanyId.HasValue && request.CompanyId > 0)
                employeesQuery = employeesQuery.Where(e => e.CompanyId == request.CompanyId.Value);

            var employees = await employeesQuery.ToListAsync();

            // Clear existing bonuses for this period if found to allow "Update last process"
            var existingQuery = _context.Bonuses
                .Where(b => b.Year == request.Year && b.Month == request.Month && b.BonusType == request.BonusType);
            
            if (request.CompanyId.HasValue && request.CompanyId > 0)
                existingQuery = existingQuery.Where(b => b.CompanyId == request.CompanyId.Value);

            var existingToRemove = await existingQuery.ToListAsync();
            if (existingToRemove.Any())
            {
                _context.Bonuses.RemoveRange(existingToRemove);
                await _context.SaveChangesAsync();
            }

            int processed = 0;
            int skipped = 0;
            decimal totalAmount = 0;

            var paymentDate = new DateTime(request.Year, request.Month, 1);

            foreach (var emp in employees)
            {
                // Calculate job age in months at the time of payment
                int totalMonths = ((paymentDate.Year - emp.JoinDate.Year) * 12) + paymentDate.Month - emp.JoinDate.Month;
                
                decimal bonusAmount = 0;
                decimal gross = emp.GrossSalary ?? 0;

                if (totalMonths >= 12)
                {
                    bonusAmount = Math.Round(gross / 2, 2);
                }
                else if (totalMonths >= 6)
                {
                    bonusAmount = Math.Round(gross / 4, 2);
                }

                if (bonusAmount <= 0)
                {
                    skipped++;
                    continue;
                }

                var bonus = new Bonus
                {
                    EmployeeId = emp.Id,
                    CompanyId = emp.CompanyId,
                    BonusType = request.BonusType,
                    Amount = bonusAmount,
                    Year = request.Year,
                    Month = request.Month,
                    PaymentDate = DateTime.UtcNow,
                    Status = "Approved"
                };

                _context.Bonuses.Add(bonus);
                totalAmount += bonusAmount;
                processed++;
            }

            await _context.SaveChangesAsync();

            return Ok(new FestivalBonusSummaryDto
            {
                ProcessedCount = processed,
                SkippedCount = skipped,
                TotalAmount = totalAmount,
                Message = $"Updated! {processed} records created/updated, {skipped} records ineligible based on 6-month rule."
            });
        }

        [HttpGet("export-festival-bonus-bank-sheet")]
        public async Task<IActionResult> ExportFestivalBonusBankSheet(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] int? companyId,
            [FromQuery] int? departmentId,
            [FromQuery] string? searchTerm)
        {
            var query = _context.Bonuses
                .Include(b => b.Employee).ThenInclude(e => e!.Department)
                .Include(b => b.Employee).ThenInclude(e => e!.Designation)
                .Include(b => b.Employee).ThenInclude(e => e!.Group)
                .Where(b => b.Year == year && b.Month == month && b.Employee != null);

            if (companyId.HasValue && companyId > 0)
                query = query.Where(b => b.Employee!.CompanyId == companyId.Value || b.CompanyId == companyId.Value);

            if (departmentId.HasValue)
                query = query.Where(b => b.Employee!.DepartmentId == departmentId.Value);

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(b => b.Employee!.FullNameEn.Contains(searchTerm) || b.Employee!.EmployeeId.Contains(searchTerm));

            var allRecords = await query.ToListAsync();

            using var package = new ExcelPackage();

            // Define classification logic
            bool IsmCash(Employee? e)
            {
                if (e == null) return false;
                var bankName = e.BankName ?? "";
                var accountType = e.BankAccountType ?? "";
                var accountNo = e.BankAccountNo ?? "";

                if (accountType.Equals("mCash", StringComparison.OrdinalIgnoreCase) && accountNo.StartsWith("01"))
                    return true;

                if (bankName.Contains("Nagad", StringComparison.OrdinalIgnoreCase) ||
                    bankName.Contains("Rocket", StringComparison.OrdinalIgnoreCase) ||
                    bankName.Contains("Bkash", StringComparison.OrdinalIgnoreCase) ||
                    bankName.Contains("mCash", StringComparison.OrdinalIgnoreCase) ||
                    bankName.Contains("Upay", StringComparison.OrdinalIgnoreCase))
                    return true;

                return false;
            }

            bool IsCard(Employee? e)
            {
                if (e == null) return false;
                var accountType = e.BankAccountType ?? "";
                var accountNo = e.BankAccountNo ?? "";

                var cardTypes = new[] { "Card", "Bank", "Savings", "Current", "Salary" };
                if (cardTypes.Any(t => accountType.Equals(t, StringComparison.OrdinalIgnoreCase)))
                    return true;

                if (accountNo.StartsWith("2050"))
                    return true;

                return false;
            }

            bool IsStaff(Employee? e) => e?.Group?.NameEn?.Contains("Staff", StringComparison.OrdinalIgnoreCase) == true;

            var activeRecords = allRecords.Where(s => s.Employee!.Status == "Active" && s.Employee.IsActive).ToList();

            var staffMcash = activeRecords.Where(s => IsStaff(s.Employee) && IsmCash(s.Employee)).Select(s => new MonthlySalarySheet { Employee = s.Employee, NetPayable = s.Amount }).ToList();
            var staffCard = activeRecords.Where(s => IsStaff(s.Employee) && IsCard(s.Employee)).Select(s => new MonthlySalarySheet { Employee = s.Employee, NetPayable = s.Amount }).ToList();
            var workerMcash = activeRecords.Where(s => !IsStaff(s.Employee) && IsmCash(s.Employee)).Select(s => new MonthlySalarySheet { Employee = s.Employee, NetPayable = s.Amount }).ToList();
            var workerCard = activeRecords.Where(s => !IsStaff(s.Employee) && IsCard(s.Employee)).Select(s => new MonthlySalarySheet { Employee = s.Employee, NetPayable = s.Amount }).ToList();

            // 1. Summary Sheet
            var summarySheet = package.Workbook.Worksheets.Add("Summary");
            CreateSummarySheet(summarySheet, year, month, staffMcash, staffCard, workerMcash, workerCard, new List<MonthlySalarySheet>(), new List<MonthlySalarySheet>());

            // 2-5. Individual Sheets
            AddDataSheet(package, "Staff - mCash", staffMcash);
            AddDataSheet(package, "Staff - Card", staffCard);
            AddDataSheet(package, "Worker - mCash", workerMcash);
            AddDataSheet(package, "Worker - Card", workerCard);

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Festival_Bonus_Bank_Payment_{monthName}_{year}.xlsx");
        }

        // ─── DAILY SALARY SHEET ───────────────────────────────────────────────────

        [HttpPost("process-daily")]
        public async Task<ActionResult<DailyProcessResultDto>> ProcessDailySalary([FromBody] DailyProcessRequestDto request)
        {
            var date = request.Date.Date;

            var employeesQuery = _context.Employees
                .Where(e => e.Status == "Active" && e.IsActive);

            if (request.CompanyId.HasValue && request.CompanyId > 0)
                employeesQuery = employeesQuery.Where(e => e.CompanyId == request.CompanyId.Value);

            if (request.DepartmentId.HasValue)
                employeesQuery = employeesQuery.Where(e => e.DepartmentId == request.DepartmentId.Value);

            if (!string.IsNullOrEmpty(request.EmployeeId))
                employeesQuery = employeesQuery.Where(e => e.EmployeeId == request.EmployeeId);

            var employees = await employeesQuery.ToListAsync();

            // Load attendances for this date
            var attendances = await _context.Attendances
                .Where(a => a.Date.Date == date)
                .ToListAsync();

            // Remove existing daily sheets for same date/employees to allow re-processing
            var existingIds = employees.Select(e => e.Id).ToList();
            var existingSheets = await _context.DailySalarySheets
                .Where(s => s.Date.Date == date && existingIds.Contains(s.EmployeeId))
                .ToListAsync();

            if (existingSheets.Any())
            {
                _context.DailySalarySheets.RemoveRange(existingSheets);
                await _context.SaveChangesAsync();
            }

            int processed = 0;
            int skipped = 0;
            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);

            foreach (var emp in employees)
            {
                decimal gross = emp.GrossSalary ?? 0;
                if (gross <= 0) { skipped++; continue; }

                var att = attendances.FirstOrDefault(a => a.EmployeeCard == emp.Id);

                decimal perDay = daysInMonth > 0 ? gross / daysInMonth : 0;
                string status = att?.Status ?? "Absent";

                decimal otHours = att?.OTHours ?? 0;
                decimal otRate = (emp.BasicSalary ?? 0) / 208 * 2;
                decimal otAmount = otHours * otRate;

                decimal deduction = (status == "Absent") ? perDay : 0;
                decimal earning = (status == "Present" || status == "Late") ? perDay + otAmount : 0;
                decimal netPayable = earning - deduction;

                var sheet = new DailySalarySheet
                {
                    EmployeeId = emp.Id,
                    CompanyId = emp.CompanyId,
                    Date = date,
                    GrossSalary = gross,
                    PerDaySalary = perDay,
                    AttendanceStatus = status,
                    OTHours = otHours,
                    OTAmount = otAmount,
                    TotalEarning = earning,
                    Deduction = deduction,
                    NetPayable = netPayable,
                    ProcessedAt = DateTime.UtcNow
                };

                _context.DailySalarySheets.Add(sheet);
                processed++;
            }

            await _context.SaveChangesAsync();

            return Ok(new DailyProcessResultDto
            {
                ProcessedCount = processed,
                SkippedCount = skipped,
                Message = $"Processed {processed} employees for {date:dd MMM yyyy}."
            });
        }

        [HttpGet("export-daily-sheet")]
        public async Task<IActionResult> ExportDailySalarySheet(
            [FromQuery] DateTime date,
            [FromQuery] int? companyId,
            [FromQuery] int? departmentId,
            [FromQuery] string? searchTerm)
        {
            var query = _context.DailySalarySheets
                .Include(s => s.Employee).ThenInclude(e => e!.Department)
                .Include(s => s.Employee).ThenInclude(e => e!.Designation)
                .Include(s => s.Employee).ThenInclude(e => e!.Company)
                .Where(s => s.Date.Date == date.Date);

            if (companyId.HasValue && companyId > 0)
                query = query.Where(s => s.Employee!.CompanyId == companyId.Value || s.CompanyId == companyId.Value);

            if (departmentId.HasValue)
                query = query.Where(s => s.Employee!.DepartmentId == departmentId.Value);

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(s => s.Employee!.FullNameEn.Contains(searchTerm) || s.Employee!.EmployeeId.Contains(searchTerm));

            var records = await query.OrderBy(s => s.Employee!.EmployeeId).ToListAsync();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add($"Daily Sheet {date:dd-MM-yyyy}");

            // Title
            ws.Cells["A1:J1"].Merge = true;
            ws.Cells["A1"].Value = $"DAILY SALARY SHEET — {date:dd MMMM yyyy}";
            ws.Cells["A1"].Style.Font.Size = 14;
            ws.Cells["A1"].Style.Font.Bold = true;
            ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            string[] headers = { "SL", "Employee ID", "Name", "Department", "Designation", "Status", "Per Day (৳)", "OT Hrs", "OT Amount (৳)", "Net Payable (৳)" };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cells[3, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(51, 122, 183));
                cell.Style.Font.Color.SetColor(Color.White);
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            int row = 4;
            foreach (var s in records)
            {
                ws.Cells[row, 1].Value = row - 3;
                ws.Cells[row, 2].Value = s.Employee?.EmployeeId;
                ws.Cells[row, 3].Value = s.Employee?.FullNameEn;
                ws.Cells[row, 4].Value = s.Employee?.Department?.NameEn;
                ws.Cells[row, 5].Value = s.Employee?.Designation?.NameEn;
                ws.Cells[row, 6].Value = s.AttendanceStatus;
                ws.Cells[row, 7].Value = s.PerDaySalary;
                ws.Cells[row, 7].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 8].Value = s.OTHours;
                ws.Cells[row, 9].Value = s.OTAmount;
                ws.Cells[row, 9].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 10].Value = s.NetPayable;
                ws.Cells[row, 10].Style.Numberformat.Format = "#,##0.00";

                for (int i = 1; i <= 10; i++)
                    ws.Cells[row, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                row++;
            }

            // Total row
            if (records.Any())
            {
                ws.Cells[row, 1, row, 9].Merge = true;
                ws.Cells[row, 1].Value = "TOTAL";
                ws.Cells[row, 1].Style.Font.Bold = true;
                ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells[row, 10].Value = records.Sum(x => x.NetPayable);
                ws.Cells[row, 10].Style.Font.Bold = true;
                ws.Cells[row, 10].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                ws.Cells[row, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            ws.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Daily_Salary_{date:dd-MM-yyyy}.xlsx");
        }
    }
}
