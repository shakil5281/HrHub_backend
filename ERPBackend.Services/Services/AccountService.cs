using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace ERPBackend.Services.Services
{
    public class AccountService : IAccountService
    {
        private readonly CashbookDbContext _context;
        private readonly ApplicationDbContext _appContext;

        public AccountService(CashbookDbContext context, ApplicationDbContext appContext)
        {
            _context = context;
            _appContext = appContext;
        }

        #region Branch Management
        public async Task<List<BranchDto>> GetBranchesAsync()
        {
            // Fetch all companies from management database
            var companies = await _appContext.Companies.ToListAsync();
            
            // Get current branches from accounts database
            var branches = await _context.Branches.ToListAsync();
            bool changed = false;

            // Ensure every company has a corresponding branch record in the accounts DB
            foreach (var company in companies) {
                if (!branches.Any(b => b.BranchName == company.CompanyNameEn)) {
                    var newBranch = new Branch {
                        BranchName = company.CompanyNameEn,
                        BranchCode = company.RegistrationNo,
                        Address = company.AddressEn,
                        Phone = company.PhoneNumber,
                        InitialBalance = 0,
                        CurrentBalance = 0,
                        IsActive = company.Status == "Active",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Branches.Add(newBranch);
                    changed = true;
                }
            }

            if (changed) {
                await _context.SaveChangesAsync();
                // Refresh branches list after insertion to get actual IDs
                branches = await _context.Branches.ToListAsync();
            }

            // Return the synchronized list
            return companies.Select(c => {
                var branch = branches.FirstOrDefault(b => b.BranchName == c.CompanyNameEn);
                return new BranchDto
                {
                    Id = branch?.Id ?? 0, // Should not be 0 due to synchronization above
                    BranchName = c.CompanyNameEn,
                    BranchCode = c.RegistrationNo,
                    Address = c.AddressEn,
                    Phone = c.PhoneNumber,
                    InitialBalance = branch?.InitialBalance ?? 0,
                    CurrentBalance = branch?.CurrentBalance ?? 0,
                    IsActive = c.Status == "Active"
                };
            }).ToList();
        }

        public async Task<BranchDto> AddBranchAsync(BranchDto branch)
        {
            var entity = new Branch
            {
                BranchName = branch.BranchName,
                BranchCode = branch.BranchCode,
                Address = branch.Address,
                Phone = branch.Phone,
                InitialBalance = branch.InitialBalance,
                CurrentBalance = branch.InitialBalance, // Start with initial
                IsActive = true
            };
            _context.Branches.Add(entity);
            await _context.SaveChangesAsync();
            branch.Id = entity.Id;
            return branch;
        }

        public async Task<bool> UpdateBranchAsync(BranchDto branch)
        {
            var entity = await _context.Branches.FindAsync(branch.Id);
            if (entity == null) return false;

            entity.BranchName = branch.BranchName;
            entity.BranchCode = branch.BranchCode;
            entity.Address = branch.Address;
            entity.Phone = branch.Phone;
            entity.IsActive = branch.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBranchAsync(int id)
        {
            var entity = await _context.Branches.FindAsync(id);
            if (entity == null) return false;
            _context.Branches.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Transactions
        public async Task<List<AccountTransactionDto>> GetTransactionsAsync(string? type = null, string? fundSource = null)
        {
            var query = _context.AccountTransactions.Include(t => t.Branch).AsQueryable();

            if (!string.IsNullOrEmpty(type))
            {
                if (Enum.TryParse<AccountTransactionType>(type, true, out var tEnum))
                    query = query.Where(t => t.Type == tEnum);
            }

            if (!string.IsNullOrEmpty(fundSource))
            {
                if (Enum.TryParse<FundType>(fundSource, true, out var fEnum))
                    query = query.Where(t => t.FundSource == fEnum);
            }

            return await query
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new AccountTransactionDto
                {
                    Id = t.Id,
                    TransactionNumber = t.TransactionNumber,
                    TransactionDate = t.TransactionDate,
                    Type = t.Type.ToString(),
                    FundSource = t.FundSource.ToString(),
                    BranchId = t.BranchId,
                    BranchName = t.Branch != null ? t.Branch.BranchName : "",
                    Amount = t.Amount,
                    Category = t.Category,
                    ReferenceNumber = t.ReferenceNumber,
                    Description = t.Description,
                    PreparedBy = t.PreparedBy,
                    CreatedAt = t.CreatedAt
                }).ToListAsync();
        }

        public async Task<AccountTransactionDto?> GetTransactionByIdAsync(int id)
        {
            var t = await _context.AccountTransactions
                .Include(tx => tx.Branch)
                .FirstOrDefaultAsync(tx => tx.Id == id);

            if (t == null) return null;

            return new AccountTransactionDto
            {
                Id = t.Id,
                TransactionNumber = t.TransactionNumber,
                TransactionDate = t.TransactionDate,
                Type = t.Type.ToString(),
                FundSource = t.FundSource.ToString(),
                BranchId = t.BranchId,
                BranchName = t.Branch != null ? t.Branch.BranchName : "",
                Amount = t.Amount,
                Category = t.Category,
                ReferenceNumber = t.ReferenceNumber,
                Description = t.Description,
                PreparedBy = t.PreparedBy,
                CreatedAt = t.CreatedAt
            };
        }

        public async Task<AccountTransactionDto> CreateTransactionAsync(AccountTransactionDto transaction)
        {
            if (!Enum.TryParse<AccountTransactionType>(transaction.Type, true, out var typeEnum))
                throw new Exception("Invalid transaction type");

            if (!Enum.TryParse<FundType>(transaction.FundSource, true, out var fundEnum))
                throw new Exception("Invalid fund source");

            var entity = new AccountTransaction
            {
                TransactionNumber = "TRX-" + DateTime.Now.Ticks.ToString().Substring(10),
                TransactionDate = transaction.TransactionDate,
                Type = typeEnum,
                FundSource = fundEnum,
                BranchId = transaction.BranchId,
                Amount = transaction.Amount,
                Category = transaction.Category,
                ReferenceNumber = transaction.ReferenceNumber,
                Description = transaction.Description,
                PreparedBy = transaction.PreparedBy
            };

            // Update Branch Balance if applicable
            if (transaction.BranchId.HasValue)
            {
                var branch = await _context.Branches.FindAsync(transaction.BranchId.Value);
                if (branch != null)
                {
                    if (typeEnum == AccountTransactionType.Receive || typeEnum == AccountTransactionType.Income)
                        branch.CurrentBalance += transaction.Amount;
                    else if (typeEnum == AccountTransactionType.Payment || typeEnum == AccountTransactionType.Expense)
                        branch.CurrentBalance -= transaction.Amount;
                }
            }

            _context.AccountTransactions.Add(entity);
            await _context.SaveChangesAsync();
            transaction.Id = entity.Id;
            transaction.TransactionNumber = entity.TransactionNumber;
            return transaction;
        }
        #endregion

        #region Advance Payments
        public async Task<List<AdvancePaymentDto>> GetAdvancesAsync()
        {
            return await _context.AdvancePayments
                .Select(a => new AdvancePaymentDto
                {
                    Id = a.Id,
                    EmployeeOrContractorName = a.EmployeeOrContractorName,
                    Date = a.Date,
                    TotalAmount = a.TotalAmount,
                    PaidAmount = a.PaidAmount,
                    DueAmount = a.DueAmount,
                    PaymentType = a.PaymentType,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt
                }).ToListAsync();
        }

        public async Task<AdvancePaymentDto> CreateAdvanceAsync(AdvancePaymentDto advance)
        {
            var entity = new AdvancePayment
            {
                EmployeeOrContractorName = advance.EmployeeOrContractorName,
                Date = advance.Date,
                TotalAmount = advance.TotalAmount,
                PaidAmount = advance.PaidAmount,
                DueAmount = advance.TotalAmount - advance.PaidAmount,
                PaymentType = advance.PaymentType,
                Status = (advance.TotalAmount - advance.PaidAmount) <= 0 ? "Settled" : "Pending"
            };
            _context.AdvancePayments.Add(entity);
            await _context.SaveChangesAsync();
            advance.Id = entity.Id;
            return advance;
        }
        #endregion

        #region Categories
        public async Task<List<string>> GetIncomeCategoriesAsync()
        {
            return await _context.IncomeCategories.Where(c => c.IsActive).Select(c => c.Name).ToListAsync();
        }

        public async Task<List<string>> GetExpenseCategoriesAsync()
        {
            return await _context.ExpenseCategories.Where(c => c.IsActive).Select(c => c.Name).ToListAsync();
        }
        #endregion

        #region Dashboard & Reports
        public async Task<AccountDashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var today = DateTime.Today;
            return new AccountDashboardSummaryDto
            {
                TotalCashBalance = await _context.AccountTransactions.Where(t => t.FundSource == FundType.Cash).SumAsync(t => (t.Type == AccountTransactionType.Receive || t.Type == AccountTransactionType.Income ? 1 : -1) * t.Amount),
                TotalBankBalance = await _context.AccountTransactions.Where(t => t.FundSource == FundType.Bank).SumAsync(t => (t.Type == AccountTransactionType.Receive || t.Type == AccountTransactionType.Income ? 1 : -1) * t.Amount),
                TotalHandCash = await _context.AccountTransactions.Where(t => t.FundSource == FundType.HandCash).SumAsync(t => (t.Type == AccountTransactionType.Receive || t.Type == AccountTransactionType.Income ? 1 : -1) * t.Amount),
                TodaysReceive = await _context.AccountTransactions.Where(t => t.TransactionDate.Date == today && (t.Type == AccountTransactionType.Receive || t.Type == AccountTransactionType.Income)).SumAsync(t => t.Amount),
                TodaysPayment = await _context.AccountTransactions.Where(t => t.TransactionDate.Date == today && (t.Type == AccountTransactionType.Payment || t.Type == AccountTransactionType.Expense)).SumAsync(t => t.Amount),
                ActiveAdvances = await _context.AdvancePayments.Where(a => a.Status == "Pending").SumAsync(a => a.DueAmount)
            };
        }

        public async Task<List<GeneralReportDto>> GetLedgerReportAsync(int? branchId, string? fundSource)
        {
            var query = _context.AccountTransactions.AsQueryable();
            if (branchId.HasValue) query = query.Where(t => t.BranchId == branchId.Value);
            
            if (!string.IsNullOrEmpty(fundSource) && Enum.TryParse<FundType>(fundSource, true, out var fEnum))
                query = query.Where(t => t.FundSource == fEnum);

            return await query.Select(t => new GeneralReportDto
            {
                Title = t.Description ?? t.TransactionNumber,
                Amount = (t.Type == AccountTransactionType.Receive || t.Type == AccountTransactionType.Income ? 1 : -1) * t.Amount,
                Category = t.Category,
                Date = t.TransactionDate
            }).ToListAsync();
        }
        #endregion

        public async Task<byte[]> ExportTransactionsToExcelAsync(string? type = null, string? fundSource = null)
        {
            var transactions = await GetTransactionsAsync(type, fundSource);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Transactions");

                // Headers
                worksheet.Cells[1, 1].Value = "Voucher #";
                worksheet.Cells[1, 2].Value = "Date";
                worksheet.Cells[1, 3].Value = "Type";
                worksheet.Cells[1, 4].Value = "Category";
                worksheet.Cells[1, 5].Value = "Branch";
                worksheet.Cells[1, 6].Value = "Source";
                worksheet.Cells[1, 7].Value = "Amount";
                worksheet.Cells[1, 8].Value = "Description";

                using (var range = worksheet.Cells[1, 1, 1, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                int row = 2;
                foreach (var t in transactions)
                {
                    worksheet.Cells[row, 1].Value = t.TransactionNumber;
                    worksheet.Cells[row, 2].Value = t.TransactionDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 3].Value = t.Type;
                    worksheet.Cells[row, 4].Value = t.Category;
                    worksheet.Cells[row, 5].Value = t.BranchName;
                    worksheet.Cells[row, 6].Value = t.FundSource;
                    worksheet.Cells[row, 7].Value = t.Amount;
                    worksheet.Cells[row, 8].Value = t.Description;
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                return package.GetAsByteArray();
            }
        }

        public async Task<byte[]> ExportTransactionsToPdfAsync(string? type = null, string? fundSource = null)
        {
            var transactions = await GetTransactionsAsync(type, fundSource);
            var title = type == "Receive" ? "Received Transactions Report" : "Expenditure Report";

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(title).FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}");
                        });
                    });

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80); // Voucher
                            columns.ConstantColumn(70); // Date
                            columns.RelativeColumn();   // Branch/Category
                            columns.ConstantColumn(80); // Amount
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Voucher #");
                            header.Cell().Element(CellStyle).Text("Date");
                            header.Cell().Element(CellStyle).Text("Details");
                            header.Cell().Element(CellStyle).AlignRight().Text("Amount");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            }
                        });

                        foreach (var t in transactions)
                        {
                            table.Cell().Element(Padding).Text(t.TransactionNumber);
                            table.Cell().Element(Padding).Text(t.TransactionDate.ToString("yyyy-MM-dd"));
                            table.Cell().Element(Padding).Column(col => {
                                col.Item().Text(t.Category).SemiBold();
                                col.Item().Text(t.BranchName).FontSize(8);
                            });
                            table.Cell().Element(Padding).AlignRight().Text($"{t.Amount:N2} ৳");

                            static IContainer Padding(IContainer container)
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
            }).GeneratePdf();
        }

        public async Task<byte[]> ExportVoucherExcelAsync(int id)
        {
            var t = await GetTransactionByIdAsync(id);
            if (t == null) throw new Exception("Transaction not found");

            // Fetch company details for header
            var company = await _appContext.Companies.FirstOrDefaultAsync(c => c.CompanyNameEn == t.BranchName) 
                          ?? await _appContext.Companies.FirstOrDefaultAsync(c => c.Status == "Active")
                          ?? await _appContext.Companies.FirstOrDefaultAsync();

            string companyName = company?.CompanyNameEn ?? "HRHub ERP System";
            string companyAddress = company?.AddressEn ?? "Dhaka, Bangladesh";

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Voucher");

                // Page Setup
                worksheet.PrinterSettings.PaperSize = ePaperSize.A4;
                worksheet.PrinterSettings.Orientation = eOrientation.Portrait;
                worksheet.PrinterSettings.LeftMargin = 0.5M;
                worksheet.PrinterSettings.RightMargin = 0.5M;
                worksheet.PrinterSettings.TopMargin = 0.5M;
                worksheet.PrinterSettings.BottomMargin = 0.5M;

                // Column Widths
                worksheet.Column(1).Width = 20;
                worksheet.Column(2).Width = 60;

                // --- Header ---
                // Company Name
                worksheet.Cells["A1:B1"].Merge = true;
                worksheet.Cells["A1"].Value = companyName;
                worksheet.Cells["A1"].Style.Font.Size = 18;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Company Address
                worksheet.Cells["A2:B2"].Merge = true;
                worksheet.Cells["A2"].Value = companyAddress;
                worksheet.Cells["A2"].Style.Font.Size = 10;
                worksheet.Cells["A2"].Style.Font.Color.SetColor(System.Drawing.Color.Gray);
                worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Document Title
                worksheet.Cells["A4:B4"].Merge = true;
                worksheet.Cells["A4"].Value = "TRANSACTION VOUCHER";
                worksheet.Cells["A4"].Style.Font.Size = 14;
                worksheet.Cells["A4"].Style.Font.Bold = true;
                worksheet.Cells["A4"].Style.Font.UnderLine = true;
                worksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // --- Transaction Meta ---
                int startRow = 6;
                void AddField(string label, string? value, int row, bool highlight = false)
                {
                    worksheet.Cells[row, 1].Value = label;
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.Font.Size = 10;
                    
                    worksheet.Cells[row, 2].Value = value ?? "N/A";
                    worksheet.Cells[row, 2].Style.Font.Size = 10;
                    if (highlight) {
                        worksheet.Cells[row, 2].Style.Font.Bold = true;
                        worksheet.Cells[row, 2].Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
                    }
                }

                AddField("Voucher Number:", t.TransactionNumber, startRow);
                AddField("Transaction Date:", t.TransactionDate.ToString("dd MMM yyyy"), startRow + 1);
                AddField("Transaction Type:", t.Type, startRow + 2, true);
                AddField("Category:", t.Category, startRow + 3);
                AddField("Branch:", t.BranchName, startRow + 4);
                AddField("Fund Source:", t.FundSource, startRow + 5);
                AddField("Reference:", t.ReferenceNumber, startRow + 6);

                // --- Main Amount & Description Block ---
                int amountRow = startRow + 8;
                worksheet.Cells[amountRow, 1, amountRow + 1, 2].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[amountRow, 1, amountRow + 1, 2].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[amountRow, 1, amountRow + 1, 2].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[amountRow, 1, amountRow + 1, 2].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                worksheet.Cells[amountRow, 1].Value = "DESCRIPTION / NARRATIVE";
                worksheet.Cells[amountRow, 1].Style.Font.Bold = true;
                worksheet.Cells[amountRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[amountRow, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                worksheet.Cells[amountRow, 2].Value = "AMOUNT (BDT)";
                worksheet.Cells[amountRow, 2].Style.Font.Bold = true;
                worksheet.Cells[amountRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[amountRow, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[amountRow, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                worksheet.Cells[amountRow + 1, 1].Value = t.Description ?? "No description provided.";
                worksheet.Cells[amountRow + 1, 1].Style.WrapText = true;
                worksheet.Row(amountRow + 1).Height = 40;

                worksheet.Cells[amountRow + 1, 2].Value = t.Amount;
                worksheet.Cells[amountRow + 1, 2].Style.Numberformat.Format = "#,##0.00 ৳";
                worksheet.Cells[amountRow + 1, 2].Style.Font.Bold = true;
                worksheet.Cells[amountRow + 1, 2].Style.Font.Size = 12;
                worksheet.Cells[amountRow + 1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[amountRow + 1, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // --- Footer Signature Area ---
                int footerRow = amountRow + 6;
                worksheet.Cells[footerRow, 1].Value = "________________________";
                worksheet.Cells[footerRow + 1, 1].Value = "Prepared By";
                worksheet.Cells[footerRow + 1, 1].Style.Font.Bold = true;
                worksheet.Cells[footerRow + 2, 1].Value = t.PreparedBy ?? "System";
                worksheet.Cells[footerRow + 2, 1].Style.Font.Italic = true;
                worksheet.Cells[footerRow + 2, 1].Style.Font.Size = 8;

                worksheet.Cells[footerRow, 2].Value = "________________________";
                worksheet.Cells[footerRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[footerRow + 1, 2].Value = "Authorized Signature";
                worksheet.Cells[footerRow + 1, 2].Style.Font.Bold = true;
                worksheet.Cells[footerRow + 1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                // Generation Info
                worksheet.Cells[footerRow + 5, 1, footerRow + 5, 2].Merge = true;
                worksheet.Cells[footerRow + 5, 1].Value = $"Generated from HRHub ERP on {DateTime.Now:dd-MM-yyyy HH:mm}";
                worksheet.Cells[footerRow + 5, 1].Style.Font.Size = 7;
                worksheet.Cells[footerRow + 5, 1].Style.Font.Color.SetColor(System.Drawing.Color.Gray);
                worksheet.Cells[footerRow + 5, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                return package.GetAsByteArray();
            }
        }

        public async Task<byte[]> ExportVoucherPdfAsync(int id)
        {
            var t = await GetTransactionByIdAsync(id);
            if (t == null) throw new Exception("Transaction not found");

            // Fetch company details for header
            var company = await _appContext.Companies.FirstOrDefaultAsync(c => c.CompanyNameEn == t.BranchName) 
                          ?? await _appContext.Companies.FirstOrDefaultAsync(c => c.Status == "Active")
                          ?? await _appContext.Companies.FirstOrDefaultAsync();

            string companyName = company?.CompanyNameEn ?? "HRHub ERP System";
            string companyAddress = company?.AddressEn ?? "Dhaka, Bangladesh";

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    // Header: Company Details (Simplified)
                    page.Header().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(companyName).FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                            col.Item().Text(companyAddress).FontSize(8).FontColor(Colors.Grey.Medium);
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("TRANSACTION VOUCHER").FontSize(12).ExtraBold().FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"Voucher: {t.TransactionNumber}").FontSize(8).SemiBold();
                            col.Item().Text($"Date: {t.TransactionDate:dd MMM yyyy}").FontSize(8);
                        });
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        // Centered Key Information
                        col.Item().PaddingBottom(10).AlignCenter().Row(row => {
                             row.ConstantItem(100).AlignCenter().Column(c => {
                                 c.Item().Text("TYPE").FontSize(7).SemiBold().FontColor(Colors.Grey.Medium);
                                 c.Item().Text(t.Type?.ToUpper() ?? "N/A").FontSize(10).Bold().FontColor((t.Type ?? "").Contains("Receive") ? Colors.Green.Darken2 : Colors.Red.Darken2);
                             });
                             row.RelativeItem().AlignCenter().Column(c => {
                                 c.Item().Text("CATEGORY").FontSize(7).SemiBold().FontColor(Colors.Grey.Medium);
                                 c.Item().Text(t.Category ?? "General").FontSize(10).SemiBold();
                             });
                             row.ConstantItem(100).AlignCenter().Column(c => {
                                 c.Item().Text("FUND SOURCE").FontSize(7).SemiBold().FontColor(Colors.Grey.Medium);
                                 c.Item().Text(t.FundSource ?? "N/A").FontSize(10).SemiBold();
                             });
                        });

                        // Details Grid
                        col.Item().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("Description / Narrative").FontSize(8).Bold();
                                header.Cell().Background(Colors.Grey.Lighten4).Padding(5).AlignRight().Text("Amount").FontSize(8).Bold();
                            });

                            table.Cell().Padding(8).Text(t.Description ?? "No description provided.").Italic();
                            table.Cell().Padding(8).AlignRight().AlignMiddle().Text($"{t.Amount:N2} ৳").FontSize(12).Bold();
                        });

                        // Reference & Meta
                        col.Item().PaddingTop(5).Row(row => {
                            row.RelativeItem().Text($"Reference: {t.ReferenceNumber ?? "N/A"}").FontSize(8).FontColor(Colors.Grey.Medium);
                            row.RelativeItem().AlignRight().Text($"Branch: {t.BranchName ?? "Main"}").FontSize(8).FontColor(Colors.Grey.Medium);
                        });

                        // Simple Signature Section
                        col.Item().PaddingTop(50).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().PaddingBottom(2).Text("________________________").FontSize(9);
                                c.Item().Text("Prepared By").FontSize(8).SemiBold();
                                c.Item().Text(t.PreparedBy ?? "System").FontSize(7).Italic();
                            });

                            row.ConstantItem(150);

                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().PaddingBottom(2).Text("________________________").FontSize(9);
                                c.Item().Text("Authorized Signature").FontSize(8).SemiBold();
                            });
                        });
                    });

                    page.Footer().PaddingTop(20).AlignCenter().Text(x =>
                    {
                        x.Span("Generated via HRHub ERP on ").FontSize(7).FontColor(Colors.Grey.Medium);
                        x.Span(DateTime.Now.ToString("dd-MM-yyyy HH:mm")).FontSize(7).SemiBold().FontColor(Colors.Grey.Medium);
                        x.Span(" | Page ").FontSize(7).FontColor(Colors.Grey.Medium);
                        x.CurrentPageNumber().FontSize(7).FontColor(Colors.Grey.Medium);
                    });
                });
            }).GeneratePdf();
        }
    }
}
