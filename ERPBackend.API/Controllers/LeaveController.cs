using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Xceed.Document.NET;
using Xceed.Words.NET;
using System.IO;
using ERPBackend.Core.Entities;



namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private static bool _fontRegistered = false;

        public LeaveController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            Console.WriteLine("Test endpoint hit at " + DateTime.Now);
            return Ok("Leave API is working");
        }

        [HttpGet("types")]
        public async Task<ActionResult<IEnumerable<LeaveTypeDto>>> GetLeaveTypes()
        {
            var types = await _context.LeaveTypes.Where(t => t.IsActive).ToListAsync();
            var result = types.Select(t => new LeaveTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                YearlyLimit = t.YearlyLimit,
                IsCarryForward = t.IsCarryForward,
                Description = t.Description
            }).ToList();
            return Ok(result);
        }

        [HttpGet("applications/{id}")]
        public async Task<ActionResult<LeaveApplicationDto>> GetApplication(int id)
        {
            var l = await _context.LeaveApplications
                .Include(l => l.Employee)
                .ThenInclude(e => e!.Department)
                .Include(l => l.Employee)
                .ThenInclude(e => e!.Designation)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (l == null) return NotFound();

            return Ok(new LeaveApplicationDto
            {
                Id = l.Id,
                EmployeeCard = l.EmployeeId,
                EmployeeId = l.Employee?.EmployeeId ?? "",
                EmployeeName = l.Employee?.FullNameEn ?? "",
                Department = l.Employee?.Department?.NameEn ?? "",
                Designation = l.Employee?.Designation?.NameEn ?? "",
                LeaveTypeId = l.LeaveTypeId,
                LeaveTypeName = l.LeaveType?.Name ?? "",
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                TotalDays = l.TotalDays,
                Reason = l.Reason,
                Status = l.Status,
                AppliedDate = l.AppliedDate,
                Remarks = l.Remarks,
                AttachmentUrl = l.AttachmentUrl
            });
        }

        [HttpGet("applications")]
        public async Task<ActionResult<IEnumerable<LeaveApplicationDto>>> GetApplications(
            [FromQuery] int? employeeId,
            [FromQuery] string? status)
        {
            var query = _context.LeaveApplications
                .Include(l => l.Employee)
                .ThenInclude(e => e!.Department)
                .Include(l => l.LeaveType)
                .AsQueryable();

            if (employeeId.HasValue) query = query.Where(l => l.EmployeeId == employeeId.Value);
            if (!string.IsNullOrEmpty(status)) query = query.Where(l => l.Status == status);

            var records = await query.OrderByDescending(l => l.AppliedDate).ToListAsync();

            return Ok(records.Select(l => new LeaveApplicationDto
            {
                Id = l.Id,
                EmployeeCard = l.EmployeeId,
                EmployeeId = l.Employee?.EmployeeId ?? "",
                EmployeeName = l.Employee?.FullNameEn ?? "",
                Department = l.Employee?.Department?.NameEn ?? "",
                LeaveTypeId = l.LeaveTypeId,
                LeaveTypeName = l.LeaveType?.Name ?? "",
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                TotalDays = l.TotalDays,
                Reason = l.Reason,
                Status = l.Status,
                AppliedDate = l.AppliedDate,
                Remarks = l.Remarks,
                AttachmentUrl = l.AttachmentUrl
            }));
        }

        [HttpPost("apply")]
        public async Task<ActionResult> ApplyLeave([FromBody] CreateLeaveApplicationDto dto)
        {
            var totalDays = (decimal)(dto.EndDate - dto.StartDate).TotalDays + 1;
            if (totalDays <= 0) return BadRequest("Invalid date range");

            var application = new LeaveApplication
            {
                EmployeeId = dto.EmployeeCard,
                LeaveTypeId = dto.LeaveTypeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TotalDays = totalDays,
                Reason = dto.Reason,
                Status = "Pending",
                AppliedDate = DateTime.UtcNow,
                AttachmentUrl = dto.AttachmentUrl
            };

            _context.LeaveApplications.Add(application);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Leave application submitted successfully." });
        }

        [HttpPost("action")]
        public async Task<ActionResult> ActionLeave([FromBody] LeaveActionDto dto)
        {
            var application = await _context.LeaveApplications.FindAsync(dto.Id);
            if (application == null) return NotFound();

            application.Status = dto.Status;
            application.Remarks = dto.Remarks;
            application.ActionDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Leave application {dto.Status} successfully." });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateLeave(int id, [FromBody] UpdateLeaveApplicationDto dto)
        {
            var application = await _context.LeaveApplications.FindAsync(id);
            if (application == null) return NotFound();
            if (application.Status != "Pending") return BadRequest("Only pending applications can be edited.");

            application.LeaveTypeId = dto.LeaveTypeId;
            application.StartDate = dto.StartDate;
            application.EndDate = dto.EndDate;
            application.TotalDays = (decimal)(dto.EndDate - dto.StartDate).TotalDays + 1;
            application.Reason = dto.Reason;
            application.AttachmentUrl = dto.AttachmentUrl;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Leave application updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteLeave(int id)
        {
            var application = await _context.LeaveApplications.FindAsync(id);
            if (application == null) return NotFound();
            if (application.Status != "Pending") return BadRequest("Only pending applications can be deleted.");

            _context.LeaveApplications.Remove(application);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Leave application deleted successfully." });
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportExcel()
        {
            var applications = await _context.LeaveApplications
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .OrderByDescending(l => l.AppliedDate)
                .ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Leave Applications");

            // Headers
            string[] headers =
            {
                "ID", "Employee ID", "Employee Name", "Type", "Start Date", "End Date", "Days", "Status",
                "Applied Date", "Reason"
            };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Data
            int row = 2;
            foreach (var app in applications)
            {
                worksheet.Cells[row, 1].Value = app.Id;
                worksheet.Cells[row, 2].Value = app.Employee?.EmployeeId;
                worksheet.Cells[row, 3].Value = app.Employee?.FullNameEn;
                worksheet.Cells[row, 4].Value = app.LeaveType?.Name;
                worksheet.Cells[row, 5].Value = app.StartDate.ToString("dd MMM yyyy");
                worksheet.Cells[row, 6].Value = app.EndDate.ToString("dd MMM yyyy");
                worksheet.Cells[row, 7].Value = app.TotalDays;
                worksheet.Cells[row, 8].Value = app.Status;
                worksheet.Cells[row, 9].Value = app.AppliedDate.ToString("dd MMM yyyy");
                worksheet.Cells[row, 10].Value = app.Reason;
                row++;
            }

            worksheet.Cells.AutoFitColumns();
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Leave_Applications.xlsx");
        }

        [HttpGet("export/pdf/{id}")]
        public async Task<IActionResult> ExportPdf(int id)
        {
            try
            {
                var application = await _context.LeaveApplications

                .Include(l => l.Employee)
                .ThenInclude(e => e!.Department)
                .Include(l => l.Employee)
                .ThenInclude(e => e!.Designation)
                .Include(l => l.Employee)
                .ThenInclude(e => e!.Company)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (application == null) return NotFound();

            var employee = application.Employee;
            var company = employee?.Company;
            
            // Fallback to primary company if employee has no company assigned
            if (company == null)
            {
                company = await _context.Companies.FirstOrDefaultAsync(c => c.Branch == ERPBackend.Core.Enums.BranchType.Primary) 
                          ?? await _context.Companies.FirstOrDefaultAsync();
            }

            var balances = await GetLeaveBalanceInternal(application.EmployeeId);

            if (!_fontRegistered)
            {
                var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "SutonnyMJ.ttf");
                if (System.IO.File.Exists(fontPath))
                {
                    using var fontStream = System.IO.File.OpenRead(fontPath);
                    QuestPDF.Drawing.FontManager.RegisterFont(fontStream);
                    _fontRegistered = true;
                }
            }
            
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    
                    // Header using Company Info (Centered)
                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text(company?.CompanyNameBn ?? "BDWwbwU d¨vewiK Bdvóªiz wjwgfUW")
                            .FontFamily("SutonnyMJ").FontSize(28).Bold().FontColor(Colors.Black);
                        
                        column.Item().AlignCenter().Text(company?.AddressBn ?? "evnv`yiæi,wgR©vcyi evRvi, MvRxcyi |")
                            .FontFamily("SutonnyMJ").FontSize(16).FontColor(Colors.Grey.Darken2);
                        
                        column.Item().PaddingTop(10).AlignCenter().Text("QzwUi Av‡e`bcÎ")
                            .FontFamily("SutonnyMJ").FontSize(22).Bold().Underline();
                            
                        column.Item().AlignRight().PaddingRight(10).Row(row => {
                            row.ConstantItem(40).Text("Zvs: ").FontFamily("SutonnyMJ").FontSize(14);
                            row.ConstantItem(100).BorderBottom(1).AlignCenter().Text($"{application.AppliedDate:dd/MM/yyyy}").FontFamily("SutonnyMJ").FontSize(14);
                        });
                    });

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        // Employee Info
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().PaddingRight(10).Row(r => {
                                r.ConstantItem(40).Text("bvg : ").FontFamily("SutonnyMJ").FontSize(14);
                                r.RelativeItem().BorderBottom(1).PaddingBottom(1).Text(employee?.FullNameBn ?? employee?.FullNameEn).FontFamily("SutonnyMJ").FontSize(14);
                            });
                            row.RelativeItem().Row(r => {
                                r.ConstantItem(45).Text("c`ex : ").FontFamily("SutonnyMJ").FontSize(14);
                                r.RelativeItem().BorderBottom(1).PaddingBottom(1).Text(employee?.Designation?.NameBn ?? employee?.Designation?.NameEn).FontFamily("SutonnyMJ").FontSize(14);
                            });
                        });

                        column.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().PaddingRight(10).Row(r => {
                                r.ConstantItem(75).Text("†mKkb/jvBb : ").FontFamily("SutonnyMJ").FontSize(14);
                                r.RelativeItem().BorderBottom(1).PaddingBottom(1).Text(employee?.Department?.NameBn ?? employee?.Department?.NameEn).FontFamily("SutonnyMJ").FontSize(14);
                            });
                            row.RelativeItem().Row(r => {
                                r.ConstantItem(55).Text("KvW© bs: ").FontFamily("SutonnyMJ").FontSize(14);
                                r.RelativeItem().BorderBottom(1).PaddingBottom(1).Text(employee?.EmployeeId).FontFamily("SutonnyMJ").FontSize(14);
                            });
                        });

                        column.Item().PaddingTop(5).Row(row => {
                            row.ConstantItem(65).Text("QzwUi KviY: ").FontFamily("SutonnyMJ").FontSize(14);
                            row.RelativeItem().BorderBottom(1).PaddingBottom(1).Text(application.Reason).FontFamily("SutonnyMJ").FontSize(14);
                        });

                        column.Item().PaddingTop(5).Row(row => {
                            row.ConstantItem(70).Text("QzwUi ZvwiL : ").FontFamily("SutonnyMJ").FontSize(14);
                            row.ConstantItem(100).AlignCenter().BorderBottom(1).Text($"{application.StartDate:dd/MM/yyyy}").FontFamily("SutonnyMJ").FontSize(14);
                            row.ConstantItem(40).AlignCenter().Text(" †_‡K : ").FontFamily("SutonnyMJ").FontSize(14);
                            row.ConstantItem(100).AlignCenter().BorderBottom(1).Text($"{application.EndDate:dd/MM/yyyy}").FontFamily("SutonnyMJ").FontSize(14);
                            row.RelativeItem().PaddingLeft(5).Text(" ch©šÍ").FontFamily("SutonnyMJ").FontSize(14);
                        });

                        column.Item().PaddingTop(5).Row(row => {
                            row.ConstantItem(40).Text("†gvU : ").FontFamily("SutonnyMJ").FontSize(14);
                            row.ConstantItem(50).AlignCenter().BorderBottom(1).Text($"{application.TotalDays}").FontFamily("SutonnyMJ").FontSize(14);
                            row.RelativeItem().PaddingLeft(5).Text(" w`‡bi QzwU gÄyi Kivi mwebq Av‡e`b KiwQ|").FontFamily("SutonnyMJ").FontSize(14);
                        });

                        column.Item().PaddingTop(10).Text("QzwU‡Z _vKvKvjxb wVKvbv :").FontFamily("SutonnyMJ").FontSize(14);
                        column.Item().PaddingTop(2).BorderBottom(1).Height(15);
                        column.Item().PaddingTop(10).BorderBottom(1).Height(15);

                        column.Item().PaddingTop(10).Row(row =>
                        {
                            row.ConstantItem(40).Text("†dvb : ").FontFamily("SutonnyMJ").FontSize(14);
                            row.RelativeItem().BorderBottom(1).Text(" ").FontFamily("SutonnyMJ").FontSize(14);
                            row.RelativeItem().AlignRight().Column(c => {
                                c.Item().PaddingTop(15).Text("Av‡e`bKvixi ^v²i").FontFamily("SutonnyMJ").FontSize(12).Bold();
                            });
                        });

                        column.Item().PaddingVertical(10).LineHorizontal(1);

                        // Office Section
                        column.Item().AlignCenter().Text("GB Ask Awdm KZ…©K c~iY Kiv n‡e")
                            .FontFamily("SutonnyMJ").FontSize(14).Bold();

                        column.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text(t => {
                                t.Span("PvKwi‡Z †hvM`v‡bi ZvwiL: ").FontFamily("SutonnyMJ").FontSize(12);
                                t.Span(" [                    ] ").FontSize(12);
                            });
                            row.RelativeItem().Text(t => {
                                t.Span("QzwUi wnmveKvj: ").FontFamily("SutonnyMJ").FontSize(12);
                                t.Span(" [                    ] ").FontSize(12);
                            });
                            row.RelativeItem().Text(t => {
                                t.Span("nBfZ: ").FontFamily("SutonnyMJ").FontSize(12);
                                t.Span(" [                    ] ").FontSize(12);
                            });
                        });

                        // Balances Table
                        var casual = balances.FirstOrDefault(b => b.LeaveTypeName.Contains("Casual")) ?? new LeaveBalanceDto();
                        var sick = balances.FirstOrDefault(b => b.LeaveTypeName.Contains("Sick")) ?? new LeaveBalanceDto();
                        var earned = balances.FirstOrDefault(b => b.LeaveTypeName.Contains("Earn")) ?? new LeaveBalanceDto();
                        var maternity = balances.FirstOrDefault(b => b.LeaveTypeName.Contains("Maternity")) ?? new LeaveBalanceDto();

                        column.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header => {
                                header.Cell().Text("QzwUi weeiY   :").FontFamily("SutonnyMJ").FontSize(12);
                                header.Cell().AlignCenter().Text("‰bwgwËK QzwU").FontFamily("SutonnyMJ").FontSize(10);
                                header.Cell().AlignCenter().Text("cxov-QzwU").FontFamily("SutonnyMJ").FontSize(10);
                                header.Cell().AlignCenter().Text("AwR©Z QzwU").FontFamily("SutonnyMJ").FontSize(10);
                                header.Cell().AlignCenter().Text("gvZ…Z¡RwbZ QzwU").FontFamily("SutonnyMJ").FontSize(10);
                            });

                            // Row 1: cvc¨ QzwU
                            table.Cell().Row(1).Column(1).PaddingVertical(2).Text("cvc¨ QzwU :").FontFamily("SutonnyMJ").FontSize(12);
                            table.Cell().Row(1).Column(2).Padding(2).Border(1f).AlignCenter().Text($"{casual.TotalAllocated}").FontSize(12);
                            table.Cell().Row(1).Column(3).Padding(2).Border(1f).AlignCenter().Text($"{sick.TotalAllocated}").FontSize(12);
                            table.Cell().Row(1).Column(4).Padding(2).Border(1f).AlignCenter().Text($"{earned.TotalAllocated}").FontSize(12);
                            table.Cell().Row(1).Column(5).Padding(2).Border(1f).AlignCenter().Text($"{maternity.TotalAllocated}").FontSize(12);

                            // Row 2: †fvMK…Z QzwU
                            table.Cell().Row(2).Column(1).PaddingVertical(2).Text("†fvMK…Z QzwU :").FontFamily("SutonnyMJ").FontSize(12);
                            table.Cell().Row(2).Column(2).Padding(2).Border(1f).AlignCenter().Text($"{casual.TotalTaken}").FontSize(12);
                            table.Cell().Row(2).Column(3).Padding(2).Border(1f).AlignCenter().Text($"{sick.TotalTaken}").FontSize(12);
                            table.Cell().Row(2).Column(4).Padding(2).Border(1f).AlignCenter().Text($"{earned.TotalTaken}").FontSize(12);
                            table.Cell().Row(2).Column(5).Padding(2).Border(1f).AlignCenter().Text($"{maternity.TotalTaken}").FontSize(12);

                            // Row 3: Aewkó QzwU
                            table.Cell().Row(3).Column(1).PaddingVertical(2).Text("Aewkó QzwU :").FontFamily("SutonnyMJ").FontSize(12);
                            table.Cell().Row(3).Column(2).Padding(2).Border(1f).AlignCenter().Text($"{casual.Balance}").FontSize(12);
                            table.Cell().Row(3).Column(3).Padding(2).Border(1f).AlignCenter().Text($"{sick.Balance}").FontSize(12);
                            table.Cell().Row(3).Column(4).Padding(2).Border(1f).AlignCenter().Text($"{earned.Balance}").FontSize(12);
                            table.Cell().Row(3).Column(5).Padding(2).Border(1f).AlignCenter().Text($"{maternity.Balance}").FontSize(12);
                        });

                        column.Item().PaddingTop(10).Row(row => {
                            row.ConstantItem(50).Border(1f).PaddingHorizontal(5).AlignCenter().Text(" ").FontSize(12);
                            row.RelativeItem().PaddingLeft(5).Text(" w`‡bi ‰bwgwËK/ cxov/ AwR©Z/ gvZ…Z¡RwbZ QzwU gÄyi Kiv nBj|").FontFamily("SutonnyMJ").FontSize(12);
                        });

                        column.Item().PaddingTop(15).Row(row =>
                        {
                            row.RelativeItem().AlignCenter().Text("GBP Avi kvLv").FontFamily("SutonnyMJ").FontSize(9);
                            row.RelativeItem().AlignCenter().Text("BbPvR©").FontFamily("SutonnyMJ").FontSize(9);
                            row.RelativeItem().AlignCenter().Text("GwcGg/wcGg").FontFamily("SutonnyMJ").FontSize(9);
                            row.RelativeItem().AlignCenter().Text("wefvMxq cÖavb").FontFamily("SutonnyMJ").FontSize(9);
                            row.RelativeItem().AlignCenter().Text("wefvMxq cÖavb(cÖ)").FontFamily("SutonnyMJ").FontSize(9);
                            row.RelativeItem().AlignCenter().Text("wR Gg").FontFamily("SutonnyMJ").FontSize(9);
                        });

                        column.Item().PaddingVertical(10).LineHorizontal(1);

                        // Report Section
                        column.Item().AlignCenter().Text(company?.CompanyNameBn ?? "BDWwbwU d¨vewiK Bdvóªiz wjwgfUW")
                            .FontFamily("SutonnyMJ").FontSize(18).Bold();
                        column.Item().AlignCenter().Text("QzwU †_‡K Kv‡R †hvM`v‡bi cÖwZ‡e`b")
                            .FontFamily("SutonnyMJ").FontSize(14).Underline();

                        column.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Row(r => {
                                r.ConstantItem(30).Text("bvg : ").FontFamily("SutonnyMJ").FontSize(12);
                                r.RelativeItem().BorderBottom(1).Text(employee?.FullNameBn ?? employee?.FullNameEn).FontFamily("SutonnyMJ").FontSize(12);
                            });
                            row.RelativeItem().Row(r => {
                                r.ConstantItem(45).Text("KvW© bs: ").FontFamily("SutonnyMJ").FontSize(12);
                                r.RelativeItem().BorderBottom(1).Text(employee?.EmployeeId).FontFamily("SutonnyMJ").FontSize(12);
                            });
                            row.RelativeItem().Row(r => {
                                r.ConstantItem(65).Text("Bm¨zi ZvwiL : ").FontFamily("SutonnyMJ").FontSize(12);
                                r.RelativeItem().BorderBottom(1).Text(" ").FontFamily("SutonnyMJ").FontSize(12);
                            });
                        });

                        column.Item().PaddingTop(5).Row(row => {
                            row.ConstantItem(180).Text("gÄziK…Z QzwU Abyhvqx †hvM`v‡bi ZvwiL : ").FontFamily("SutonnyMJ").FontSize(12);
                            row.RelativeItem().BorderBottom(1).Text(" ").FontFamily("SutonnyMJ").FontSize(12);
                        });
                        column.Item().PaddingTop(5).Row(row => {
                            row.ConstantItem(120).Text("†hvM`v‡bi cÖK…Z ZvwiL : ").FontFamily("SutonnyMJ").FontSize(12);
                            row.RelativeItem().BorderBottom(1).Text(" ").FontFamily("SutonnyMJ").FontSize(12);
                        });

                        column.Item().PaddingTop(15).Row(row =>
                        {
                            row.RelativeItem().Column(c => {
                                c.Item().AlignCenter().BorderBottom(1).Height(12);
                                c.Item().AlignCenter().Text("Av‡e`bKvixi ^v²i").FontFamily("SutonnyMJ").FontSize(10);
                            });
                            row.RelativeItem(2).PaddingHorizontal(5).AlignCenter().Text("GB Askটি QzwU †_‡K Kv‡R †hvM`v‡bi mgq cÖkvmb kvLvq Rgv w`fZ n‡e|")
                                .FontFamily("SutonnyMJ").FontSize(10);
                            row.RelativeItem().Column(c => {
                                c.Item().AlignCenter().BorderBottom(1).Height(12);
                                c.Item().AlignCenter().Text("GBP Avi kvLv").FontFamily("SutonnyMJ").FontSize(10);
                            });
                        });
                    });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"Leave_Application_{application.Id}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF Error for ID {id}: {ex}");
                return StatusCode(500, new { message = "Error generating PDF", details = ex.Message });
            }
        }


        private async Task<IEnumerable<LeaveBalanceDto>> GetLeaveBalanceInternal(int employeeId)
        {
            var types = await _context.LeaveTypes.Where(t => t.IsActive).ToListAsync();
            var applications = await _context.LeaveApplications
                .Where(l => l.EmployeeId == employeeId && l.Status == "Approved" &&
                            l.StartDate.Year == DateTime.Now.Year)
                .ToListAsync();

            var balances = types.Select(t => new LeaveBalanceDto
            {
                LeaveTypeId = t.Id,
                LeaveTypeName = t.Name,
                TotalAllocated = t.YearlyLimit,
                TotalTaken = applications.Where(a => a.LeaveTypeId == t.Id).Sum(a => a.TotalDays),
                Balance = t.YearlyLimit - applications.Where(a => a.LeaveTypeId == t.Id).Sum(a => a.TotalDays)
            }).ToList();


            return balances;
        }




        [HttpGet("balance/{employeeId}")]
        public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetLeaveBalance(int employeeId)
        {
            var types = await _context.LeaveTypes.Where(t => t.IsActive).ToListAsync();
            var applications = await _context.LeaveApplications
                .Where(l => l.EmployeeId == employeeId && l.Status == "Approved" &&
                            l.StartDate.Year == DateTime.Now.Year)
                .ToListAsync();

            var balances = types.Select(t => new LeaveBalanceDto
            {
                LeaveTypeId = t.Id,
                LeaveTypeName = t.Name,
                TotalAllocated = t.YearlyLimit,
                TotalTaken = applications.Where(a => a.LeaveTypeId == t.Id).Sum(a => a.TotalDays),
                Balance = t.YearlyLimit - applications.Where(a => a.LeaveTypeId == t.Id).Sum(a => a.TotalDays)
            }).ToList();

            return Ok(balances);
        }

        [HttpPost("seed")]
        public async Task<ActionResult> SeedLeaveTypes()
        {
            var types = new List<LeaveType>
            {
                new() { Name = "Casual Leave", Code = "CL", YearlyLimit = 10, IsCarryForward = false },
                new() { Name = "Sick Leave", Code = "SL", YearlyLimit = 14, IsCarryForward = false },
                new() { Name = "Earn Leave", Code = "EL", YearlyLimit = 20, IsCarryForward = true },
                new() { Name = "Paternity leave", Code = "PL", YearlyLimit = 5, IsCarryForward = false },
                new() { Name = "Maturnity Leave", Code = "ML", YearlyLimit = 112, IsCarryForward = false },
                new() { Name = "Leave without Pay", Code = "LWP", YearlyLimit = 365, IsCarryForward = false },
            };

            foreach (var t in types)
            {
                var existing = await _context.LeaveTypes.FirstOrDefaultAsync(x => x.Code == t.Code);
                if (existing != null)
                {
                    existing.Name = t.Name;
                    existing.YearlyLimit = t.YearlyLimit;
                    existing.IsCarryForward = t.IsCarryForward;
                    existing.IsActive = true;
                    _context.LeaveTypes.Update(existing);
                }
                else
                {
                    _context.LeaveTypes.Add(t);
                }
            }
            
            var allTypes = await _context.LeaveTypes.ToListAsync();
            foreach (var existing in allTypes)
            {
                if (!types.Any(t => t.Code == existing.Code))
                {
                    existing.IsActive = false;
                    _context.LeaveTypes.Update(existing);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Leave types updated successfully." });
        }

        [HttpGet("monthly-report")]
        public async Task<ActionResult> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var applications = await _context.LeaveApplications
                .Include(l => l.Employee)
                .ThenInclude(e => e!.Department)
                .Include(l => l.LeaveType)
                .Where(l => l.Status == "Approved" &&
                            ((l.StartDate >= startDate && l.StartDate <= endDate) ||
                             (l.EndDate >= startDate && l.EndDate <= endDate)))
                .ToListAsync();

            var report = applications
                .GroupBy(l => new
                {
                    l.EmployeeId, l.Employee!.FullNameEn, EmployeeIdCard = l.Employee.EmployeeId,
                    Dept = l.Employee.Department?.NameEn ?? "N/A"
                })
                .Select(g => new
                {
                    EmployeeCard = g.Key.EmployeeId,
                    EmployeeId = g.Key.EmployeeIdCard,
                    EmployeeName = g.Key.FullNameEn,
                    Department = g.Key.Dept,
                    SickLeave = g.Where(x => x.LeaveType!.Code == "SL").Sum(x => x.TotalDays),
                    CasualLeave = g.Where(x => x.LeaveType!.Code == "CL").Sum(x => x.TotalDays),
                    EarnedLeave = g.Where(x => x.LeaveType!.Code == "EL").Sum(x => x.TotalDays),
                    PaternityLeave = g.Where(x => x.LeaveType!.Code == "PL").Sum(x => x.TotalDays),
                    MaternityLeave = g.Where(x => x.LeaveType!.Code == "ML").Sum(x => x.TotalDays),
                    LWP = g.Where(x => x.LeaveType!.Code == "LWP").Sum(x => x.TotalDays),
                    OtherLeave = g.Where(x => 
                        !new[] {"SL", "CL", "EL", "PL", "ML", "LWP"}.Contains(x.LeaveType!.Code)
                    ).Sum(x => x.TotalDays),
                    TotalDays = g.Sum(x => x.TotalDays)
                })
                .ToList();

            return Ok(report);
        }
    }
}
