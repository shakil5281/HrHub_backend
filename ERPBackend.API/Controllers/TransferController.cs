using ERPBackend.Core.Constants;
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
    public class TransferController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransferController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransferDto>>> GetTransfers()
        {
            var transfers = await _context.Transfers
                .Include(t => t.Employee)
                .Include(t => t.FromDepartment)
                .Include(t => t.FromDesignation)
                .Include(t => t.ToDepartment)
                .Include(t => t.ToDesignation)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return transfers.Select(t => new TransferDto
            {
                Id = t.Id,
                EmployeeId = t.EmployeeId,
                EmployeeName = t.Employee?.FullNameEn ?? "Unknown",
                EmployeeCode = t.Employee?.EmployeeId ?? "N/A",
                FromDepartmentId = t.FromDepartmentId,
                FromDepartmentName = t.FromDepartment?.NameEn,
                FromDesignationId = t.FromDesignationId,
                FromDesignationName = t.FromDesignation?.NameEn,
                ToDepartmentId = t.ToDepartmentId,
                ToDepartmentName = t.ToDepartment?.NameEn,
                ToDesignationId = t.ToDesignationId,
                ToDesignationName = t.ToDesignation?.NameEn,
                TransferDate = t.TransferDate,
                Reason = t.Reason,
                Status = t.Status,
                CreatedAt = t.CreatedAt
            }).ToList();
        }

        [HttpPost]
        public async Task<ActionResult<TransferDto>> CreateTransfer(CreateTransferDto dto)
        {
            var employee = await _context.Employees.FindAsync(dto.EmployeeId);
            if (employee == null) return NotFound("Employee not found");

            var transfer = new Transfer
            {
                EmployeeId = dto.EmployeeId,
                FromDepartmentId = employee.DepartmentId,
                FromDesignationId = employee.DesignationId,
                ToDepartmentId = dto.ToDepartmentId,
                ToDesignationId = dto.ToDesignationId,
                TransferDate = dto.TransferDate,
                Reason = dto.Reason,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            _context.Transfers.Add(transfer);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Transfer request created", id = transfer.Id });
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager)]
        public async Task<IActionResult> UpdateStatus(int id, UpdateTransferStatusDto dto)
        {
            var transfer = await _context.Transfers
                .Include(t => t.Employee)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transfer == null) return NotFound();

            // Prevent re-processing if already final
            if (transfer.Status == "Approved" || transfer.Status == "Rejected")
            {
                 return BadRequest($"Transfer is already {transfer.Status}");
            }

            if (dto.Status == "Approved")
            {
                // Update Employee
                var employee = transfer.Employee;
                if (employee != null)
                {
                    employee.DepartmentId = transfer.ToDepartmentId;
                    employee.DesignationId = transfer.ToDesignationId;
                    _context.Employees.Update(employee);
                }
                transfer.ApprovedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                transfer.ApprovedAt = DateTime.UtcNow;
            }

            transfer.Status = dto.Status;
            transfer.AdminRemark = dto.AdminRemark;

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Transfer {dto.Status}" });
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransfer(int id)
        {
             var transfer = await _context.Transfers.FindAsync(id);
             if (transfer == null) return NotFound();
             
             if (transfer.Status == "Approved") return BadRequest("Cannot delete approved transfer");

             _context.Transfers.Remove(transfer);
             await _context.SaveChangesAsync();
             return NoContent();
        }
    }
}
