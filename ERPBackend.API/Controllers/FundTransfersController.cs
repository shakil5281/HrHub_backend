using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class FundTransfersController : ControllerBase
    {
        private readonly CashbookDbContext _context;

        public FundTransfersController(CashbookDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FundTransfer>>> GetTransfers(string? branch, string? status)
        {
            var query = _context.FundTransfers.AsQueryable();

            if (!string.IsNullOrEmpty(branch))
                query = query.Where(t => t.FromBranch == branch || t.ToBranch == branch);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            return await query.OrderByDescending(t => t.RequestDate).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FundTransfer>> GetTransfer(int id)
        {
            var transfer = await _context.FundTransfers.FindAsync(id);
            if (transfer == null) return NotFound();
            return transfer;
        }

        [HttpPost]
        public async Task<ActionResult<FundTransfer>> CreateTransfer(FundTransferDto dto)
        {
            var transfer = new FundTransfer
            {
                FromBranch = dto.FromBranch,
                ToBranch = dto.ToBranch,
                RequestedAmount = dto.RequestedAmount,
                Reason = dto.Reason,
                Status = "Pending",
                RequestDate = DateTime.Now
            };

            _context.FundTransfers.Add(transfer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTransfer), new { id = transfer.Id }, transfer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransfer(int id, FundTransferDto dto)
        {
            var transfer = await _context.FundTransfers.FindAsync(id);
            if (transfer == null) return NotFound();

            transfer.Status = dto.Status ?? transfer.Status;
            transfer.ApprovedAmount = dto.ApprovedAmount ?? transfer.ApprovedAmount;
            
            if (transfer.Status == "Approved")
                transfer.ApprovedDate = DateTime.Now;
            
            if (transfer.Status == "Completed")
                transfer.CompletedDate = DateTime.Now;

            _context.Entry(transfer).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransfer(int id)
        {
            var transfer = await _context.FundTransfers.FindAsync(id);
            if (transfer == null) return NotFound();

            _context.FundTransfers.Remove(transfer);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
