using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using ERPBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERPBackend.Services.Services
{
    public class NightBillService : INightBillService
    {
        private readonly ApplicationDbContext _context;

        public NightBillService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<NightBillResponseDto> GetNightBillsAsync(DateTime? fromDate, DateTime? toDate, int? employeeId, int? departmentId, string? status, string? searchTerm)
        {
            var query = _context.NightBills
                .Include(i => i.Employee).ThenInclude(e => e!.Department)
                .Include(i => i.Employee).ThenInclude(e => e!.Company)
                .Include(i => i.Employee).ThenInclude(e => e!.Designation)
                .Include(i => i.Shift)
                .AsQueryable();

            if (fromDate.HasValue) 
            {
                var from = fromDate.Value.Date;
                query = query.Where(i => i.Date >= from);
            }
            if (toDate.HasValue)
            {
                var to = toDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(i => i.Date <= to);
            }
            if (employeeId.HasValue) query = query.Where(i => i.EmployeeId == employeeId.Value);
            if (departmentId.HasValue) query = query.Where(i => i.Employee!.DepartmentId == departmentId.Value);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(i => i.Status == status);

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(o => o.Employee!.EmployeeId.Contains(searchTerm) || o.Employee!.FullNameEn.Contains(searchTerm));

            var records = await (from b in _context.NightBills
                                 join a in _context.Attendances on new { b.EmployeeId, Date = b.Date.Date } equals new { EmployeeId = a.EmployeeCard, Date = a.Date.Date } into attJoin
                                 from att in attJoin.DefaultIfEmpty()
                                 where (fromDate == null || b.Date >= fromDate.Value.Date) &&
                                       (toDate == null || b.Date <= toDate.Value.Date.AddDays(1).AddSeconds(-1)) &&
                                       (employeeId == null || b.EmployeeId == employeeId) &&
                                       (departmentId == null || b.Employee!.DepartmentId == departmentId) &&
                                       (string.IsNullOrEmpty(status) || b.Status == status) &&
                                       (string.IsNullOrEmpty(searchTerm) || b.Employee!.EmployeeId.Contains(searchTerm) || b.Employee!.FullNameEn.Contains(searchTerm))
                                 orderby b.Date descending
                                 select new NightBillDto
                                 {
                                     Id = b.Id,
                                     EmployeeCard = b.EmployeeId,
                                     EmployeeId = b.Employee!.EmployeeId,
                                     EmployeeName = b.Employee!.FullNameEn,
                                     Department = b.Employee!.Department!.NameEn,
                                     Designation = b.Employee!.Designation!.NameEn,
                                     Date = b.Date,
                                     Amount = b.Amount,
                                     Status = b.Status,
                                     CreatedAt = b.CreatedAt,
                                     ShiftName = b.Shift != null ? b.Shift.NameEn : "N/A",
                                     CompanyName = b.Employee.Company != null ? b.Employee.Company.CompanyNameEn : "",
                                     // Fallback to Attendance table if bill record timing is null
                                     InTime = b.InTime ?? (att != null ? att.InTime : null),
                                     OutTime = b.OutTime ?? (att != null ? att.OutTime : null)
                                 }).ToListAsync();

            var summary = new NightBillSummaryDto
            {
                TotalAmount = records.Sum(r => r.Amount),
                TotalEmployees = records.Select(r => r.EmployeeId).Distinct().Count(),
                TotalRecords = records.Count
            };

            return new NightBillResponseDto { Summary = summary, Records = records };
        }

        public async Task<int> ProcessNightBillsAsync(BillProcessRequestDto request, string userName)
        {
            int processedCount = 0;
            var from = request.FromDate.Date;
            var to = request.ToDate.Date.AddDays(1).AddSeconds(-1);

            var employees = await _context.Employees
                .Where(e => e.IsActive && (!request.DepartmentId.HasValue || e.DepartmentId == request.DepartmentId))
                .Include(e => e.Shift)
                .Include(e => e.Designation)
                .ToListAsync();

            var attendanceRecords = await _context.Attendances
                .Where(a => a.Date >= from && a.Date <= to && (a.Status.StartsWith("Present") || a.Status == "Late"))
                .ToListAsync();

            var existingBills = await _context.NightBills
                .Where(i => i.Date >= from && i.Date <= to)
                .ToListAsync();

            var config = await _context.NightBillConfigs.FirstOrDefaultAsync(c => c.IsActive && (!request.CompanyId.HasValue || c.CompanyId == request.CompanyId));
            TimeSpan eligibleTimeThreshold = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(45)); // Default 23:45 
            
            if (config != null && !string.IsNullOrEmpty(config.EligibleTime))
            {
                if (TimeSpan.TryParse(config.EligibleTime, out var customThreshold))
                {
                    eligibleTimeThreshold = customThreshold;
                }
            }
            
            decimal configAmount = config?.Amount ?? 0;

            var formulas = new List<NightBill>();

            foreach (var att in attendanceRecords)
            {
                var emp = employees.FirstOrDefault(e => e.Id == att.EmployeeCard);
                if (emp == null || emp.Shift == null || emp.Designation == null) continue;

                if (!emp.Designation.IsNightBillEligible || !emp.Designation.IsStaff) continue;

                if (att.OutTime.HasValue)
                {
                    TimeSpan outTime = att.OutTime.Value.TimeOfDay;
                    // Logic: Either checkout after threshold (e.g. 23:45) OR checkout early next morning (before 6am)
                    bool isAfterThreshold = (outTime >= eligibleTimeThreshold) || (outTime < TimeSpan.FromHours(6));

                    if (isAfterThreshold)
                    {
                        var existing = existingBills.FirstOrDefault(i => i.EmployeeId == emp.Id && i.Date.Date == att.Date.Date);
                        if (existing == null)
                        {
                            formulas.Add(new NightBill
                            {
                                EmployeeId = emp.Id,
                                Date = att.Date,
                                Amount = configAmount > 0 ? configAmount : emp.Designation.NightBill,
                                ShiftId = emp.Shift.Id,
                                CompanyId = emp.CompanyId,
                                InTime = att.InTime,
                                OutTime = att.OutTime,
                                Status = "Approved",
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = userName
                            });
                            processedCount++;
                        }
                    }
                }
            }

            if (formulas.Any())
            {
                await _context.NightBills.AddRangeAsync(formulas);
                await _context.SaveChangesAsync();
            }

            return processedCount;
        }

        public async Task<bool> DeleteNightBillAsync(int id)
        {
            var record = await _context.NightBills.FindAsync(id);
            if (record == null) return false;

            _context.NightBills.Remove(record);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteMultipleAsync(List<int> ids)
        {
            var records = await _context.NightBills.Where(r => ids.Contains(r.Id)).ToListAsync();
            if (!records.Any()) return 0;

            _context.NightBills.RemoveRange(records);
            await _context.SaveChangesAsync();
            return records.Count;
        }
    }
}
