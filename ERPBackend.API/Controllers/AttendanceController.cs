using ERPBackend.Core.Constants;
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
using System.Drawing;
using System.Security.Claims;
using System.Linq;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/attendance/daily-report
        [HttpGet("daily-report")]
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetDailyReport(
            [FromQuery] DateTime date,
            [FromQuery] int? departmentId,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm)
        {
            try
            {
                var query = _context.Attendances
                    .Include(a => a.Employee)
                        .ThenInclude(e => e!.Department)
                    .Include(a => a.Employee)
                        .ThenInclude(e => e!.Designation)
                    .Include(a => a.Employee)
                        .ThenInclude(e => e!.Shift)
                    .Where(a => a.Date.Date == date.Date)
                    .AsQueryable();

                if (departmentId.HasValue)
                    query = query.Where(a => a.Employee!.DepartmentId == departmentId.Value);

                if (!string.IsNullOrEmpty(status) && status != "all")
                    query = query.Where(a => a.Status == status);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(a => a.Employee!.FullNameEn.Contains(searchTerm) || 
                                           a.Employee!.EmployeeId.Contains(searchTerm));
                }

                var result = await query.Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    EmployeeId = a.EmployeeId,
                    EmployeeIdCard = a.Employee!.EmployeeId,
                    EmployeeName = a.Employee!.FullNameEn,
                    Department = a.Employee!.Department != null ? a.Employee.Department.NameEn : "N/A",
                    Designation = a.Employee!.Designation != null ? a.Employee.Designation.NameEn : "N/A",
                    Shift = a.Employee!.Shift != null ? a.Employee.Shift.NameEn : "N/A",
                    Date = a.Date,
                    InTime = a.InTime,
                    OutTime = a.OutTime,
                    Status = a.Status,
                    OTHours = a.OTHours
                }).ToListAsync();
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the daily report.", error = ex.Message });
            }
        }

        // GET: api/attendance/daily-report/export/excel
        [HttpGet("daily-report/export/excel")]
        public async Task<IActionResult> ExportDailyReportToExcel(
            [FromQuery] DateTime date,
            [FromQuery] int? departmentId,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm)
        {
            try
            {
                var data = await GetDailyReportInternal(date, departmentId, status, searchTerm);
                var company = await _context.Set<Company>().FirstOrDefaultAsync();
                
                using (var package = new ExcelPackage())
                {
                    // Create Section Wise Sheet
                    var sectionSheet = package.Workbook.Worksheets.Add("Section Wise");
                    CreateAttendanceWorksheet(sectionSheet, data, company, date, "Section");

                    // Create Line Wise Sheet
                    var lineSheet = package.Workbook.Worksheets.Add("Line Wise");
                    CreateAttendanceWorksheet(lineSheet, data, company, date, "Line");

                    var content = package.GetAsByteArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DailyAttendanceReport_{date:yyyyMMdd}.xlsx");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while exporting to Excel.", error = ex.Message });
            }
        }

        private void CreateAttendanceWorksheet(ExcelWorksheet worksheet, IEnumerable<AttendanceDto> data, Company? company, DateTime date, string groupBy)
        {
            // 0. Page Setup
            worksheet.PrinterSettings.Orientation = OfficeOpenXml.eOrientation.Landscape;
            worksheet.PrinterSettings.FitToPage = true;
            worksheet.PrinterSettings.FitToWidth = 1;
            worksheet.PrinterSettings.FitToHeight = 0;
            worksheet.PrinterSettings.PaperSize = OfficeOpenXml.ePaperSize.A4;
            worksheet.PrinterSettings.TopMargin = 0.5;
            worksheet.PrinterSettings.BottomMargin = 0.5;
            worksheet.PrinterSettings.LeftMargin = 0.5;
            worksheet.PrinterSettings.RightMargin = 0.5;

            // 1. Header Section
            string companyName = company?.CompanyNameEn ?? "HR HUB";
            string address = company?.Address ?? "Industrial Area, Dhaka, Bangladesh";
            
            worksheet.Cells["A1:J1"].Merge = true;
            worksheet.Cells["A1"].Value = companyName;
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["A2:J2"].Merge = true;
            worksheet.Cells["A2"].Value = address;
            worksheet.Cells["A2"].Style.Font.Size = 10;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["A3:J3"].Merge = true;
            worksheet.Cells["A3"].Value = $"Daily Attendance Report ({groupBy} Wise)";
            worksheet.Cells["A3"].Style.Font.Size = 12;
            worksheet.Cells["A3"].Style.Font.Bold = true;
            worksheet.Cells["A3"].Style.Font.UnderLine = true;
            worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["A4:J4"].Merge = true;
            worksheet.Cells["A4"].Value = $"Date: {date:dd MMMM yyyy}";
            worksheet.Cells["A4"].Style.Font.Size = 10;
            worksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // 2. Table Headers
            int headerRow = 6;
            worksheet.Cells[headerRow, 1].Value = "SL";
            worksheet.Cells[headerRow, 2].Value = "Employee ID";
            worksheet.Cells[headerRow, 3].Value = "Name";
            worksheet.Cells[headerRow, 4].Value = "Department";
            worksheet.Cells[headerRow, 5].Value = "Designation";
            worksheet.Cells[headerRow, 6].Value = "Shift";
            worksheet.Cells[headerRow, 7].Value = "In Time";
            worksheet.Cells[headerRow, 8].Value = "Out Time";
            worksheet.Cells[headerRow, 9].Value = "Status";
            worksheet.Cells[headerRow, 10].Value = "OT Hours";

            using (var range = worksheet.Cells[headerRow, 1, headerRow, 10])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            // 3. Data Rows
            int currentRow = headerRow + 1;
            int sl = 1;

            var groupedData = groupBy == "Section" 
                ? data.OrderBy(x => x.Section).ThenBy(x => x.EmployeeName).GroupBy(x => x.Section)
                : data.OrderBy(x => x.Line).ThenBy(x => x.EmployeeName).GroupBy(x => x.Line);

            foreach (var group in groupedData)
            {
                // Group Header Row
                worksheet.Cells[currentRow, 1, currentRow, 10].Merge = true;
                worksheet.Cells[currentRow, 1].Value = $"{groupBy}: {group.Key}";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(240, 240, 240));
                currentRow++;

                foreach (var item in group)
                {
                    worksheet.Cells[currentRow, 1].Value = sl++;
                    worksheet.Cells[currentRow, 2].Value = item.EmployeeIdCard;
                    worksheet.Cells[currentRow, 3].Value = item.EmployeeName;
                    worksheet.Cells[currentRow, 4].Value = item.Department;
                    worksheet.Cells[currentRow, 5].Value = item.Designation;
                    worksheet.Cells[currentRow, 6].Value = item.Shift;
                    worksheet.Cells[currentRow, 7].Value = item.InTime ?? "-";
                    worksheet.Cells[currentRow, 8].Value = item.OutTime ?? "-";

                    string statusShort = item.Status switch
                    {
                        "Present" => "P",
                        "Absent" => "A",
                        "Late" => "L",
                        "On Leave" => "LV",
                        "Off Day" => "OFF",
                        _ => item.Status
                    };
                    
                    var statusCell = worksheet.Cells[currentRow, 9];
                    statusCell.Value = statusShort;
                    statusCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    statusCell.Style.Font.Bold = true;

                    if (statusShort == "P") statusCell.Style.Font.Color.SetColor(System.Drawing.Color.Green);
                    else if (statusShort == "A") statusCell.Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    else if (statusShort == "L") statusCell.Style.Font.Color.SetColor(System.Drawing.Color.Orange);
                    else if (statusShort == "LV") statusCell.Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                    else if (statusShort == "OFF") statusCell.Style.Font.Color.SetColor(System.Drawing.Color.Gray);

                    worksheet.Cells[currentRow, 10].Value = item.OTHours;

                    for (int i = 1; i <= 10; i++)
                    {
                        worksheet.Cells[currentRow, i].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[currentRow, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[currentRow, i].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[currentRow, i].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }
                    currentRow++;
                }
            }

            // 4. Summary Footer Section
            currentRow += 2;
            int summaryStartRow = currentRow;
            
            worksheet.Cells[currentRow, 2].Value = $"Summary Statistics ({groupBy} Wise)";
            worksheet.Cells[currentRow, 2].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 2].Style.Font.UnderLine = true;
            currentRow++;

            worksheet.Cells[currentRow, 2].Value = "Total Employee";
            worksheet.Cells[currentRow, 3].Value = data.Count();
            currentRow++;

            worksheet.Cells[currentRow, 2].Value = "Present (P)";
            worksheet.Cells[currentRow, 3].Value = data.Count(x => x.Status == "Present" || x.Status == "Late");
            worksheet.Cells[currentRow, 3].Style.Font.Color.SetColor(System.Drawing.Color.Green);
            currentRow++;

            worksheet.Cells[currentRow, 2].Value = "Late (L)";
            worksheet.Cells[currentRow, 3].Value = data.Count(x => x.Status == "Late");
            worksheet.Cells[currentRow, 3].Style.Font.Color.SetColor(System.Drawing.Color.Orange);
            currentRow++;

            worksheet.Cells[currentRow, 2].Value = "Absent (A)";
            worksheet.Cells[currentRow, 3].Value = data.Count(x => x.Status == "Absent");
            worksheet.Cells[currentRow, 3].Style.Font.Color.SetColor(System.Drawing.Color.Red);
            currentRow++;

            worksheet.Cells[currentRow, 2].Value = "On Leave (LV)";
            worksheet.Cells[currentRow, 3].Value = data.Count(x => x.Status == "On Leave");
            worksheet.Cells[currentRow, 3].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
            currentRow++;

            using (var range = worksheet.Cells[summaryStartRow + 1, 3, currentRow - 1, 3])
            {
                range.Style.Font.Bold = true;
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            }

            worksheet.Cells.AutoFitColumns();
            worksheet.Column(1).Width = 5;
            worksheet.Column(2).Width = 15;
            worksheet.Column(3).Width = 25;
            worksheet.Column(9).Width = 8;
        }

        // GET: api/attendance/daily-report/export/pdf
        [HttpGet("daily-report/export/pdf")]
        public async Task<IActionResult> ExportDailyReportToPdf(
            [FromQuery] DateTime date,
            [FromQuery] int? departmentId,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm)
        {
            try
            {
                var data = await GetDailyReportInternal(date, departmentId, status, searchTerm);

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9));

                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Daily Attendance Report").FontSize(16).SemiBold().FontColor(Colors.Blue.Medium);
                                col.Item().Text($"Date: {date:dd MMMM yyyy}").FontSize(10);
                            });
                        });

                        page.Content().PaddingVertical(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25); // SL
                                columns.ConstantColumn(80); // Emp ID
                                columns.RelativeColumn();   // Name
                                columns.RelativeColumn();   // Dept
                                columns.RelativeColumn();   // Desig
                                columns.ConstantColumn(50); // In
                                columns.ConstantColumn(50); // Out
                                columns.ConstantColumn(60); // Status
                                columns.ConstantColumn(40); // OT
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("SL");
                                header.Cell().Element(CellStyle).Text("Emp ID");
                                header.Cell().Element(CellStyle).Text("Name");
                                header.Cell().Element(CellStyle).Text("Dept");
                                header.Cell().Element(CellStyle).Text("Desig");
                                header.Cell().Element(CellStyle).Text("In");
                                header.Cell().Element(CellStyle).Text("Out");
                                header.Cell().Element(CellStyle).Text("Status");
                                header.Cell().Element(CellStyle).Text("OT");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold())
                                                    .PaddingVertical(5)
                                                    .BorderBottom(1)
                                                    .BorderColor(Colors.Black);
                                }
                            });

                            int slCount = 1;
                            foreach (var item in data)
                            {
                                table.Cell().Element(CellStyle).Text(slCount++.ToString());
                                table.Cell().Element(CellStyle).Text(item.EmployeeIdCard);
                                table.Cell().Element(CellStyle).Text(item.EmployeeName);
                                table.Cell().Element(CellStyle).Text(item.Department);
                                table.Cell().Element(CellStyle).Text(item.Designation);
                                table.Cell().Element(CellStyle).Text(item.InTime ?? "-");
                                table.Cell().Element(CellStyle).Text(item.OutTime ?? "-");
                                table.Cell().Element(CellStyle).Text(item.Status);
                                table.Cell().Element(CellStyle).Text(item.OTHours.ToString());

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3);
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
                return File(stream, "application/pdf", $"DailyAttendanceReport_{date:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while exporting to PDF.", error = ex.Message });
            }
        }

        private async Task<IEnumerable<AttendanceDto>> GetDailyReportInternal(
            DateTime date,
            int? departmentId,
            string? status,
            string? searchTerm)
        {
            var query = _context.Attendances
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Department)
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Section)
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Line)
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Designation)
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Shift)
                .Where(a => a.Date.Date == date.Date)
                .AsQueryable();

            if (departmentId.HasValue)
                query = query.Where(a => a.Employee!.DepartmentId == departmentId.Value);

            if (!string.IsNullOrEmpty(status) && status != "all")
                query = query.Where(a => a.Status == status);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(a => a.Employee!.FullNameEn.Contains(searchTerm) ||
                                       a.Employee!.EmployeeId.Contains(searchTerm));
            }

            return await query.Select(a => new AttendanceDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeIdCard = a.Employee!.EmployeeId,
                EmployeeName = a.Employee!.FullNameEn,
                Department = a.Employee!.Department != null ? a.Employee.Department.NameEn : "N/A",
                Section = a.Employee!.Section != null ? a.Employee.Section.NameEn : "N/A",
                Line = a.Employee!.Line != null ? a.Employee.Line.NameEn : "N/A",
                Designation = a.Employee!.Designation != null ? a.Employee.Designation.NameEn : "N/A",
                Shift = a.Employee!.Shift != null ? a.Employee.Shift.NameEn : "N/A",
                Date = a.Date,
                InTime = a.InTime,
                OutTime = a.OutTime,
                Status = a.Status,
                OTHours = a.OTHours
            }).ToListAsync();
        }

        // GET: api/attendance/summary
        [HttpGet("summary")]
        public async Task<ActionResult<AttendanceSummaryDto>> GetSummary([FromQuery] DateTime date)
        {
            try
            {
                var totalHeadcount = await _context.Employees.CountAsync(e => e.IsActive);
                var attendances = await _context.Attendances
                    .Where(a => a.Date.Date == date.Date)
                    .ToListAsync();

                var present = attendances.Count(a => a.Status == "Present" || a.Status == "Late");
                var late = attendances.Count(a => a.Status == "Late");
                var absent = attendances.Count(a => a.Status == "Absent");
                var leave = attendances.Count(a => a.Status == "On Leave");

                return Ok(new AttendanceSummaryDto
                {
                    TotalHeadcount = totalHeadcount,
                    PresentCount = present,
                    AbsentCount = absent,
                    LateCount = late,
                    LeaveCount = leave,
                    AttendanceRate = totalHeadcount > 0 ? Math.Round((double)present / totalHeadcount * 100, 2) : 0
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the attendance summary.", error = ex.Message });
            }
        }

        // GET: api/attendance/daily-summary
        [HttpGet("daily-summary")]
        public async Task<ActionResult<DailySummaryResponseDto>> GetDailySummary(
            [FromQuery] DateTime date,
            [FromQuery] int? departmentId)
        {
            try
            {
                var summary = await GetDailySummaryInternal(date, departmentId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the daily summary.", error = ex.Message });
            }
        }

        private async Task<DailySummaryResponseDto> GetDailySummaryInternal(DateTime date, int? departmentId)
        {
            // 1. Fetch all active employees with their details to calculate total headcount per group
            var allEmployees = await _context.Employees
                .Where(e => e.IsActive)
                .Include(e => e.Department)
                .Include(e => e.Section)
                .Include(e => e.Designation)
                .Include(e => e.Line)
                .Include(e => e.Group)
                .ToListAsync();

            // 2. Fetch all attendance for the date
            var attendances = await _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.Date.Date == date.Date)
                .ToListAsync();

            if (departmentId.HasValue)
            {
                allEmployees = allEmployees.Where(e => e.DepartmentId == departmentId.Value).ToList();
                attendances = attendances.Where(a => a.Employee?.DepartmentId == departmentId.Value).ToList();
            }

            // Filter attendances to only include active employees to ensure rate consistency
            var activeEmpIds = allEmployees.Select(e => e.Id).ToHashSet();
            attendances = attendances.Where(a => activeEmpIds.Contains(a.EmployeeId)).ToList();

            // 3. Overall Summary
            var present = attendances.Count(a => a.Status == "Present" || a.Status == "Late");
            var late = attendances.Count(a => a.Status == "Late");
            var absent = attendances.Count(a => a.Status == "Absent");
            var leave = attendances.Count(a => a.Status == "On Leave");
            var totalHeadcount = allEmployees.Count;

            var overallSummary = new AttendanceSummaryDto
            {
                TotalHeadcount = totalHeadcount,
                PresentCount = present,
                AbsentCount = absent,
                LateCount = late,
                LeaveCount = leave,
                AttendanceRate = totalHeadcount > 0 ? Math.Round((double)present / totalHeadcount * 100, 2) : 0
            };

            // 4. Department Summary
            var departments = allEmployees.Where(e => e.Department != null)
                .Select(e => e.Department).DistinctBy(d => d!.Id).ToList();

            var departmentSummaries = departments.Select(dept =>
            {
                var empIds = allEmployees.Where(e => e.DepartmentId == dept!.Id).Select(e => e.Id).ToHashSet();
                var deptAttendances = attendances.Where(a => empIds.Contains(a.EmployeeId)).ToList();
                
                var p = deptAttendances.Count(a => a.Status == "Present" || a.Status == "Late");
                var l = deptAttendances.Count(a => a.Status == "Late");
                var ab = deptAttendances.Count(a => a.Status == "Absent");
                var lv = deptAttendances.Count(a => a.Status == "On Leave");
                var total = empIds.Count;

                return new DepartmentDailySummaryDto
                {
                    Id = dept!.Id,
                    DepartmentId = dept.Id,
                    DepartmentName = dept.NameEn,
                    TotalEmployees = total,
                    Present = p,
                    Absent = ab,
                    Late = l,
                    OnLeave = lv,
                    AttendanceRate = total > 0 ? Math.Round((double)p / total * 100, 2) : 0
                };
            }).OrderByDescending(x => x.TotalEmployees).ToList();

            // 5. Section Summary
            var sections = allEmployees.Where(e => e.Section != null)
                .Select(e => e.Section).DistinctBy(s => s!.Id).ToList();

            var sectionSummaries = sections.Select(sec =>
            {
                var empIds = allEmployees.Where(e => e.SectionId == sec!.Id).Select(e => e.Id).ToHashSet();
                var secAttendances = attendances.Where(a => empIds.Contains(a.EmployeeId)).ToList();

                var p = secAttendances.Count(a => a.Status == "Present" || a.Status == "Late");
                var l = secAttendances.Count(a => a.Status == "Late");
                var ab = secAttendances.Count(a => a.Status == "Absent");
                var lv = secAttendances.Count(a => a.Status == "On Leave");
                var total = empIds.Count;

                return new SectionDailySummaryDto
                {
                    Id = sec!.Id,
                    SectionId = sec.Id,
                    SectionName = sec.NameEn,
                    TotalEmployees = total,
                    Present = p,
                    Absent = ab,
                    Late = l,
                    OnLeave = lv,
                    AttendanceRate = total > 0 ? Math.Round((double)p / total * 100, 2) : 0
                };
            }).OrderByDescending(x => x.TotalEmployees).ToList();

            // 6. Designation Summary
            var designations = allEmployees.Where(e => e.Designation != null)
                .Select(e => e.Designation).DistinctBy(d => d!.Id).ToList();

            var designationSummaries = designations.Select(desig =>
            {
                var empIds = allEmployees.Where(e => e.DesignationId == desig!.Id).Select(e => e.Id).ToHashSet();
                var desigAttendances = attendances.Where(a => empIds.Contains(a.EmployeeId)).ToList();

                var p = desigAttendances.Count(a => a.Status == "Present" || a.Status == "Late");
                var l = desigAttendances.Count(a => a.Status == "Late");
                var ab = desigAttendances.Count(a => a.Status == "Absent");
                var lv = desigAttendances.Count(a => a.Status == "On Leave");
                var total = empIds.Count;

                return new DesignationDailySummaryDto
                {
                    Id = desig!.Id,
                    DesignationId = desig.Id,
                    DesignationName = desig.NameEn,
                    TotalEmployees = total,
                    Present = p,
                    Absent = ab,
                    Late = l,
                    OnLeave = lv,
                    AttendanceRate = total > 0 ? Math.Round((double)p / total * 100, 2) : 0
                };
            }).OrderByDescending(x => x.TotalEmployees).ToList();

            // 7. Line Summary
            var lines = allEmployees.Where(e => e.Line != null)
                .Select(e => e.Line).DistinctBy(l => l!.Id).ToList();

            var lineSummaries = lines.Select(line =>
            {
                var empIds = allEmployees.Where(e => e.LineId == line!.Id).Select(e => e.Id).ToHashSet();
                var lineAttendances = attendances.Where(a => empIds.Contains(a.EmployeeId)).ToList();

                var p = lineAttendances.Count(a => a.Status == "Present" || a.Status == "Late");
                var l = lineAttendances.Count(a => a.Status == "Late");
                var ab = lineAttendances.Count(a => a.Status == "Absent");
                var lv = lineAttendances.Count(a => a.Status == "On Leave");
                var total = empIds.Count;

                return new LineDailySummaryDto
                {
                    Id = line!.Id,
                    LineId = line.Id,
                    LineName = line.NameEn,
                    TotalEmployees = total,
                    Present = p,
                    Absent = ab,
                    Late = l,
                    OnLeave = lv,
                    AttendanceRate = total > 0 ? Math.Round((double)p / total * 100, 2) : 0
                };
            }).OrderByDescending(x => x.TotalEmployees).ToList();

            // 8. Group Summary (Worker / Staff)
            var groups = allEmployees
                .Where(e => e.Group != null)
                .Select(e => e.Group!)
                .GroupBy(g => g.Id)
                .Select(g => g.First())
                .ToList();

            var groupSummaries = groups.Select(grp =>
            {
                var empIds = allEmployees.Where(e => e.GroupId == grp.Id).Select(e => e.Id).ToHashSet();
                var grpAttendances = attendances.Where(a => empIds.Contains(a.EmployeeId)).ToList();

                var p = grpAttendances.Count(a => a.Status == "Present" || a.Status == "Late");
                var l = grpAttendances.Count(a => a.Status == "Late");
                var ab = grpAttendances.Count(a => a.Status == "Absent");
                var lv = grpAttendances.Count(a => a.Status == "On Leave");
                var total = empIds.Count;

                return new GroupDailySummaryDto
                {
                    Id = grp.Id,
                    GroupId = grp.Id,
                    GroupName = grp.NameEn,
                    TotalEmployees = total,
                    Present = p,
                    Absent = ab,
                    Late = l,
                    OnLeave = lv,
                    AttendanceRate = total > 0 ? Math.Round((double)p / total * 100, 2) : 0
                };
            }).OrderByDescending(x => x.TotalEmployees).ToList();

            // 9. Department + Section Wise Summary
            var deptSectionSummaries = allEmployees
                .Where(e => e.Department != null && e.Section != null)
                .GroupBy(e => new { e.DepartmentId, DepartmentName = e.Department!.NameEn, e.SectionId, SectionName = e.Section!.NameEn })
                .Select(g => 
                {
                    var empIds = g.Select(e => e.Id).ToHashSet();
                    var dsAttendances = attendances.Where(a => empIds.Contains(a.EmployeeId)).ToList();

                    var p = dsAttendances.Count(a => a.Status == "Present" || a.Status == "Late");
                    var l = dsAttendances.Count(a => a.Status == "Late");
                    var ab = dsAttendances.Count(a => a.Status == "Absent");
                    var lv = dsAttendances.Count(a => a.Status == "On Leave");
                    var total = empIds.Count;

                    return new DeptSectionDailySummaryDto
                    {
                        Id = $"{g.Key.DepartmentId}-{g.Key.SectionId}",
                        DepartmentId = g.Key.DepartmentId,
                        DepartmentName = g.Key.DepartmentName,
                        SectionId = g.Key.SectionId!.Value,
                        SectionName = g.Key.SectionName,
                        TotalEmployees = total,
                        Present = p,
                        Absent = ab,
                        Late = l,
                        OnLeave = lv,
                        AttendanceRate = total > 0 ? Math.Round((double)p / total * 100, 2) : 0
                    };
                })
                .OrderBy(x => x.DepartmentName).ThenBy(x => x.SectionName)
                .ToList();

            return new DailySummaryResponseDto
            {
                OverallSummary = overallSummary,
                DepartmentSummaries = departmentSummaries,
                SectionSummaries = sectionSummaries,
                DeptSectionSummaries = deptSectionSummaries,
                DesignationSummaries = designationSummaries,
                LineSummaries = lineSummaries,
                GroupSummaries = groupSummaries
            };
        }

        // GET: api/attendance/daily-summary/export/excel
        [HttpGet("daily-summary/export/excel")]
        public async Task<IActionResult> ExportDailySummaryToExcel(
            [FromQuery] DateTime date,
            [FromQuery] int? departmentId)
        {
            try
            {
                var summaryData = await GetDailySummaryInternal(date, departmentId);
                var company = await _context.Set<Company>().FirstOrDefaultAsync();
                
                using (var package = new ExcelPackage())
                {
                    // 1. Overall Summary Sheet
                    var summarySheet = package.Workbook.Worksheets.Add("Overall Summary");
                    AddSummaryHeader(summarySheet, company, "Overall Attendance Summary", date, 6);
                    
                    int row = 7;
                    string[] summaryHeaders = { "Metric", "Count" };
                    for (int i = 0; i < summaryHeaders.Length; i++)
                    {
                        summarySheet.Cells[row, i + 1].Value = summaryHeaders[i];
                        summarySheet.Cells[row, i + 1].Style.Font.Bold = true;
                        summarySheet.Cells[row, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        summarySheet.Cells[row, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }
                    row++;

                    AddSummaryRow(summarySheet, ref row, "Total Headcount", summaryData.OverallSummary.TotalHeadcount);
                    AddSummaryRow(summarySheet, ref row, "Present", summaryData.OverallSummary.PresentCount, System.Drawing.Color.Green);
                    AddSummaryRow(summarySheet, ref row, "Late", summaryData.OverallSummary.LateCount, System.Drawing.Color.Orange);
                    AddSummaryRow(summarySheet, ref row, "Absent", summaryData.OverallSummary.AbsentCount, System.Drawing.Color.Red);
                    AddSummaryRow(summarySheet, ref row, "On Leave", summaryData.OverallSummary.LeaveCount, System.Drawing.Color.Blue);
                    AddSummaryRow(summarySheet, ref row, "Attendance Rate (%)", summaryData.OverallSummary.AttendanceRate);

                    summarySheet.Cells[7, 1, row - 1, 2].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    summarySheet.Cells[7, 1, row - 1, 2].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    summarySheet.Cells[7, 1, row - 1, 2].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    summarySheet.Cells[7, 1, row - 1, 2].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    summarySheet.Column(1).Width = 25;
                    summarySheet.Column(2).Width = 15;

                    // 2. Department Wise Sheet
                    var deptSheet = package.Workbook.Worksheets.Add("Department Wise");
                    AddSummaryTableHeader(deptSheet, company, "Department Wise Summary", date, 8);
                    FillSummaryTable(deptSheet, summaryData.DepartmentSummaries.Select(d => new { Name = d.DepartmentName, d.TotalEmployees, d.Present, d.Absent, d.Late, d.OnLeave, d.AttendanceRate }));

                    // 3. Section Wise Sheet
                    var secSheet = package.Workbook.Worksheets.Add("Section Wise");
                    AddSummaryTableHeader(secSheet, company, "Section Wise Summary", date, 8);
                    FillSummaryTable(secSheet, summaryData.SectionSummaries.Select(s => new { Name = s.SectionName, s.TotalEmployees, s.Present, s.Absent, s.Late, s.OnLeave, s.AttendanceRate }));

                    // 4. Designation Wise Sheet
                    var desigSheet = package.Workbook.Worksheets.Add("Designation Wise");
                    AddSummaryTableHeader(desigSheet, company, "Designation Wise Summary", date, 8);
                    FillSummaryTable(desigSheet, summaryData.DesignationSummaries.Select(d => new { Name = d.DesignationName, d.TotalEmployees, d.Present, d.Absent, d.Late, d.OnLeave, d.AttendanceRate }));

                    // 5. Line Wise Sheet
                    var lineSheet = package.Workbook.Worksheets.Add("Line Wise");
                    AddSummaryTableHeader(lineSheet, company, "Line Wise Summary", date, 8);
                    FillSummaryTable(lineSheet, summaryData.LineSummaries.Select(l => new { Name = l.LineName, l.TotalEmployees, l.Present, l.Absent, l.Late, l.OnLeave, l.AttendanceRate }));

                    // 6. Group Wise Sheet
                    var groupSheet = package.Workbook.Worksheets.Add("Group Wise");
                    AddSummaryTableHeader(groupSheet, company, "Group Wise Summary", date, 8);
                    FillSummaryTable(groupSheet, summaryData.GroupSummaries.Select(g => new { Name = g.GroupName, g.TotalEmployees, g.Present, g.Absent, g.Late, g.OnLeave, g.AttendanceRate }));

                    var content = package.GetAsByteArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DailyAttendanceSummary_{date:yyyyMMdd}.xlsx");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while exporting summary to Excel.", error = ex.Message });
            }
        }

        // GET: api/attendance/daily-summary/export/pdf
        [HttpGet("daily-summary/export/pdf")]
        public async Task<IActionResult> ExportDailySummaryToPdf(
            [FromQuery] DateTime date,
            [FromQuery] int? departmentId)
        {
            try
            {
                var summaryData = await GetDailySummaryInternal(date, departmentId);
                var company = await _context.Set<Company>().FirstOrDefaultAsync();
                string companyName = company?.CompanyNameEn ?? "HR HUB";

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Portrait());
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9));

                        page.Header().Column(col =>
                        {
                            col.Item().Text(companyName).FontSize(16).Bold().FontColor(Colors.Blue.Medium).AlignCenter();
                            col.Item().Text(company?.Address ?? "").FontSize(9).AlignCenter();
                            col.Item().PaddingTop(5).Text("Daily Attendance Summary Report").FontSize(12).Bold().Underline().AlignCenter();
                            col.Item().Text($"Date: {date:dd MMMM yyyy}").FontSize(10).AlignCenter();
                            if (departmentId.HasValue)
                            {
                                var deptName = summaryData.DepartmentSummaries.FirstOrDefault()?.DepartmentName ?? "Selected Department";
                                col.Item().Text($"Filtered by Department: {deptName}").FontSize(9).Italic().AlignCenter();
                            }
                        });

                        page.Content().PaddingTop(10).Column(col =>
                        {
                            // Overall Summary Table
                            col.Item().PaddingBottom(5).Text("1. Overall Summary").FontSize(11).Bold();
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Metric");
                                    header.Cell().Element(CellStyle).Text("Count");
                                });

                                table.Cell().Element(CellContentStyle).Text("Total Headcount");
                                table.Cell().Element(CellContentStyle).Text(summaryData.OverallSummary.TotalHeadcount.ToString());

                                table.Cell().Element(CellContentStyle).Text("Present");
                                table.Cell().Element(CellContentStyle).Text(summaryData.OverallSummary.PresentCount.ToString()).FontColor(Colors.Green.Medium);

                                table.Cell().Element(CellContentStyle).Text("Absent");
                                table.Cell().Element(CellContentStyle).Text(summaryData.OverallSummary.AbsentCount.ToString()).FontColor(Colors.Red.Medium);

                                table.Cell().Element(CellContentStyle).Text("On Leave");
                                table.Cell().Element(CellContentStyle).Text(summaryData.OverallSummary.LeaveCount.ToString()).FontColor(Colors.Blue.Medium);

                                table.Cell().Element(CellContentStyle).Text("Attendance Rate");
                                table.Cell().Element(CellContentStyle).Text($"{summaryData.OverallSummary.AttendanceRate}%").Bold();
                            });

                            // Department Summaries
                            col.Item().Element(c => AddDepartmentSummarySection(c, "2. Department Wise Summary", summaryData.DepartmentSummaries));
                            
                            // Section Summaries
                            col.Item().Element(c => AddSectionSummarySection(c, "3. Section Wise Summary", summaryData.SectionSummaries));
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                    });
                });

                var pdfBytes = document.GeneratePdf();
                return File(pdfBytes, "application/pdf", $"DailySummary_{date:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error exporting Daily Summary to PDF", error = ex.Message });
            }
        }

        private void AddDepartmentSummarySection(IContainer container, string title, IEnumerable<DepartmentDailySummaryDto> data)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(15).PaddingBottom(5).Text(title).FontSize(11).Bold();
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25); // SL
                        columns.RelativeColumn();   // Name
                        columns.ConstantColumn(40); // Total
                        columns.ConstantColumn(40); // Pres
                        columns.ConstantColumn(40); // Abs
                        columns.ConstantColumn(40); // Late
                        columns.ConstantColumn(45); // Rate
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("SL");
                        header.Cell().Element(CellStyle).Text("Name");
                        header.Cell().Element(CellStyle).Text("Tot");
                        header.Cell().Element(CellStyle).Text("Prs");
                        header.Cell().Element(CellStyle).Text("Abs");
                        header.Cell().Element(CellStyle).Text("Lat");
                        header.Cell().Element(CellStyle).Text("Rate%");
                    });

                    int sl = 1;
                    foreach (var item in data.Take(20))
                    {
                        table.Cell().Element(CellContentStyle).AlignCenter().Text(sl++.ToString());
                        table.Cell().Element(CellContentStyle).Text(item.DepartmentName);
                        table.Cell().Element(CellContentStyle).AlignCenter().Text(item.TotalEmployees.ToString());
                        table.Cell().Element(CellContentStyle).AlignCenter().Text(item.Present.ToString());
                        table.Cell().Element(CellContentStyle).AlignCenter().Text(item.Absent.ToString());
                        table.Cell().Element(CellContentStyle).AlignCenter().Text(item.Late.ToString());
                        table.Cell().Element(CellContentStyle).AlignCenter().Text($"{item.AttendanceRate}%");
                    }
                });
            });
        }

        private void AddSectionSummarySection(IContainer container, string title, IEnumerable<SectionDailySummaryDto> data)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(15).PaddingBottom(5).Text(title).FontSize(11).Bold();
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25); // SL
                        columns.RelativeColumn();   // Name
                        columns.ConstantColumn(40); // Total
                        columns.ConstantColumn(40); // Pres
                        columns.ConstantColumn(40); // Abs
                        columns.ConstantColumn(40); // Late
                        columns.ConstantColumn(45); // Rate
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("SL");
                        header.Cell().Element(CellStyle).Text("Name");
                        header.Cell().Element(CellStyle).Text("Tot");
                        header.Cell().Element(CellStyle).Text("Prs");
                        header.Cell().Element(CellStyle).Text("Abs");
                        header.Cell().Element(CellStyle).Text("Lat");
                        header.Cell().Element(CellStyle).Text("Rate%");
                    });

                    int sl = 1;
                    foreach (var item in data.Take(20))
                    {
                        table.Cell().Element(CellContentStyle).AlignCenter().Text(sl++.ToString());
                        table.Cell().Element(CellContentStyle).Text(item.SectionName);
                        table.Cell().Element(CellContentStyle).AlignCenter().Text(item.TotalEmployees.ToString());
                        table.Cell().Element(CellContentStyle).AlignCenter().Text(item.Present.ToString());
                        table.Cell().Element(CellContentStyle).AlignCenter().Text(item.Absent.ToString());
                        table.Cell().Element(CellContentStyle).AlignCenter().Text(item.Late.ToString());
                        table.Cell().Element(CellContentStyle).AlignCenter().Text($"{item.AttendanceRate}%");
                    }
                });
            });
        }

        private IContainer CellStyle(IContainer container)
        {
            return container.DefaultTextStyle(x => x.SemiBold())
                            .PaddingVertical(5)
                            .BorderBottom(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Background(Colors.Grey.Lighten4)
                            .PaddingHorizontal(5)
                            .AlignCenter();
        }

        private IContainer CellContentStyle(IContainer container)
        {
            return container.PaddingVertical(3)
                            .BorderBottom(1)
                            .BorderColor(Colors.Grey.Lighten3)
                            .PaddingHorizontal(5);
        }

        private void AddSummaryHeader(ExcelWorksheet worksheet, Company? company, string title, DateTime date, int columns)
        {
            string companyName = company?.CompanyNameEn ?? "HR HUB";
            string address = company?.Address ?? "Industrial Area, Dhaka, Bangladesh";
            string colRange = $"A1:{(char)('A' + columns - 1)}";

            worksheet.Cells[$"{colRange}1"].Merge = true;
            worksheet.Cells["A1"].Value = companyName;
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[$"{colRange}2"].Merge = true;
            worksheet.Cells["A2"].Value = address;
            worksheet.Cells["A2"].Style.Font.Size = 10;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[$"{colRange}3"].Merge = true;
            worksheet.Cells["A3"].Value = title;
            worksheet.Cells["A3"].Style.Font.Size = 12;
            worksheet.Cells["A3"].Style.Font.Bold = true;
            worksheet.Cells["A3"].Style.Font.UnderLine = true;
            worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[$"{colRange}4"].Merge = true;
            worksheet.Cells["A4"].Value = $"Date: {date:dd MMMM yyyy}";
            worksheet.Cells["A4"].Style.Font.Size = 10;
            worksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Page Setup
            worksheet.PrinterSettings.Orientation = OfficeOpenXml.eOrientation.Portrait;
            worksheet.PrinterSettings.FitToPage = true;
            worksheet.PrinterSettings.FitToWidth = 1;
            worksheet.PrinterSettings.PaperSize = OfficeOpenXml.ePaperSize.A4;
        }

        private void AddSummaryRow(ExcelWorksheet sheet, ref int row, string metric, object value, System.Drawing.Color? color = null)
        {
            sheet.Cells[row, 1].Value = metric;
            sheet.Cells[row, 2].Value = value;
            if (color.HasValue) sheet.Cells[row, 2].Style.Font.Color.SetColor(color.Value);
            row++;
        }

        private void AddSummaryTableHeader(ExcelWorksheet sheet, Company? company, string title, DateTime date, int columns)
        {
            AddSummaryHeader(sheet, company, title, date, columns);
            int row = 6;
            string[] headers = { "SL", "Name", "Total", "Present", "Absent", "Late", "Leave", "Rate (%)" };
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[row, i + 1].Value = headers[i];
                sheet.Cells[row, i + 1].Style.Font.Bold = true;
                sheet.Cells[row, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                sheet.Cells[row, i + 1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                sheet.Cells[row, i + 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                sheet.Cells[row, i + 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                sheet.Cells[row, i + 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }
        }

        private void FillSummaryTable(ExcelWorksheet sheet, IEnumerable<dynamic> data)
        {
            int row = 7;
            int sl = 1;
            foreach (var item in data)
            {
                sheet.Cells[row, 1].Value = sl++;
                sheet.Cells[row, 2].Value = item.Name;
                sheet.Cells[row, 3].Value = item.TotalEmployees;
                sheet.Cells[row, 4].Value = item.Present;
                sheet.Cells[row, 5].Value = item.Absent;
                sheet.Cells[row, 6].Value = item.Late;
                sheet.Cells[row, 7].Value = item.OnLeave;
                sheet.Cells[row, 8].Value = item.AttendanceRate;

                for (int i = 1; i <= 8; i++)
                {
                    sheet.Cells[row, i].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    sheet.Cells[row, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    sheet.Cells[row, i].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    sheet.Cells[row, i].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }
                row++;
            }
            sheet.Cells.AutoFitColumns();
            sheet.Column(1).Width = 5;
            sheet.Column(2).Width = 30;
        }

        // GET: api/attendance/job-card
        [HttpGet("job-card")]
        public async Task<ActionResult<JobCardResponseDto>> GetJobCard(
            [FromQuery] int employeeId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            try
            {
                // Get employee details
                var employee = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Designation)
                    .Include(e => e.Section)
                    .Include(e => e.Shift)
                    .FirstOrDefaultAsync(e => e.Id == employeeId);

                if (employee == null)
                    return NotFound(new { message = "Employee not found" });

                // Get attendance records for the date range
                var attendances = await _context.Attendances
                    .Where(a => a.EmployeeId == employeeId && 
                               a.Date.Date >= fromDate.Date && 
                               a.Date.Date <= toDate.Date)
                    .OrderBy(a => a.Date)
                    .ToListAsync();

                // Generate all dates in range
                var allDates = new List<DateTime>();
                for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
                {
                    allDates.Add(date);
                }

                // Create job card records
                var jobCardRecords = new List<JobCardDto>();
                int presentDays = 0, absentDays = 0, weekendDays = 0, holidayDays = 0;
                decimal totalOT = 0;
                int totalLate = 0, totalEarly = 0;

                foreach (var date in allDates)
                {
                    var attendance = attendances.FirstOrDefault(a => a.Date.Date == date.Date);
                    var dayName = date.ToString("ddd");
                    
                    // Determine if weekend (Friday/Saturday based on your business logic)
                    bool isWeekend = dayName == "Fri"; // Adjust based on your weekend days

                    JobCardDto record;
                    
                    if (attendance != null)
                    {
                        // Calculate late minutes (assuming shift starts at 9:00 AM)
                        int lateMinutes = 0;
                        if (attendance.InTime != null && attendance.Status == "Late")
                        {
                            // Simple calculation - you can make this more sophisticated
                            lateMinutes = 15; // Default late minutes
                        }

                        // Calculate total hours (assuming 9 hours standard)
                        decimal totalHours = attendance.Status == "Present" || attendance.Status == "Late" ? 9 + attendance.OTHours : 0;

                        record = new JobCardDto
                        {
                            Date = date.ToString("dd MMM"),
                            Day = dayName,
                            Status = attendance.Status,
                            InTime = attendance.InTime,
                            OutTime = attendance.OutTime,
                            LateMinutes = lateMinutes,
                            EarlyMinutes = 0,
                            OTHours = attendance.OTHours,
                            TotalHours = totalHours,
                            Remarks = attendance.Status == "Absent" ? "Uninformed" : ""
                        };

                        // Update counters
                        if (attendance.Status == "Present" || attendance.Status == "Late") presentDays++;
                        else if (attendance.Status == "Absent") absentDays++;
                        else if (attendance.Status == "Off Day") weekendDays++;
                        else if (attendance.Status == "Holiday") holidayDays++;

                        totalOT += attendance.OTHours;
                        totalLate += lateMinutes;
                    }
                    else if (isWeekend)
                    {
                        record = new JobCardDto
                        {
                            Date = date.ToString("dd MMM"),
                            Day = dayName,
                            Status = "Weekend",
                            InTime = "-",
                            OutTime = "-",
                            LateMinutes = 0,
                            EarlyMinutes = 0,
                            OTHours = 0,
                            TotalHours = 0,
                            Remarks = "Weekly Off"
                        };
                        weekendDays++;
                    }
                    else
                    {
                        // No attendance record and not weekend - mark as absent
                        record = new JobCardDto
                        {
                            Date = date.ToString("dd MMM"),
                            Day = dayName,
                            Status = "Absent",
                            InTime = "-",
                            OutTime = "-",
                            LateMinutes = 0,
                            EarlyMinutes = 0,
                            OTHours = 0,
                            TotalHours = 0,
                            Remarks = "No Record"
                        };
                        absentDays++;
                    }

                    jobCardRecords.Add(record);
                }

                var response = new JobCardResponseDto
                {
                    Employee = new EmployeeJobCardDto
                    {
                        EmployeeId = employee.Id,
                        EmployeeIdCard = employee.EmployeeId,
                        EmployeeName = employee.FullNameEn,
                        Department = employee.Department?.NameEn ?? "N/A",
                        Designation = employee.Designation?.NameEn ?? "N/A",
                        Section = employee.Section?.NameEn ?? "N/A",
                        JoiningDate = employee.JoinDate.ToString("MMM dd, yyyy"),
                        Grade = null, // Grade not available in Employee model
                        Shift = employee.Shift?.NameEn ?? "N/A"
                    },
                    Summary = new JobCardSummaryDto
                    {
                        PresentDays = presentDays,
                        AbsentDays = absentDays,
                        WeekendDays = weekendDays,
                        HolidayDays = holidayDays,
                        TotalOTHours = totalOT,
                        TotalLateMinutes = totalLate,
                        TotalEarlyMinutes = totalEarly
                    },
                    AttendanceRecords = jobCardRecords,
                    FromDate = fromDate,
                    ToDate = toDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the job card.", error = ex.Message });
            }
        }

        // POST: api/attendance/manual-entry
        [HttpPost("manual-entry")]
        public async Task<ActionResult<ManualAttendanceResponseDto>> CreateManualEntry([FromBody] ManualAttendanceDto dto)
        {
            try
            {
                // Get current user
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

                // Validate employee exists
                var employee = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId);

                if (employee == null)
                    return NotFound(new { message = "Employee not found" });

                // Check if attendance already exists for this date
                var existing = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == dto.EmployeeId && a.Date.Date == dto.Date.Date);

                if (existing != null)
                {
                    // Update existing record
                    existing.InTime = dto.InTime;
                    existing.OutTime = dto.OutTime;
                    existing.Status = dto.Status;
                    existing.Reason = dto.Reason;
                    existing.Remarks = dto.Remarks;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = userName;

                    await _context.SaveChangesAsync();

                    return Ok(new ManualAttendanceResponseDto
                    {
                        Id = existing.Id,
                        EmployeeId = employee.Id,
                        EmployeeIdCard = employee.EmployeeId,
                        EmployeeName = employee.FullNameEn,
                        Date = existing.Date,
                        InTime = existing.InTime,
                        OutTime = existing.OutTime,
                        Status = existing.Status,
                        Reason = existing.Reason ?? "",
                        Remarks = existing.Remarks ?? "",
                        CreatedBy = existing.UpdatedBy ?? existing.CreatedBy ?? "",
                        CreatedAt = existing.UpdatedAt ?? existing.CreatedAt
                    });
                }
                else
                {
                    // Create new record
                    var attendance = new Attendance
                    {
                        EmployeeId = dto.EmployeeId,
                        Date = dto.Date.Date,
                        InTime = dto.InTime,
                        OutTime = dto.OutTime,
                        Status = dto.Status,
                        OTHours = 0, // Can be calculated based on shift
                        Reason = dto.Reason,
                        Remarks = dto.Remarks,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userName
                    };

                    _context.Attendances.Add(attendance);
                    await _context.SaveChangesAsync();

                    return Ok(new ManualAttendanceResponseDto
                    {
                        Id = attendance.Id,
                        EmployeeId = employee.Id,
                        EmployeeIdCard = employee.EmployeeId,
                        EmployeeName = employee.FullNameEn,
                        Date = attendance.Date,
                        InTime = attendance.InTime,
                        OutTime = attendance.OutTime,
                        Status = attendance.Status,
                        Reason = attendance.Reason ?? "",
                        Remarks = attendance.Remarks ?? "",
                        CreatedBy = attendance.CreatedBy ?? "",
                        CreatedAt = attendance.CreatedAt
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating manual entry.", error = ex.Message });
            }
        }

        // GET: api/attendance/manual-entry/history
        [HttpGet("manual-entry/history")]
        public async Task<ActionResult<IEnumerable<ManualAttendanceHistoryDto>>> GetManualEntryHistory(
            [FromQuery] int? employeeId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Attendances
                    .Include(a => a.Employee)
                    .Where(a => a.Reason != null) // Only manual entries
                    .AsQueryable();

                if (employeeId.HasValue)
                    query = query.Where(a => a.EmployeeId == employeeId.Value);

                if (fromDate.HasValue)
                    query = query.Where(a => a.Date.Date >= fromDate.Value.Date);

                if (toDate.HasValue)
                    query = query.Where(a => a.Date.Date <= toDate.Value.Date);

                var history = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(pageSize)
                    .Select(a => new ManualAttendanceHistoryDto
                    {
                        Id = a.Id,
                        EmployeeIdCard = a.Employee!.EmployeeId,
                        EmployeeName = a.Employee!.FullNameEn,
                        Date = a.Date,
                        InTime = a.InTime,
                        OutTime = a.OutTime,
                        Status = a.Status,
                        Reason = a.Reason ?? "",
                        CreatedBy = a.UpdatedBy ?? a.CreatedBy ?? "",
                        CreatedAt = a.UpdatedAt ?? a.CreatedAt
                    })
                    .ToListAsync();

                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching manual entry history.", error = ex.Message });
            }
        }

        // GET: api/attendance/missing-entries
        [HttpGet("missing-entries")]
        public async Task<ActionResult<MissingEntryResponseDto>> GetMissingEntries(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] int? departmentId,
            [FromQuery] int? designationId,
            [FromQuery] int? sectionId,
            [FromQuery] string? searchTerm)
        {
            try
            {
                // Get all active employees with filters
                var employeesQuery = _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Designation)
                    .Include(e => e.Shift)
                    .Where(e => e.IsActive);

                if (departmentId.HasValue)
                    employeesQuery = employeesQuery.Where(e => e.DepartmentId == departmentId.Value);

                if (designationId.HasValue)
                    employeesQuery = employeesQuery.Where(e => e.DesignationId == designationId.Value);

                if (sectionId.HasValue)
                    employeesQuery = employeesQuery.Where(e => e.SectionId == sectionId.Value);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                    employeesQuery = employeesQuery.Where(e => 
                        e.EmployeeId.Contains(searchTerm) || 
                        e.FullNameEn.Contains(searchTerm));

                var employees = await employeesQuery.ToListAsync();

                // Get all attendance records for the date range
                var attendances = await _context.Attendances
                    .Where(a => a.Date.Date >= fromDate.Date && a.Date.Date <= toDate.Date)
                    .ToListAsync();

                // Find missing entries
                var missingEntries = new List<MissingEntryDto>();
                int idCounter = 1;
                
                foreach (var employee in employees)
                {
                    // Check each date in the range
                    for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
                    {
                        // Skip weekends (Friday for now - can be made configurable)
                        if (date.DayOfWeek == DayOfWeek.Friday)
                            continue;

                        var attendance = attendances.FirstOrDefault(a => 
                            a.EmployeeId == employee.Id && 
                            a.Date.Date == date.Date);

                        if (attendance != null)
                        {
                            // Check for missing in/out times
                            bool missingIn = string.IsNullOrWhiteSpace(attendance.InTime);
                            bool missingOut = string.IsNullOrWhiteSpace(attendance.OutTime);

                            if (missingIn || missingOut)
                            {
                                string missingType;
                                string status;

                                if (missingIn && missingOut)
                                {
                                    missingType = "Both (No Punch)";
                                    status = "Critical";
                                }
                                else if (missingIn)
                                {
                                    missingType = "In Time";
                                    status = "Pending";
                                }
                                else
                                {
                                    missingType = "Out Time";
                                    status = "Pending";
                                }

                                missingEntries.Add(new MissingEntryDto
                                {
                                    Id = idCounter++,
                                    EmployeeId = employee.Id,
                                    EmployeeIdCard = employee.EmployeeId,
                                    EmployeeName = employee.FullNameEn,
                                    Department = employee.Department?.NameEn ?? "N/A",
                                    Designation = employee.Designation?.NameEn ?? "N/A",
                                    Shift = employee.Shift?.NameEn,
                                    Date = date,
                                    InTime = attendance.InTime,
                                    OutTime = attendance.OutTime,
                                    MissingType = missingType,
                                    Status = status
                                });
                            }
                        }
                        else
                        {
                            // No attendance record at all - critical
                            missingEntries.Add(new MissingEntryDto
                            {
                                Id = idCounter++,
                                EmployeeId = employee.Id,
                                EmployeeIdCard = employee.EmployeeId,
                                EmployeeName = employee.FullNameEn,
                                Department = employee.Department?.NameEn ?? "N/A",
                                Designation = employee.Designation?.NameEn ?? "N/A",
                                Shift = employee.Shift?.NameEn,
                                Date = date,
                                InTime = null,
                                OutTime = null,
                                MissingType = "Both (No Punch)",
                                Status = "Critical"
                            });
                        }
                    }
                }

                // Calculate summary
                var summary = new MissingEntrySummaryDto
                {
                    TotalMissing = missingEntries.Count,
                    MissingInTime = missingEntries.Count(e => e.MissingType == "In Time"),
                    MissingOutTime = missingEntries.Count(e => e.MissingType == "Out Time"),
                    MissingBoth = missingEntries.Count(e => e.MissingType == "Both (No Punch)"),
                    CriticalCount = missingEntries.Count(e => e.Status == "Critical")
                };

                return Ok(new MissingEntryResponseDto
                {
                    Summary = summary,
                    Entries = missingEntries.OrderByDescending(e => e.Date).ThenBy(e => e.EmployeeName).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching missing entries.", error = ex.Message });
            }
        }

        // GET: api/attendance/absenteeism-records
        [HttpGet("absenteeism-records")]
        public async Task<ActionResult<AbsenteeismResponseDto>> GetAbsenteeismRecords(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] int? departmentId,
            [FromQuery] int? designationId,
            [FromQuery] string? searchTerm)
        {
            try
            {
                var query = _context.Attendances
                    .Include(a => a.Employee)
                    .ThenInclude(e => e!.Department)
                    .Include(a => a.Employee)
                    .ThenInclude(e => e!.Designation)
                    .Where(a => a.Date.Date >= fromDate.Date && a.Date.Date <= toDate.Date)
                    .Where(a => a.Status == "Absent" || a.Status == "On Leave");

                if (departmentId.HasValue)
                    query = query.Where(a => a.Employee!.DepartmentId == departmentId.Value);

                if (designationId.HasValue)
                    query = query.Where(a => a.Employee!.DesignationId == designationId.Value);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                    query = query.Where(a => 
                        a.Employee!.EmployeeId.Contains(searchTerm) || 
                        a.Employee!.FullNameEn.Contains(searchTerm));

                var absences = await query.ToListAsync();

                // Group by employee to calculate consecutive days
                var records = absences
                    .GroupBy(a => a.EmployeeId)
                    .SelectMany(g =>
                    {
                        var empAbsences = g.OrderBy(a => a.Date).ToList();
                        var result = new List<AbsenteeismRecordDto>();
 
                        for (int i = 0; i < empAbsences.Count; i++)
                        {
                            var attendance = empAbsences[i];
                            int consecutiveDays = 1;

                            // Count consecutive days
                            for (int j = i + 1; j < empAbsences.Count; j++)
                            {
                                var nextDate = empAbsences[j].Date;
                                var currentDate = empAbsences[j - 1].Date;
                                if ((nextDate - currentDate).Days == 1)
                                    consecutiveDays++;
                                else
                                    break;
                            }

                            result.Add(new AbsenteeismRecordDto
                            {
                                Id = attendance.Id,
                                EmployeeId = attendance.EmployeeId,
                                EmployeeIdCard = attendance.Employee!.EmployeeId,
                                EmployeeName = attendance.Employee!.FullNameEn,
                                Department = attendance.Employee!.Department?.NameEn ?? "N/A",
                                Designation = attendance.Employee!.Designation?.NameEn ?? "N/A",
                                Date = attendance.Date,
                                Status = attendance.Status,
                                ConsecutiveDays = consecutiveDays,
                                Remarks = attendance.Remarks
                            });
                        }

                        return result;
                    })
                    .ToList();

                var summary = new AbsenteeismSummaryDto
                {
                    TotalAbsent = records.Count,
                    AbsentWithoutLeave = records.Count(r => r.Status == "Absent"),
                    OnLeave = records.Count(r => r.Status == "On Leave"),
                    CriticalCases = records.Count(r => r.ConsecutiveDays >= 3)
                };

                return Ok(new AbsenteeismResponseDto
                {
                    Summary = summary,
                    Records = records.OrderByDescending(r => r.Date).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching absenteeism records.", error = ex.Message });
            }
        }

        // GET: api/attendance/daily-ot-sheet
        [HttpGet("daily-ot-sheet")]
        public async Task<ActionResult<OTSheetResponseDto>> GetDailyOTSheet(
            [FromQuery] DateTime date,
            [FromQuery] int? departmentId,
            [FromQuery] int? designationId,
            [FromQuery] string? searchTerm)
        {
            try
            {
                var query = _context.Attendances
                    .Include(a => a.Employee)
                    .ThenInclude(e => e!.Department)
                    .Include(a => a.Employee)
                    .ThenInclude(e => e!.Designation)
                    .Where(a => a.Date.Date == date.Date)
                    .Where(a => a.OTHours > 0);

                if (departmentId.HasValue)
                    query = query.Where(a => a.Employee!.DepartmentId == departmentId.Value);

                if (designationId.HasValue)
                    query = query.Where(a => a.Employee!.DesignationId == designationId.Value);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                    query = query.Where(a => 
                        a.Employee!.EmployeeId.Contains(searchTerm) || 
                        a.Employee!.FullNameEn.Contains(searchTerm));

                var records = await query
                    .Select(a => new DailyOTSheetDto
                    {
                        Id = a.Id,
                        EmployeeId = a.EmployeeId,
                        EmployeeIdCard = a.Employee!.EmployeeId,
                        EmployeeName = a.Employee!.FullNameEn,
                        Department = a.Employee!.Department!.NameEn,
                        Designation = a.Employee!.Designation!.NameEn,
                        Date = a.Date,
                        InTime = a.InTime,
                        OutTime = a.OutTime,
                        RegularHours = 8, // Can be calculated based on shift
                        OTHours = a.OTHours,
                        Remarks = a.Remarks
                    })
                    .ToListAsync();

                return Ok(new OTSheetResponseDto
                {
                    Records = records,
                    TotalOTHours = records.Sum(r => r.OTHours),
                    TotalEmployees = records.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching OT sheet.", error = ex.Message });
            }
        }

        // GET: api/attendance/daily-ot-summary
        [HttpGet("daily-ot-summary")]
        public async Task<ActionResult<OTSummaryResponseDto>> GetDailyOTSummary([FromQuery] DateTime date)
        {
            try
            {
                var records = await _context.Attendances
                    .Include(a => a.Employee)
                    .ThenInclude(e => e!.Department)
                    .Where(a => a.Date.Date == date.Date)
                    .Where(a => a.OTHours > 0)
                    .ToListAsync();

                int summaryId = 1;
                var departmentSummaries = records
                    .GroupBy(a => a.Employee!.Department)
                    .Select(g => new DailyOTSummaryDto
                    {
                        Id = summaryId++,
                        Department = g.Key?.NameEn ?? "N/A",
                        EmployeeCount = g.Count(),
                        TotalOTHours = g.Sum(a => a.OTHours),
                        AverageOTPerEmployee = g.Average(a => a.OTHours),
                        TotalRegularHours = g.Count() * 8m // 8 hours per employee
                    })
                    .OrderByDescending(s => s.TotalOTHours)
                    .ToList();

                return Ok(new OTSummaryResponseDto
                {
                    DepartmentSummaries = departmentSummaries,
                    GrandTotalOTHours = departmentSummaries.Sum(s => s.TotalOTHours),
                    TotalEmployees = records.Count,
                    Date = date
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching OT summary.", error = ex.Message });
            }
        }

        // POST: api/attendance/seed-mock
        [HttpPost("seed-mock")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> SeedMockData([FromQuery] DateTime date)
        {
            var employees = await _context.Employees.ToListAsync();
            var existing = await _context.Attendances.Where(a => a.Date.Date == date.Date).AnyAsync();
            if (existing) return BadRequest("Mock data already exists for this date");

            var random = new Random();
            var statuses = new[] { "Present", "Present", "Present", "Present", "Late", "Absent", "On Leave" };

            foreach (var emp in employees)
            {
                var status = statuses[random.Next(statuses.Length)];
                var inTime = status == "Absent" || status == "On Leave" ? null : "09:00 AM";
                var outTime = status == "Absent" || status == "On Leave" ? null : "06:00 PM";
                
                _context.Attendances.Add(new Attendance
                {
                    EmployeeId = emp.Id,
                    Date = date.Date,
                    InTime = inTime,
                    OutTime = outTime,
                    Status = status,
                    OTHours = (status == "Present" && emp.IsOTEnabled) ? (decimal)(random.NextDouble() * 2) : 0,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "SystemMock"
                });
            }

            await _context.SaveChangesAsync();
            return Ok("Mock data seeded successfully");
        }

        // POST: api/attendance/process
        [HttpPost("process")]
        public async Task<IActionResult> ProcessDailyData([FromBody] DailyProcessDto dto)
        {
            try
            {
                var startDate = dto.FromDate.Date;
                var endDate = (dto.ToDate ?? dto.FromDate).Date;

                if (startDate > endDate)
                    return BadRequest("FromDate must be before or equal to ToDate");

                // Get all active employees
                var employeesQuery = _context.Employees
                    .Where(e => e.IsActive)
                    .AsQueryable();

                if (dto.DepartmentId.HasValue)
                    employeesQuery = employeesQuery.Where(e => e.DepartmentId == dto.DepartmentId);

                var employees = await employeesQuery.ToListAsync();
                var shifts = await _context.Shifts.ToListAsync();
                
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

                int processedCount = 0;

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    // Pre-fetch data for the current date to optimize
                    var rosters = await _context.EmployeeShiftRosters
                        .Where(r => r.Date.Date == date.Date)
                        .ToListAsync();

                    var leaves = await _context.LeaveApplications
                        .Where(l => l.Status == "Approved" && l.StartDate.Date <= date.Date && l.EndDate.Date >= date.Date)
                        .ToListAsync();

                    var existingAttendances = await _context.Attendances
                        .Where(a => a.Date.Date == date.Date)
                        .ToListAsync();

                    foreach (var emp in employees)
                    {
                        var roster = rosters.FirstOrDefault(r => r.EmployeeId == emp.Id);
                        var leave = leaves.FirstOrDefault(l => l.EmployeeId == emp.Id);
                        var attendance = existingAttendances.FirstOrDefault(a => a.EmployeeId == emp.Id);

                        // Determine Shift
                        var shiftId = roster?.ShiftId ?? emp.ShiftId;
                        var shift = shifts.FirstOrDefault(s => s.Id == shiftId);

                        if (shift == null) continue;

                        // Process status
                        string status = "Absent";
                        decimal otHours = 0;

                        if (leave != null)
                        {
                            status = "On Leave";
                        }
                        else if (roster?.IsOffDay == true || IsWeekend(date, shift.Weekends))
                        {
                            status = "Off Day";
                        }
                        else if (attendance != null && !string.IsNullOrEmpty(attendance.InTime))
                        {
                            // Calculate Late
                            var inTime = ParseTime(attendance.InTime);
                            var shiftInTime = ParseTime(shift.InTime);
                            var lateLimit = ParseTime(shift.LateInTime) ?? shiftInTime?.Add(TimeSpan.FromMinutes(15));

                            if (inTime > lateLimit)
                            {
                                status = "Late";
                            }
                            else
                            {
                                status = "Present";
                            }

                            // Calculate OT
                            if (emp.IsOTEnabled && !string.IsNullOrEmpty(attendance.OutTime) && !string.IsNullOrEmpty(shift.OutTime))
                            {
                                var outTime = ParseTime(attendance.OutTime);
                                var shiftOutTime = ParseTime(shift.OutTime);

                                if (outTime > shiftOutTime)
                                {
                                    var diff = (outTime.Value - shiftOutTime.Value).TotalHours;
                                    otHours = (decimal)Math.Max(0, Math.Floor(diff)); // Usually OT is calculated in full hours
                                }
                            }
                        }

                        // Update or Create Attendance Record
                        if (attendance != null)
                        {
                            attendance.Status = status;
                            attendance.OTHours = otHours;
                            attendance.UpdatedAt = DateTime.UtcNow;
                            attendance.UpdatedBy = userName;
                        }
                        else
                        {
                            _context.Attendances.Add(new Attendance
                            {
                                EmployeeId = emp.Id,
                                Date = date,
                                Status = status,
                                OTHours = otHours,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = userName
                            });
                        }
                        processedCount++;
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = $"Successfully processed {processedCount} records from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error during processing: " + ex.Message });
            }
        }

        private bool IsWeekend(DateTime date, string? weekends)
        {
            if (string.IsNullOrEmpty(weekends)) return false;
            var dayName = date.DayOfWeek.ToString();
            return weekends.Split(',').Any(w => w.Trim().Equals(dayName, StringComparison.OrdinalIgnoreCase));
        }

        private TimeSpan? ParseTime(string? timeStr)
        {
            if (string.IsNullOrWhiteSpace(timeStr)) return null;
            if (DateTime.TryParse(timeStr, out DateTime dt)) return dt.TimeOfDay;
            return null;
        }
    }
}

