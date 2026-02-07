using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AccountsController : ControllerBase
    {
        private readonly CashbookDbContext _context;

        public AccountsController(CashbookDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CashTransaction>> GetTransaction(int id)
        {
            var transaction = await _context.CashTransactions.FindAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            return transaction;
        }

        [HttpGet("transactions")]
        public async Task<ActionResult<IEnumerable<CashTransaction>>> GetTransactions(string? type, string? branch, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.CashTransactions.AsQueryable();

            if (!string.IsNullOrEmpty(type))
                query = query.Where(t => t.TransactionType == type);

            if (!string.IsNullOrEmpty(branch))
                query = query.Where(t => t.Branch == branch);

            if (fromDate.HasValue)
                query = query.Where(t => t.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.TransactionDate <= toDate.Value);

            return await query.OrderByDescending(t => t.TransactionDate).ToListAsync();
        }

        [HttpPost("receive")]
        public async Task<ActionResult<CashTransaction>> ReceiveCash([FromBody] CashTransactionDto dto)
        {
            var transaction = new CashTransaction
            {
                TransactionDate = dto.TransactionDate,
                TransactionType = "Received",
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                ReferenceNumber = dto.ReferenceNumber,
                Description = dto.Description,
                Branch = dto.Branch,
                CreatedAt = DateTime.Now
            };

            _context.CashTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(transaction);
        }

        [HttpPost("expense")]
        public async Task<ActionResult<CashTransaction>> ExpenseCash([FromBody] CashTransactionDto dto)
        {
            var transaction = new CashTransaction
            {
                TransactionDate = dto.TransactionDate,
                TransactionType = "Expense",
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                ReferenceNumber = dto.ReferenceNumber,
                Description = dto.Description,
                Branch = dto.Branch,
                CreatedAt = DateTime.Now
            };

            _context.CashTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(transaction);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, CashTransactionDto dto)
        {
            var transaction = await _context.CashTransactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            transaction.TransactionDate = dto.TransactionDate;
            transaction.Amount = dto.Amount;
            transaction.PaymentMethod = dto.PaymentMethod ?? transaction.PaymentMethod;
            transaction.ReferenceNumber = dto.ReferenceNumber ?? transaction.ReferenceNumber;
            transaction.Description = dto.Description ?? transaction.Description;
            transaction.Branch = dto.Branch ?? transaction.Branch;
            if (!string.IsNullOrEmpty(dto.TransactionType))
            {
                transaction.TransactionType = dto.TransactionType;
            }

            _context.Entry(transaction).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransactionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.CashTransactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            _context.CashTransactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TransactionExists(int id)
        {
            return _context.CashTransactions.Any(e => e.Id == id);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<AccountsSummaryDto>> GetSummary()
        {
            var transactions = await _context.CashTransactions.ToListAsync();

            var received = transactions.Where(t => t.TransactionType == "Received").Sum(t => t.Amount);
            var expense = transactions.Where(t => t.TransactionType == "Expense").Sum(t => t.Amount);

            var branchBalances = transactions
                .GroupBy(t => t.Branch ?? "Main")
                .Select(g => new BranchBalanceDto
                {
                    BranchName = g.Key,
                    Balance = g.Where(t => t.TransactionType == "Received").Sum(t => t.Amount) - 
                              g.Where(t => t.TransactionType == "Expense").Sum(t => t.Amount)
                }).ToList();

            return Ok(new AccountsSummaryDto
            {
                TotalReceived = received,
                TotalExpense = expense,
                CurrentBalance = received - expense,
                BranchBalances = branchBalances
            });
        }

        [HttpGet("balance-sheet")]
        public async Task<ActionResult<BalanceSheetDto>> GetBalanceSheet()
        {
            var transactions = await _context.CashTransactions.ToListAsync();

            var totalReceived = transactions.Where(t => t.TransactionType == "Received").Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.TransactionType == "Expense").Sum(t => t.Amount);

            return Ok(new BalanceSheetDto
            {
                TotalAssets = totalReceived,
                TotalLiabilities = totalExpense,
                NetWorth = totalReceived - totalExpense
            });
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportExcel(string? type, string? branch, DateTime? fromDate, DateTime? toDate)
        {
            var transactions = await GetTransactionsInternal(type, branch, fromDate, toDate);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Transactions");
                
                // Headers
                worksheet.Cells[1, 1].Value = "Date";
                worksheet.Cells[1, 2].Value = "Type";
                worksheet.Cells[1, 3].Value = "Amount";
                worksheet.Cells[1, 4].Value = "Method";
                worksheet.Cells[1, 5].Value = "Branch";
                worksheet.Cells[1, 6].Value = "Reference";
                worksheet.Cells[1, 7].Value = "Description";

                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                int row = 2;
                foreach (var t in transactions)
                {
                    worksheet.Cells[row, 1].Value = t.TransactionDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 2].Value = t.TransactionType;
                    worksheet.Cells[row, 3].Value = t.Amount;
                    worksheet.Cells[row, 4].Value = t.PaymentMethod;
                    worksheet.Cells[row, 5].Value = t.Branch;
                    worksheet.Cells[row, 6].Value = t.ReferenceNumber;
                    worksheet.Cells[row, 7].Value = t.Description;
                    row++;
                }

                worksheet.Cells.AutoFitColumns();
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"Transactions_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportPdf(string? type, string? branch, DateTime? fromDate, DateTime? toDate)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            var transactions = await GetTransactionsInternal(type, branch, fromDate, toDate);
            string title = type == "Received" ? "Cash Received Report" : (type == "Expense" ? "Cash Expense Report" : "Cash Transaction Report");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Header().Text(title).FontSize(24).ExtraBold().FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Date");
                            header.Cell().Element(CellStyle).Text("Type");
                            header.Cell().Element(CellStyle).Text("Amount");
                            header.Cell().Element(CellStyle).Text("Method");
                            header.Cell().Element(CellStyle).Text("Branch");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            }
                        });

                        foreach (var item in transactions)
                        {
                            table.Cell().Element(ValueStyle).Text(item.TransactionDate.ToString("yyyy-MM-dd"));
                            table.Cell().Element(ValueStyle).Text(item.TransactionType);
                            table.Cell().Element(ValueStyle).Text(item.Amount.ToString("C"));
                            table.Cell().Element(ValueStyle).Text(item.PaymentMethod);
                            table.Cell().Element(ValueStyle).Text(item.Branch);

                            static IContainer ValueStyle(IContainer container)
                            {
                                return container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            });

            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;

            string fileName = $"Transactions_{DateTime.Now:yyyyMMdd}.pdf";
            return File(stream, "application/pdf", fileName);
        }

        private async Task<List<CashTransaction>> GetTransactionsInternal(string? type, string? branch, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.CashTransactions.AsQueryable();

            if (!string.IsNullOrEmpty(type))
                query = query.Where(t => t.TransactionType == type);

            if (!string.IsNullOrEmpty(branch))
                query = query.Where(t => t.Branch == branch);

            if (fromDate.HasValue)
                query = query.Where(t => t.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.TransactionDate <= toDate.Value);

            return await query.OrderByDescending(t => t.TransactionDate).ToListAsync();
        }
    }
}
