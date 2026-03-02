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
                CompanyName = s.Employee?.Company?.CompanyNameEn ?? s.Company?.CompanyNameEn ?? ""
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

            foreach (var emp in employees)
            {
                var sheet = existingSheets.FirstOrDefault(s => s.EmployeeId == emp.Id) ?? new MonthlySalarySheet();
                var empAttendances = monthAttendances.Where(a => a.EmployeeCard == emp.Id).ToList();

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
            return Ok(records.Select(a => new AdvanceSalaryDto
            {
                Id = a.Id,
                EmployeeId = a.Employee?.EmployeeId ?? "",
                CompanyId = a.Employee?.CompanyId ?? a.CompanyId ?? 0,
                EmployeeName = a.Employee?.FullNameEn ?? "",
                Amount = a.Amount,
                RequestDate = a.RequestDate,
                RepaymentMonth = a.RepaymentMonth,
                RepaymentYear = a.RepaymentYear,
                Status = a.Status,
                Remarks = a.Remarks,
                CompanyName = a.Employee?.Company?.CompanyNameEn ?? a.Company?.CompanyNameEn ?? ""
            }));
        }

        [HttpPost("advance-salary")]
        public async Task<ActionResult> CreateAdvanceSalary([FromBody] CreateAdvanceSalaryDto dto)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == dto.EmployeeId && e.CompanyId == dto.CompanyId);

            if (employee == null) return NotFound("Employee not found");

            var advance = new AdvanceSalary
            {
                EmployeeId = employee.Id,
                CompanyId = employee.CompanyId,
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
            [FromQuery] string? searchTerm)
        {
            var query = _context.MonthlySalarySheets
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Department)
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Designation)
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

            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add($"Salary Sheet {month}-{year}");

            // Headers
            var headers = new[]
            {
                "SL", "ID", "Name", "Designation", "Department",
                "Gross Salary", "Basic", "Total Days", "Present", "Absent", "Leave",
                "OT Hours", "OT Amount", "Total Earning", "Total Deduction", "Net Payable"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            int row = 2;
            foreach (var s in records)
            {
                worksheet.Cells[row, 1].Value = row - 1;
                worksheet.Cells[row, 2].Value = s.Employee?.EmployeeId;
                worksheet.Cells[row, 3].Value = s.Employee?.FullNameEn;
                worksheet.Cells[row, 4].Value = s.Employee?.Designation?.NameEn;
                worksheet.Cells[row, 5].Value = s.Employee?.Department?.NameEn;
                worksheet.Cells[row, 6].Value = s.GrossSalary;
                worksheet.Cells[row, 7].Value = s.BasicSalary;
                worksheet.Cells[row, 8].Value = s.TotalDays;
                worksheet.Cells[row, 9].Value = s.PresentDays;
                worksheet.Cells[row, 10].Value = s.AbsentDays;
                worksheet.Cells[row, 11].Value = s.LeaveDays;
                worksheet.Cells[row, 12].Value = s.OTHours;
                worksheet.Cells[row, 13].Value = s.OTAmount;
                worksheet.Cells[row, 14].Value = s.TotalEarning;
                worksheet.Cells[row, 15].Value = s.TotalDeduction;
                worksheet.Cells[row, 16].Value = s.NetPayable;
                row++;
            }

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Salary_Sheet_{monthName}_{year}.xlsx");
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
            bool IsmCash(string? bankName) => 
                !string.IsNullOrEmpty(bankName) && 
                (bankName.Contains("Nagad", StringComparison.OrdinalIgnoreCase) || 
                 bankName.Contains("Rocket", StringComparison.OrdinalIgnoreCase) || 
                 bankName.Contains("Bkash", StringComparison.OrdinalIgnoreCase) || 
                 bankName.Contains("mCash", StringComparison.OrdinalIgnoreCase) ||
                 bankName.Contains("Upay", StringComparison.OrdinalIgnoreCase));

            bool IsStaff(Employee? e) => 
                e?.Group?.NameEn?.Contains("Staff", StringComparison.OrdinalIgnoreCase) == true;

            var holdList = allRecords.Where(s => s.Status.Equals("Hold", StringComparison.OrdinalIgnoreCase)).ToList();
            var nonHoldRecords = allRecords.Where(s => !s.Status.Equals("Hold", StringComparison.OrdinalIgnoreCase)).ToList();
            
            var closeList = nonHoldRecords.Where(s => s.Employee!.Status != "Active" || !s.Employee.IsActive).ToList();
            var activeRecords = nonHoldRecords.Where(s => s.Employee!.Status == "Active" && s.Employee.IsActive).ToList();

            var staffMcash = activeRecords.Where(s => IsStaff(s.Employee) && IsmCash(s.Employee?.BankName)).ToList();
            var staffCard = activeRecords.Where(s => IsStaff(s.Employee) && !IsmCash(s.Employee?.BankName)).ToList();
            var workerMcash = activeRecords.Where(s => !IsStaff(s.Employee) && IsmCash(s.Employee?.BankName)).ToList();
            var workerCard = activeRecords.Where(s => !IsStaff(s.Employee) && !IsmCash(s.Employee?.BankName)).ToList();

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

        [HttpDelete("bonus/{id}")]
        public async Task<IActionResult> DeleteBonus(int id)
        {
            var bonus = await _context.Bonuses.FindAsync(id);
            if (bonus == null) return NotFound();

            _context.Bonuses.Remove(bonus);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
