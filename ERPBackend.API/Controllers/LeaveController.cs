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

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LeaveController(ApplicationDbContext context)
        {
            _context = context;
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
                EmployeeId = l.EmployeeId,
                EmployeeIdCard = l.Employee?.EmployeeId ?? "",
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
                EmployeeId = l.EmployeeId,
                EmployeeIdCard = l.Employee?.EmployeeId ?? "",
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
                EmployeeId = dto.EmployeeId,
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
        public async Task<IActionResult> ExportApplicationPdf(int id)
        {
            try
            {
                var app = await _context.LeaveApplications
                    .Include(l => l.Employee).ThenInclude(e => e!.Designation)
                    .Include(l => l.Employee).ThenInclude(e => e!.Department)
                    .Include(l => l.Employee).ThenInclude(e => e!.Section)
                    .Include(l => l.LeaveType)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (app == null) return NotFound("Leave Application not found");

                Company? company = app.Employee?.Department?.Company;
                if (company == null && !string.IsNullOrEmpty(app.Employee?.CompanyName))
                {
                    company = await _context.Companies.FirstOrDefaultAsync(c =>
                        c.CompanyNameEn == app.Employee.CompanyName);
                }

                company ??= await _context.Companies.FirstOrDefaultAsync();
                var balances = await GetLeaveBalancesInternal(app.EmployeeId);

                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

                var document = QuestPDF.Fluent.Document.Create(container =>
                {
                    // Styles
                    static IContainer LabelStyle(IContainer container) =>
                        container.Padding(2).DefaultTextStyle(x => x.SemiBold());

                    static IContainer ValueStyle(IContainer container) => container.BorderBottom(0.5f)
                        .BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(2);

                    // AlignCenter on Container
                    static IContainer HeaderStyle(IContainer container) => container.Border(1)
                        .BorderColor(QuestPDF.Helpers.Colors.Black).Background(QuestPDF.Helpers.Colors.Grey.Lighten3)
                        .Padding(2).AlignCenter();

                    void DrawPage(QuestPDF.Fluent.PageDescriptor page, bool isBangla)
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(QuestPDF.Helpers.Colors.White);

                        page.Header().Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().AlignCenter().Text(company?.CompanyNameEn ?? "HR HUB COMPOSITE LTD.")
                                        .Style(TextStyle.Default.Bold().FontSize(20)
                                            .FontColor(QuestPDF.Helpers.Colors.Blue.Darken2));
                                    c.Item().AlignCenter()
                                        .Text(company?.AddressEn ?? "123, Garments Avenue, Dhaka, Bangladesh")
                                        .FontSize(10);
                                });
                            });
                            col.Item().PaddingVertical(10).AlignCenter()
                                .Text(isBangla ? "ছুটির আবেদন ফর্ম" : "LEAVE APPLICATION FORM").Underline().Bold()
                                .FontSize(16);
                        });

                        page.Content().PaddingVertical(10).Column(col =>
                        {
                            // Date
                            col.Item().AlignRight().Text($"Date: {DateTime.Now:dd MMM yyyy}");

                            // Employee Info Table
                            col.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(100);
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(100);
                                    columns.RelativeColumn();
                                });

                                table.Cell().Element(LabelStyle).Text(isBangla ? "নাম:" : "Name:");
                                table.Cell().Element(ValueStyle).Text(app.Employee?.FullNameEn ?? "");
                                table.Cell().Element(LabelStyle).Text(isBangla ? "আইডি:" : "ID:");
                                table.Cell().Element(ValueStyle).Text(app.Employee?.EmployeeId ?? "");

                                table.Cell().Element(LabelStyle).Text(isBangla ? "পদবী:" : "Designation:");
                                table.Cell().Element(ValueStyle).Text(app.Employee?.Designation?.NameEn ?? "");
                                table.Cell().Element(LabelStyle).Text(isBangla ? "বিভাগ:" : "Department:");
                                table.Cell().Element(ValueStyle).Text(app.Employee?.Department?.NameEn ?? "");

                                table.Cell().Element(LabelStyle).Text(isBangla ? "সেকশন:" : "Section:");
                                table.Cell().Element(ValueStyle).Text(app.Employee?.Section?.NameEn ?? "");
                                table.Cell().Element(LabelStyle).Text(isBangla ? "যোগদান:" : "Join Date:");
                                table.Cell().Element(ValueStyle)
                                    .Text(app.Employee?.JoinDate.ToString("dd MMM yyyy") ?? "");
                            });

                            // Leave Details
                            col.Item().PaddingTop(20).Text(isBangla ? "ছুটির বিবরণ:" : "Leave Details:").Bold()
                                .FontSize(12);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderStyle).Text(isBangla ? "ছুটির ধরন" : "Leave Type");
                                    header.Cell().Element(HeaderStyle).Text(isBangla ? "শুরু" : "From Date");
                                    header.Cell().Element(HeaderStyle).Text(isBangla ? "শেষ" : "To Date");
                                    header.Cell().Element(HeaderStyle).Text(isBangla ? "মোট দিন" : "Total Days");
                                });

                                table.Cell().Element(ValueStyle).Text(app.LeaveType?.Name ?? "");
                                table.Cell().Element(ValueStyle).Text(app.StartDate.ToString("dd MMM yyyy"));
                                table.Cell().Element(ValueStyle).Text(app.EndDate.ToString("dd MMM yyyy"));
                                table.Cell().Element(ValueStyle).Text(app.TotalDays.ToString());
                            });

                            col.Item().PaddingTop(5).Text($"Reason: {app.Reason}").Italic();

                            // Leave Balance Summary
                            col.Item().PaddingTop(20).Text(isBangla ? "ছুটির স্থিতি:" : "Leave Balance Summary:").Bold()
                                .FontSize(12);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderStyle).Text(isBangla ? "ধরন" : "Leave Type");
                                    header.Cell().Element(HeaderStyle).Text(isBangla ? "বরাদ্দ" : "Allocated");
                                    header.Cell().Element(HeaderStyle).Text(isBangla ? "ভোগকৃত" : "Enjoyed");
                                    header.Cell().Element(HeaderStyle).Text(isBangla ? "অবশিষ্ট" : "Balance");
                                });

                                foreach (var balance in balances)
                                {
                                    table.Cell().Element(ValueStyle).Text(balance.LeaveTypeName);
                                    table.Cell().Element(ValueStyle).Text(balance.TotalAllocated.ToString());
                                    table.Cell().Element(ValueStyle).Text(balance.TotalTaken.ToString());
                                    table.Cell().Element(ValueStyle).Text(balance.Balance.ToString());
                                }
                            });

                            // Signatures
                            col.Item().PaddingTop(50).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().BorderTop(1).AlignCenter()
                                        .Text(isBangla ? "আবেদনকারীর স্বাক্ষর" : "Applicant Signature");
                                });
                                row.ConstantItem(20);
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().BorderTop(1).AlignCenter()
                                        .Text(isBangla ? "সুপারিশকারী" : "Recommended By");
                                });
                                row.ConstantItem(20);
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().BorderTop(1).AlignCenter()
                                        .Text(isBangla ? "অনুমোদনকারী" : "Approved By");
                                });
                            });
                        });
                    }

                    // Page 1: English
                    container.Page(page => DrawPage(page, false));

                    // Page 2: Bangla
                    container.Page(page => DrawPage(page, true));
                });

                using var stream = new MemoryStream();
                document.GeneratePdf(stream);
                return File(stream.ToArray(), "application/pdf",
                    $"Leave_Application_{app.Employee?.EmployeeId}_{id}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        private async Task<List<LeaveBalanceDto>> GetLeaveBalancesInternal(int employeeId)
        {
            var types = await _context.LeaveTypes.Where(t => t.IsActive).ToListAsync();
            var applications = await _context.LeaveApplications
                .Where(l => l.EmployeeId == employeeId && l.Status == "Approved" &&
                            l.StartDate.Year == DateTime.Now.Year)
                .ToListAsync();

            return types.Select(t => new LeaveBalanceDto
            {
                LeaveTypeId = t.Id,
                LeaveTypeName = t.Name,
                TotalAllocated = t.YearlyLimit,
                TotalTaken = applications.Where(a => a.LeaveTypeId == t.Id).Sum(a => a.TotalDays),
                Balance = t.YearlyLimit - applications.Where(a => a.LeaveTypeId == t.Id).Sum(a => a.TotalDays)
            }).ToList();
        }

        [HttpGet("export/word/{id}")]
        public async Task<IActionResult> ExportApplicationWord(int id)
        {
            try
            {
                var app = await _context.LeaveApplications
                    .Include(l => l.Employee).ThenInclude(e => e!.Designation)
                    .Include(l => l.Employee).ThenInclude(e => e!.Department)
                    .Include(l => l.Employee).ThenInclude(e => e!.Section)
                    .Include(l => l.LeaveType)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (app == null) return NotFound("Leave Application not found");

                Company? company = app.Employee?.Department?.Company;
                if (company == null && !string.IsNullOrEmpty(app.Employee?.CompanyName))
                {
                    company = await _context.Companies.FirstOrDefaultAsync(c =>
                        c.CompanyNameEn == app.Employee.CompanyName);
                }

                company ??= await _context.Companies.FirstOrDefaultAsync();
                var balances = await GetLeaveBalancesInternal(app.EmployeeId);

                var sb = new System.Text.StringBuilder();

                // Helper to generate a page
                void GenerateHtmlPage(bool isBangla)
                {
                    sb.Append("<div style='page-break-after: always; padding: 20px;'>");

                    // Header
                    sb.Append($"<div style='text-align:center; margin-bottom: 20px;'>");
                    sb.Append(
                        $"<h2 style='margin:0; color:#1a365d'>{(company?.CompanyNameEn ?? "HR HUB COMPOSITE LTD.")}</h2>");
                    sb.Append(
                        $"<p style='margin:0; font-size:12px'>{(company?.AddressEn ?? "123, Garments Avenue, Dhaka, Bangladesh")}</p>");
                    sb.Append(
                        $"<h3 style='text-decoration: underline; margin-top:10px'>{(isBangla ? "ছুটির আবেদন ফর্ম" : "LEAVE APPLICATION FORM")}</h3>");
                    sb.Append("</div>");

                    // Date
                    sb.Append($"<p style='text-align:right'>Date: {DateTime.Now:dd MMM yyyy}</p>");

                    // Employee Info
                    sb.Append("<table style='width:100%; border-collapse: collapse; margin-bottom: 20px;'>");

                    string Row(string l1, string v1, string l2, string v2) =>
                        $"<tr><td style='width:15%; font-weight:bold; padding:5px'>{l1}</td><td style='width:35%; border-bottom:1px solid #ddd; padding:5px'>{v1}</td><td style='width:15%; font-weight:bold; padding:5px'>{l2}</td><td style='width:35%; border-bottom:1px solid #ddd; padding:5px'>{v2}</td></tr>";

                    sb.Append(Row(isBangla ? "নাম:" : "Name:", app.Employee?.FullNameEn ?? "",
                        isBangla ? "আইডি:" : "ID:", app.Employee?.EmployeeId ?? ""));
                    sb.Append(Row(isBangla ? "পদবী:" : "Designation:", app.Employee?.Designation?.NameEn ?? "",
                        isBangla ? "বিভাগ:" : "Department:", app.Employee?.Department?.NameEn ?? ""));
                    sb.Append(Row(isBangla ? "সেকশন:" : "Section:", app.Employee?.Section?.NameEn ?? "",
                        isBangla ? "যোগদান:" : "Join Date:", app.Employee?.JoinDate.ToString("dd MMM yyyy") ?? ""));
                    sb.Append("</table>");

                    // Leave Details
                    sb.Append($"<h4>{(isBangla ? "ছুটির বিবরণ:" : "Leave Details:")}</h4>");
                    sb.Append(
                        "<table border='1' style='width:100%; border-collapse:collapse; margin-bottom:20px; text-align:center'>");
                    sb.Append(
                        $"<tr style='background-color:#f0f0f0'><th>{(isBangla ? "ধরন" : "Type")}</th><th>{(isBangla ? "শুরু" : "From")}</th><th>{(isBangla ? "শেষ" : "To")}</th><th>{(isBangla ? "দিন" : "Days")}</th></tr>");
                    sb.Append(
                        $"<tr><td>{app.LeaveType?.Name}</td><td>{app.StartDate:dd MMM yyyy}</td><td>{app.EndDate:dd MMM yyyy}</td><td>{app.TotalDays}</td></tr>");
                    sb.Append("</table>");
                    sb.Append($"<p><b>Reason:</b> {app.Reason}</p>");

                    // Balance
                    sb.Append($"<h4>{(isBangla ? "ছুটির স্থিতি:" : "Leave Balance Summary:")}</h4>");
                    sb.Append(
                        "<table border='1' style='width:100%; border-collapse:collapse; margin-bottom:40px; text-align:center'>");
                    sb.Append(
                        $"<tr style='background-color:#f0f0f0'><th>{(isBangla ? "ধরন" : "Leave Type")}</th><th>{(isBangla ? "বরাদ্দ" : "Allocated")}</th><th>{(isBangla ? "ভোগকৃত" : "Enjoyed")}</th><th>{(isBangla ? "অবশিষ্ট" : "Balance")}</th></tr>");
                    foreach (var b in balances)
                    {
                        sb.Append(
                            $"<tr><td>{b.LeaveTypeName}</td><td>{b.TotalAllocated}</td><td>{b.TotalTaken}</td><td>{b.Balance}</td></tr>");
                    }

                    sb.Append("</table>");

                    // Signatures
                    sb.Append("<table style='width:100%; margin-top:50px; text-align:center'>");
                    sb.Append("<tr>");
                    sb.Append(
                        $"<td style='border-top:1px solid #000; width:30%'>{(isBangla ? "আবেদনকারী" : "Applicant")}</td>");
                    sb.Append("<td style='width:5%'></td>");
                    sb.Append(
                        $"<td style='border-top:1px solid #000; width:30%'>{(isBangla ? "সুপারিশকারী" : "Recommended By")}</td>");
                    sb.Append("<td style='width:5%'></td>");
                    sb.Append(
                        $"<td style='border-top:1px solid #000; width:30%'>{(isBangla ? "অনুমোদনকারী" : "Approved By")}</td>");
                    sb.Append("</tr>");
                    sb.Append("</table>");

                    sb.Append("</div>");
                }

                sb.Append("<html><body>");
                GenerateHtmlPage(false); // English
                GenerateHtmlPage(true); // Bangla
                sb.Append("</body></html>");

                var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                return File(bytes, "application/msword", $"Leave_Application_{app.Employee?.EmployeeId}_{id}.doc");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExportWord Exception: {ex}");
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
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
            if (await _context.LeaveTypes.AnyAsync()) return Ok(new { message = "Leave types already seeded." });

            var types = new List<LeaveType>
            {
                new() { Name = "Sick Leave", Code = "SL", YearlyLimit = 14, IsCarryForward = false },
                new() { Name = "Casual Leave", Code = "CL", YearlyLimit = 10, IsCarryForward = false },
                new() { Name = "Earned Leave", Code = "EL", YearlyLimit = 20, IsCarryForward = true },
                new() { Name = "Maternity Leave", Code = "ML", YearlyLimit = 112, IsCarryForward = false },
            };

            _context.LeaveTypes.AddRange(types);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Leave types seeded successfully." });
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
                    EmployeeId = g.Key.EmployeeId,
                    EmployeeIdCard = g.Key.EmployeeIdCard,
                    EmployeeName = g.Key.FullNameEn,
                    Department = g.Key.Dept,
                    SickLeave = g.Where(x => x.LeaveType!.Code == "SL").Sum(x => x.TotalDays),
                    CasualLeave = g.Where(x => x.LeaveType!.Code == "CL").Sum(x => x.TotalDays),
                    EarnedLeave = g.Where(x => x.LeaveType!.Code == "EL").Sum(x => x.TotalDays),
                    OtherLeave =
                        g.Where(x =>
                                x.LeaveType!.Code != "SL" && x.LeaveType!.Code != "CL" && x.LeaveType!.Code != "EL")
                            .Sum(x => x.TotalDays),
                    TotalDays = g.Sum(x => x.TotalDays)
                })
                .ToList();

            return Ok(report);
        }
    }
}
