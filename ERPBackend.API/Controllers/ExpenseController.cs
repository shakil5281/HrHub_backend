using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {
        private readonly CashbookDbContext _context;

        public ExpenseController(CashbookDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetExpenses(DateTime? fromDate, DateTime? toDate, string? category, string? branch)
        {
            var query = _context.Expenses.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(e => e.ExpenseDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(e => e.ExpenseDate <= toDate.Value);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(e => e.Category == category);

            if (!string.IsNullOrEmpty(branch))
                query = query.Where(e => e.Branch == branch);

            return await query.OrderByDescending(e => e.ExpenseDate)
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    ExpenseDate = e.ExpenseDate,
                    Category = e.Category,
                    Amount = e.Amount,
                    PaymentMethod = e.PaymentMethod,
                    ReferenceNumber = e.ReferenceNumber,
                    Description = e.Description,
                    Branch = e.Branch,
                    CreatedAt = e.CreatedAt
                }).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseDto>> GetExpense(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            return new ExpenseDto
            {
                Id = expense.Id,
                ExpenseDate = expense.ExpenseDate,
                Category = expense.Category,
                Amount = expense.Amount,
                PaymentMethod = expense.PaymentMethod,
                ReferenceNumber = expense.ReferenceNumber,
                Description = expense.Description,
                Branch = expense.Branch,
                CreatedAt = expense.CreatedAt
            };
        }

        [HttpPost]
        public async Task<ActionResult<ExpenseDto>> CreateExpense(ExpenseCreateDto dto)
        {
            var expense = new Expense
            {
                ExpenseDate = dto.ExpenseDate,
                Category = dto.Category,
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                ReferenceNumber = dto.ReferenceNumber,
                Description = dto.Description,
                Branch = dto.Branch,
                CreatedAt = DateTime.Now
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, new ExpenseDto
            {
                Id = expense.Id,
                ExpenseDate = expense.ExpenseDate,
                Category = expense.Category,
                Amount = expense.Amount,
                PaymentMethod = expense.PaymentMethod,
                ReferenceNumber = expense.ReferenceNumber,
                Description = expense.Description,
                Branch = expense.Branch,
                CreatedAt = expense.CreatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpense(int id, ExpenseCreateDto dto)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            expense.ExpenseDate = dto.ExpenseDate;
            expense.Category = dto.Category;
            expense.Amount = dto.Amount;
            expense.PaymentMethod = dto.PaymentMethod;
            expense.ReferenceNumber = dto.ReferenceNumber;
            expense.Description = dto.Description;
            expense.Branch = dto.Branch;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportExcel(DateTime? fromDate, DateTime? toDate, string? category, string? branch)
        {
            var query = _context.Expenses.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(e => e.ExpenseDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(e => e.ExpenseDate <= toDate.Value);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(e => e.Category == category);

            if (!string.IsNullOrEmpty(branch))
                query = query.Where(e => e.Branch == branch);

            var expenses = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Expenses");
                worksheet.Cells[1, 1].Value = "Date";
                worksheet.Cells[1, 2].Value = "Category";
                worksheet.Cells[1, 3].Value = "Amount";
                worksheet.Cells[1, 4].Value = "Payment Method";
                worksheet.Cells[1, 5].Value = "Reference";
                worksheet.Cells[1, 6].Value = "Branch";
                worksheet.Cells[1, 7].Value = "Description";

                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                int row = 2;
                foreach (var e in expenses)
                {
                    worksheet.Cells[row, 1].Value = e.ExpenseDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 2].Value = e.Category;
                    worksheet.Cells[row, 3].Value = e.Amount;
                    worksheet.Cells[row, 4].Value = e.PaymentMethod;
                    worksheet.Cells[row, 5].Value = e.ReferenceNumber;
                    worksheet.Cells[row, 6].Value = e.Branch;
                    worksheet.Cells[row, 7].Value = e.Description;
                    row++;
                }

                worksheet.Cells.AutoFitColumns();
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Expenses_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }
    }
}
