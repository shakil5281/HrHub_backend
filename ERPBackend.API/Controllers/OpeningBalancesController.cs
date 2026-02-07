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
    public class OpeningBalancesController : ControllerBase
    {
        private readonly CashbookDbContext _context;

        public OpeningBalancesController(CashbookDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OpeningBalance>>> GetBalances()
        {
            return await _context.OpeningBalances.OrderByDescending(b => b.Date).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OpeningBalance>> GetBalance(int id)
        {
            var balance = await _context.OpeningBalances.FindAsync(id);
            if (balance == null) return NotFound();
            return balance;
        }

        [HttpPost]
        public async Task<ActionResult<OpeningBalance>> CreateBalance(OpeningBalanceDto dto)
        {
            var balance = new OpeningBalance
            {
                AccountName = dto.AccountName,
                Category = dto.Category,
                Amount = dto.Amount,
                Date = dto.Date,
                Remarks = dto.Remarks,
                CreatedAt = DateTime.Now
            };

            _context.OpeningBalances.Add(balance);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBalance), new { id = balance.Id }, balance);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBalance(int id, OpeningBalanceDto dto)
        {
            var balance = await _context.OpeningBalances.FindAsync(id);
            if (balance == null) return NotFound();

            balance.AccountName = dto.AccountName ?? balance.AccountName;
            balance.Category = dto.Category ?? balance.Category;
            balance.Amount = dto.Amount;
            balance.Date = dto.Date;
            balance.Remarks = dto.Remarks ?? balance.Remarks;

            _context.Entry(balance).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBalance(int id)
        {
            var balance = await _context.OpeningBalances.FindAsync(id);
            if (balance == null) return NotFound();

            _context.OpeningBalances.Remove(balance);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
