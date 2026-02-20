using ERPBackend.Core.Constants;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Core.Entities;
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
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetDailyReport([FromQuery] CommonFilterDto filters)
        {
            try
            {
                var result = await GetDailyReportInternal(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "An error occurred while fetching the daily report.", error = ex.Message });
            }
        }

        // GET: api/attendance/daily-report/export/excel
        [HttpGet("daily-report/export/excel")]
        public async Task<IActionResult> ExportDailyReportToExcel([FromQuery] CommonFilterDto filters)
        {
            try
            {
                var data = (await GetDailyReportInternal(filters)).ToList();
                Company? company = null;
                if (!string.IsNullOrEmpty(filters.CompanyName))
                {
                    company = await _context.Set<Company>()
                        .FirstOrDefaultAsync(c => c.CompanyNameEn == filters.CompanyName);
                }

                company ??= await _context.Set<Company>().FirstOrDefaultAsync();

                using (var package = new ExcelPackage())
                {
                    var date = filters.Date ?? DateTime.Today;
                    // Create Section Wise Sheet
                    var sectionSheet = package.Workbook.Worksheets.Add("Section Wise");
                    CreateAttendanceWorksheet(sectionSheet, data, company, date, "Section");

                    // Create Line Wise Sheet
                    var lineSheet = package.Workbook.Worksheets.Add("Line Wise");
                    CreateAttendanceWorksheet(lineSheet, data, company, date, "Line");

                    var content = package.GetAsByteArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"DailyAttendanceReport_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "An error occurred while exporting to Excel.", error = ex.Message });
            }
        }

        private void CreateAttendanceWorksheet(ExcelWorksheet worksheet, IEnumerable<AttendanceDto> data,
            Company? company, DateTime date, string groupBy)
        {
            // 0. Page Setup
            worksheet.PrinterSettings.Orientation = eOrientation.Landscape;
            worksheet.PrinterSettings.FitToPage = true;
            worksheet.PrinterSettings.FitToWidth = 1;
            worksheet.PrinterSettings.FitToHeight = 0;
            worksheet.PrinterSettings.PaperSize = ePaperSize.A4;
            worksheet.PrinterSettings.TopMargin = 0.5m;
            worksheet.PrinterSettings.BottomMargin = 0.5m;
            worksheet.PrinterSettings.LeftMargin = 0.5m;
            worksheet.PrinterSettings.RightMargin = 0.5m;

            // 1. Header Section
            string companyName = company?.CompanyNameEn ?? "HR HUB";
            string address = company?.AddressEn ?? "Industrial Area, Dhaka, Bangladesh";

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
                worksheet.Cells[currentRow, 1].Style.Fill.BackgroundColor
                    .SetColor(System.Drawing.Color.FromArgb(240, 240, 240));
                currentRow++;

                foreach (var item in group)
                {
                    worksheet.Cells[currentRow, 1].Value = sl++;
                    worksheet.Cells[currentRow, 2].Value = item.EmployeeId;
                    worksheet.Cells[currentRow, 3].Value = item.EmployeeName;
                    worksheet.Cells[currentRow, 4].Value = item.Department;
                    worksheet.Cells[currentRow, 5].Value = item.Designation;
                    worksheet.Cells[currentRow, 6].Value = item.Shift;
                    worksheet.Cells[currentRow, 7].Value = item.InTime?.ToString("HH:mm") ?? "-";
                    worksheet.Cells[currentRow, 8].Value = item.OutTime?.ToString("HH:mm") ?? "-";

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
            worksheet.Cells[currentRow, 3].Value =
                data.Count(x => x.Status.StartsWith("Present") || x.Status == "Late");
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
        public async Task<IActionResult> ExportDailyReportToPdf([FromQuery] CommonFilterDto filters)
        {
            try
            {
                var data = (await GetDailyReportInternal(filters)).ToList();
                var date = filters.Date ?? DateTime.Today;

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
                                col.Item().Text("Daily Attendance Report").FontSize(16).SemiBold()
                                    .FontColor(Colors.Blue.Medium);
                                col.Item().Text($"Date: {date:dd MMMM yyyy}").FontSize(10);
                            });
                        });

                        page.Content().PaddingVertical(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25); // SL
                                columns.ConstantColumn(80); // Emp ID
                                columns.RelativeColumn(); // Name
                                columns.RelativeColumn(); // Dept
                                columns.RelativeColumn(); // Desig
                                columns.ConstantColumn(50); // In
                                columns.ConstantColumn(50); // Out
                                columns.ConstantColumn(60); // Status
                                columns.ConstantColumn(40); // OT
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCellStyle).Text("SL");
                                header.Cell().Element(HeaderCellStyle).Text("Emp ID");
                                header.Cell().Element(HeaderCellStyle).Text("Name");
                                header.Cell().Element(HeaderCellStyle).Text("Dept");
                                header.Cell().Element(HeaderCellStyle).Text("Desig");
                                header.Cell().Element(HeaderCellStyle).Text("In");
                                header.Cell().Element(HeaderCellStyle).Text("Out");
                                header.Cell().Element(HeaderCellStyle).Text("Status");
                                header.Cell().Element(HeaderCellStyle).Text("OT");

                                static IContainer HeaderCellStyle(IContainer container)
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
                                table.Cell().Element(DataCellStyle).Text(slCount++.ToString());
                                table.Cell().Element(DataCellStyle).Text(item.EmployeeId);
                                table.Cell().Element(DataCellStyle).Text(item.EmployeeName);
                                table.Cell().Element(DataCellStyle).Text(item.Department);
                                table.Cell().Element(DataCellStyle).Text(item.Designation);
                                table.Cell().Element(DataCellStyle).Text(item.InTime?.ToString("HH:mm") ?? "-");
                                table.Cell().Element(DataCellStyle).Text(item.OutTime?.ToString("HH:mm") ?? "-");
                                table.Cell().Element(DataCellStyle).Text(item.Status);
                                table.Cell().Element(DataCellStyle).Text(item.OTHours.ToString());

                                static IContainer DataCellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(3);
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
                return StatusCode(500,
                    new { message = "An error occurred while exporting to PDF.", error = ex.Message });
            }
        }

        private async Task<IEnumerable<AttendanceDto>> GetDailyReportInternal(CommonFilterDto filters)
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
                .Include(a => a.Employee)
                .ThenInclude(e => e!.Group)
                .Include(a => a.Employee)
                .ThenInclude(e => e!.Floor)
                .Include(a => a.Shift)
                .AsQueryable();

            query = ApplyAttendanceFilters(query, filters);

            return await query.Select(a => new AttendanceDto
            {
                Id = a.Id,
                EmployeeCard = a.EmployeeCard,
                EmployeeId = a.Employee != null ? a.Employee.EmployeeId : "",
                EmployeeName = a.Employee != null ? a.Employee.FullNameEn : "",
                Department = (a.Employee != null && a.Employee.Department != null)
                    ? a.Employee.Department.NameEn
                    : "N/A",
                Section = (a.Employee != null && a.Employee.Section != null) ? a.Employee.Section.NameEn : "N/A",
                Line = (a.Employee != null && a.Employee.Line != null) ? a.Employee.Line.NameEn : "N/A",
                Designation = (a.Employee != null && a.Employee.Designation != null)
                    ? a.Employee.Designation.NameEn
                    : "N/A",
                Shift = a.Shift != null ? a.Shift.NameEn : (a.Employee != null && a.Employee.Shift != null ? a.Employee.Shift.NameEn : "N/A"),
                Date = a.Date,
                InTime = a.InTime,
                OutTime = a.OutTime,
                Status = a.Status,
                OTHours = a.OTHours,
                ShiftId = a.ShiftId,
                ShiftName = a.Shift != null ? a.Shift.NameEn : null,
                IsOffDay = a.IsOffDay
            }).ToListAsync();
        }

        // GET: api/attendance/summary
        [HttpGet("summary")]
        public async Task<ActionResult<AttendanceSummaryDto>> GetSummary([FromQuery] CommonFilterDto filters)
        {
            try
            {
                var date = filters.Date ?? DateTime.Today;

                var employeeQuery = _context.Employees.Where(e => e.IsActive).AsQueryable();
                employeeQuery = ApplyEmployeeFilters(employeeQuery, filters);
                var totalHeadcount = await employeeQuery.CountAsync();

                var attendanceQuery = _context.Attendances.Where(a => a.Date.Date == date.Date).AsQueryable();
                attendanceQuery = ApplyAttendanceFilters(attendanceQuery, filters);
                var attendances = await attendanceQuery.ToListAsync();

                var present = attendances.Count(a => a.Status.StartsWith("Present") || a.Status == "Late");
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
                return StatusCode(500,
                    new { message = "An error occurred while fetching the attendance summary.", error = ex.Message });
            }
        }

        // GET: api/attendance/daily-summary
        [HttpGet("daily-summary")]
        public async Task<ActionResult<DailySummaryResponseDto>> GetDailySummary([FromQuery] CommonFilterDto filters)
        {
            try
            {
                var summary = await GetDailySummaryInternal(filters);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "An error occurred while fetching the daily summary.", error = ex.Message });
            }
        }

        private async Task<DailySummaryResponseDto> GetDailySummaryInternal(CommonFilterDto filters)
        {
            DateTime date = filters.Date ?? DateTime.Today;

            // 1. Fetch all active employees with their details to calculate total headcount per group
            var employeeQuery = _context.Employees
                .Where(e => e.IsActive)
                .Include(e => e.Department)
                .Include(e => e.Section)
                .Include(e => e.Designation)
                .Include(e => e.Line)
                .Include(e => e.Group)
                .AsQueryable();

            employeeQuery = ApplyEmployeeFilters(employeeQuery, filters);
            var allEmployees = await employeeQuery.ToListAsync();

            // 2. Fetch all attendance for the date
            var attendanceQuery = _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.Date.Date == date.Date)
                .AsQueryable();

            attendanceQuery = ApplyAttendanceFilters(attendanceQuery, filters);
            var attendances = await attendanceQuery.ToListAsync();

            // Filter attendances to only include active employees to ensure rate consistency
            var activeEmpIds = allEmployees.Select(e => e.Id).ToHashSet();
            attendances = attendances.Where(a => activeEmpIds.Contains(a.EmployeeCard)).ToList();

            // 3. Overall Summary
            var present = attendances.Count(a => a.Status.StartsWith("Present") || a.Status == "Late");
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
                var deptAttendances = attendances.Where(a => empIds.Contains(a.EmployeeCard)).ToList();

                var p = deptAttendances.Count(a => a.Status.StartsWith("Present") || a.Status == "Late");
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
                var secAttendances = attendances.Where(a => empIds.Contains(a.EmployeeCard)).ToList();

                var p = secAttendances.Count(a => a.Status.StartsWith("Present") || a.Status == "Late");
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
                var desigAttendances = attendances.Where(a => empIds.Contains(a.EmployeeCard)).ToList();

                var p = desigAttendances.Count(a => a.Status.StartsWith("Present") || a.Status == "Late");
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
                var lineAttendances = attendances.Where(a => empIds.Contains(a.EmployeeCard)).ToList();

                var p = lineAttendances.Count(a => a.Status.StartsWith("Present") || a.Status == "Late");
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
                var grpAttendances = attendances.Where(a => empIds.Contains(a.EmployeeCard)).ToList();

                var p = grpAttendances.Count(a => a.Status.StartsWith("Present") || a.Status == "Late");
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
                .GroupBy(e => new
                {
                    e.DepartmentId, DepartmentName = e.Department!.NameEn, e.SectionId, SectionName = e.Section!.NameEn
                })
                .Select(g =>
                {
                    var empIds = g.Select(e => e.Id).ToHashSet();
                    var dsAttendances = attendances.Where(a => empIds.Contains(a.EmployeeCard)).ToList();

                    var p = dsAttendances.Count(a => a.Status.StartsWith("Present") || a.Status == "Late");
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
        public async Task<IActionResult> ExportDailySummaryToExcel([FromQuery] CommonFilterDto filters)
        {
            try
            {
                var date = filters.Date ?? DateTime.Today;
                var summaryData = await GetDailySummaryInternal(filters);
                Company? company = null;
                if (!string.IsNullOrEmpty(filters.CompanyName))
                {
                    company = await _context.Set<Company>()
                        .FirstOrDefaultAsync(c => c.CompanyNameEn == filters.CompanyName);
                }

                company ??= await _context.Set<Company>().FirstOrDefaultAsync();

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
                        summarySheet.Cells[row, i + 1].Style.Fill.BackgroundColor
                            .SetColor(System.Drawing.Color.LightGray);
                    }

                    row++;

                    AddSummaryRow(summarySheet, ref row, "Total Headcount", summaryData.OverallSummary.TotalHeadcount);
                    AddSummaryRow(summarySheet, ref row, "Present", summaryData.OverallSummary.PresentCount,
                        System.Drawing.Color.Green);
                    AddSummaryRow(summarySheet, ref row, "Late", summaryData.OverallSummary.LateCount,
                        System.Drawing.Color.Orange);
                    AddSummaryRow(summarySheet, ref row, "Absent", summaryData.OverallSummary.AbsentCount,
                        System.Drawing.Color.Red);
                    AddSummaryRow(summarySheet, ref row, "On Leave", summaryData.OverallSummary.LeaveCount,
                        System.Drawing.Color.Blue);
                    AddSummaryRow(summarySheet, ref row, "Attendance Rate (%)",
                        summaryData.OverallSummary.AttendanceRate);

                    summarySheet.Cells[7, 1, row - 1, 2].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    summarySheet.Cells[7, 1, row - 1, 2].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    summarySheet.Cells[7, 1, row - 1, 2].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    summarySheet.Cells[7, 1, row - 1, 2].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    summarySheet.Column(1).Width = 25;
                    summarySheet.Column(2).Width = 15;

                    // 2. Department Wise Sheet
                    var deptSheet = package.Workbook.Worksheets.Add("Department Wise");
                    AddSummaryTableHeader(deptSheet, company, "Department Wise Summary", date, 8);
                    FillSummaryTable(deptSheet,
                        summaryData.DepartmentSummaries.Select(d => new
                        {
                            Name = d.DepartmentName, d.TotalEmployees, d.Present, d.Absent, d.Late, d.OnLeave,
                            d.AttendanceRate
                        }));

                    // 3. Section Wise Sheet
                    var secSheet = package.Workbook.Worksheets.Add("Section Wise");
                    AddSummaryTableHeader(secSheet, company, "Section Wise Summary", date, 8);
                    FillSummaryTable(secSheet,
                        summaryData.SectionSummaries.Select(s => new
                        {
                            Name = s.SectionName, s.TotalEmployees, s.Present, s.Absent, s.Late, s.OnLeave,
                            s.AttendanceRate
                        }));

                    // 4. Designation Wise Sheet
                    var desigSheet = package.Workbook.Worksheets.Add("Designation Wise");
                    AddSummaryTableHeader(desigSheet, company, "Designation Wise Summary", date, 8);
                    FillSummaryTable(desigSheet,
                        summaryData.DesignationSummaries.Select(d => new
                        {
                            Name = d.DesignationName, d.TotalEmployees, d.Present, d.Absent, d.Late, d.OnLeave,
                            d.AttendanceRate
                        }));

                    // 5. Line Wise Sheet
                    var lineSheet = package.Workbook.Worksheets.Add("Line Wise");
                    AddSummaryTableHeader(lineSheet, company, "Line Wise Summary", date, 8);
                    FillSummaryTable(lineSheet,
                        summaryData.LineSummaries.Select(l => new
                        {
                            Name = l.LineName, l.TotalEmployees, l.Present, l.Absent, l.Late, l.OnLeave,
                            l.AttendanceRate
                        }));

                    // 6. Group Wise Sheet
                    var groupSheet = package.Workbook.Worksheets.Add("Group Wise");
                    AddSummaryTableHeader(groupSheet, company, "Group Wise Summary", date, 8);
                    FillSummaryTable(groupSheet,
                        summaryData.GroupSummaries.Select(g => new
                        {
                            Name = g.GroupName, g.TotalEmployees, g.Present, g.Absent, g.Late, g.OnLeave,
                            g.AttendanceRate
                        }));

                    var content = package.GetAsByteArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"DailyAttendanceSummary_{date:yyyyMMdd}.xlsx");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "An error occurred while exporting summary to Excel.", error = ex.Message });
            }
        }

        // GET: api/attendance/daily-summary/export/pdf
        [HttpGet("daily-summary/export/pdf")]
        public async Task<IActionResult> ExportDailySummaryToPdf([FromQuery] CommonFilterDto filters)
        {
            try
            {
                var date = filters.Date ?? DateTime.Today;
                var summaryData = await GetDailySummaryInternal(filters);
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
                            col.Item().AlignCenter().Text(companyName).FontSize(16).Bold()
                                .FontColor(Colors.Blue.Medium);
                            col.Item().AlignCenter().Text(company?.AddressEn ?? "").FontSize(9);
                            col.Item().PaddingTop(5).AlignCenter().Text("Daily Attendance Summary Report").FontSize(12)
                                .Bold().Underline();
                            col.Item().AlignCenter().Text($"Date: {date:dd MMMM yyyy}").FontSize(10);
                            if (filters.DepartmentId.HasValue)
                            {
                                var deptName = summaryData.DepartmentSummaries.FirstOrDefault()?.DepartmentName ??
                                               "Selected Department";
                                col.Item().AlignCenter().Text($"Filtered by Department: {deptName}").FontSize(9)
                                    .Italic();
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
                                table.Cell().Element(CellContentStyle)
                                    .Text(summaryData.OverallSummary.TotalHeadcount.ToString());

                                table.Cell().Element(CellContentStyle).Text("Present");
                                table.Cell().Element(CellContentStyle)
                                    .Text(summaryData.OverallSummary.PresentCount.ToString())
                                    .FontColor(Colors.Green.Medium);

                                table.Cell().Element(CellContentStyle).Text("Absent");
                                table.Cell().Element(CellContentStyle)
                                    .Text(summaryData.OverallSummary.AbsentCount.ToString())
                                    .FontColor(Colors.Red.Medium);

                                table.Cell().Element(CellContentStyle).Text("On Leave");
                                table.Cell().Element(CellContentStyle)
                                    .Text(summaryData.OverallSummary.LeaveCount.ToString())
                                    .FontColor(Colors.Blue.Medium);

                                table.Cell().Element(CellContentStyle).Text("Attendance Rate");
                                table.Cell().Element(CellContentStyle)
                                    .Text($"{summaryData.OverallSummary.AttendanceRate}%").Bold();
                            });

                            // Department Summaries
                            col.Item().Element(c =>
                                AddDepartmentSummarySection(c, "2. Department Wise Summary",
                                    summaryData.DepartmentSummaries));

                            // Section Summaries
                            col.Item().Element(c =>
                                AddSectionSummarySection(c, "3. Section Wise Summary", summaryData.SectionSummaries));
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

        private void AddDepartmentSummarySection(IContainer container, string title,
            IEnumerable<DepartmentDailySummaryDto> data)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(15).PaddingBottom(5).Text(title).FontSize(11).Bold();
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25); // SL
                        columns.RelativeColumn(); // Name
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

        private void AddSectionSummarySection(IContainer container, string title,
            IEnumerable<SectionDailySummaryDto> data)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(15).PaddingBottom(5).Text(title).FontSize(11).Bold();
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25); // SL
                        columns.RelativeColumn(); // Name
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

        private void AddSummaryHeader(ExcelWorksheet worksheet, Company? company, string title, DateTime date,
            int columns)
        {
            string companyName = company?.CompanyNameEn ?? "HR HUB";
            string address = company?.AddressEn ?? "Industrial Area, Dhaka, Bangladesh";
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

        private void AddSummaryRow(ExcelWorksheet sheet, ref int row, string metric, object value,
            System.Drawing.Color? color = null)
        {
            sheet.Cells[row, 1].Value = metric;
            sheet.Cells[row, 2].Value = value;
            if (color.HasValue) sheet.Cells[row, 2].Style.Font.Color.SetColor(color.Value);
            row++;
        }

        private void AddSummaryTableHeader(ExcelWorksheet sheet, Company? company, string title, DateTime date,
            int columns)
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
        public async Task<ActionResult<JobCardResponseDto>> GetJobCard(int employeeCard, DateTime fromDate,
            DateTime toDate)
        {
            try
            {
                var companyId = int.Parse(User.FindFirst("CompanyId")?.Value ?? "0");
                var from = fromDate.Date;
                var to = toDate.Date;

                // Get employee details using primary key
                var employee = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Designation)
                    .Include(e => e.Section)
                    .Include(e => e.Shift)
                    .FirstOrDefaultAsync(e => e.Id == employeeCard);

                if (employee == null)
                    return NotFound("Employee not found");

                var attendances = await _context.Attendances
                    .Include(a => a.Shift)
                    .Where(a => a.EmployeeCard == employeeCard &&
                                a.Date >= from &&
                                a.Date <= to)
                    .OrderBy(a => a.Date)
                    .ToListAsync();

                // Get shift roster (with fallback support: fetch all up to 'to' date)
                var roster = await _context.EmployeeShiftRosters
                    .Include(r => r.Shift)
                    .Where(r => r.EmployeeId == employeeCard && r.Date <= to)
                    .OrderByDescending(r => r.Date)
                    .ToListAsync();

                // Optimized Log Retrieval: Get logs for the range + 1 day buffer for overnight shifts
                var minDate = from.AddHours(-6); // Buffer for early morning punches
                var maxDate = to.AddDays(1).AddHours(14); // Buffer for late night punches/overnight

                // Split the query to avoid OR performance issues on large datasets
                var dbEmployeeId = employee.Id;
                var stringEmployeeId = employee.EmployeeId;

                var logs = await _context.AttendanceLogs
                    .Where(l => (l.EmployeeCard == dbEmployeeId) &&
                                l.LogTime >= minDate &&
                                l.LogTime <= maxDate)
                    .OrderBy(l => l.LogTime)
                    .ToListAsync();

                // If no logs found via EmployeeCard, try string EmployeeId (for legacy/imported data support)
                if (logs.Count == 0 && !string.IsNullOrEmpty(stringEmployeeId))
                {
                    logs = await _context.AttendanceLogs
                        .Where(l => l.EmployeeId == stringEmployeeId &&
                                    l.LogTime >= minDate &&
                                    l.LogTime <= maxDate)
                        .OrderBy(l => l.LogTime)
                        .ToListAsync();
                }

                // 24-hour overlap logic using RAW LOGS
                var missingOutAttendance = attendances.Where(a => a.InTime.HasValue && !a.OutTime.HasValue).ToList();
                Console.WriteLine(
                    $"[JobCard] Found {missingOutAttendance.Count} attendance records with missing OutTime");

                if (missingOutAttendance.Any())
                {
                    // Reuse the logs we already fetched for the entire range
                    Console.WriteLine($"[JobCard] Using {logs.Count} log entries for processing missing OutTimes");


                    Console.WriteLine($"[JobCard] Retrieved {logs.Count} log entries");
                    foreach (var log in logs)
                    {
                        Console.WriteLine($"  - Log: {log.LogTime:yyyy-MM-dd HH:mm:ss}");
                    }

                    foreach (var att in missingOutAttendance)
                    {
                        var inTime = att.InTime!.Value;
                        Console.WriteLine(
                            $"[JobCard] Processing Date={att.Date:yyyy-MM-dd}, InTime={inTime:yyyy-MM-dd HH:mm:ss}");

                        var nextDay = att.Date.AddDays(1);
                        var nextAtt = attendances.FirstOrDefault(a => a.Date.Date == nextDay.Date);

                        // Use shift end time as search limit, not next day's InTime
                        // Shift ends at 07:10 AM next day, so search up to that time
                        DateTime searchLimit;
                        if (!string.IsNullOrEmpty(employee.Shift?.ActualOutTime))
                        {
                            // Parse the time string (e.g., "07:10 AM")
                            if (TimeOnly.TryParse(employee.Shift.ActualOutTime, out var shiftEndTime))
                            {
                                searchLimit = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day,
                                    shiftEndTime.Hour, shiftEndTime.Minute, 0);
                                Console.WriteLine(
                                    $"  - Using shift end time as searchLimit={searchLimit:yyyy-MM-dd HH:mm:ss}");
                            }
                            else
                            {
                                // Parse failed, default to 24 hours
                                searchLimit = inTime.AddHours(24);
                                Console.WriteLine(
                                    $"  - Parse failed, using 24h limit={searchLimit:yyyy-MM-dd HH:mm:ss}");
                            }
                        }
                        else
                        {
                            // Fallback: 24 hours from InTime
                            searchLimit = inTime.AddHours(24);
                            Console.WriteLine(
                                $"  - No shift end time, using 24h limit={searchLimit:yyyy-MM-dd HH:mm:ss}");
                        }

                        var candidateLogs = logs.Where(l => l.LogTime > inTime.AddMinutes(1) && l.LogTime < searchLimit)
                            .ToList();
                        Console.WriteLine(
                            $"  - Found {candidateLogs.Count} candidate logs between {inTime.AddMinutes(1):yyyy-MM-dd HH:mm:ss} and {searchLimit:yyyy-MM-dd HH:mm:ss}");

                        var potentialOut = candidateLogs.LastOrDefault();

                        if (potentialOut != null)
                        {
                            Console.WriteLine(
                                $"  - Using potentialOut={potentialOut.LogTime:yyyy-MM-dd HH:mm:ss} as OutTime");
                            att.OutTime = potentialOut.LogTime;
                            if (nextAtt != null && nextAtt.InTime.HasValue)
                            {
                                var diff = Math.Abs((nextAtt.InTime.Value - potentialOut.LogTime).TotalMinutes);
                                if (diff < 5)
                                {
                                    Console.WriteLine(
                                        $"  - Duplicate punch detected (diff={diff:F2} min), clearing next day InTime");
                                    nextAtt.InTime = null;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"  - No potential OutTime found");
                        }
                    }
                }
/*
// 24-hour overlap logic: Check for cross-day punches
// If an employee punched in early morning (e.g., < 5 AM) on Day X,
// and Day X-1 has a missing OutTime, assume it belongs to Day X-1.
foreach (var att in attendances)
{
    if (att.InTime.HasValue && att.InTime.Value.Hour < 5)
    {
        var prevDate = att.Date.Date.AddDays(-1);
        var prevAtt = attendances.FirstOrDefault(a => a.Date.Date == prevDate);

        // If previous day exists, has InTime, but NO OutTime
        if (prevAtt != null && prevAtt.InTime.HasValue && !prevAtt.OutTime.HasValue)
        {
            // Move current InTime to previous OutTime
            prevAtt.OutTime = att.InTime;

            // Mark current InTime as consumed (null) so it doesn't show up as a start for today
            // Note: This modifies the object in memory only, not DB
            att.InTime = null;

            // Adjust status if needed?
            // If current day has no other punches, it might become Absent or Leave depending on logic
            // For now, we leave status as is, but InTime will be null/empty.
        }
    }
}
*/

// Generate all dates in range
                var allDates = new List<DateTime>();
                for (var date = from; date <= to; date = date.AddDays(1))
                {
                    allDates.Add(date);
                }

// Create job card records
                var jobCardRecords = new List<JobCardDto>();
                int presentDays = 0, absentDays = 0, weekendDays = 0, holidayDays = 0;
                decimal totalOt = 0;
                int totalLate = 0, totalEarly = 0;

                foreach (var date in allDates)
                {
                    var attendance = attendances.FirstOrDefault(a => a.Date.Date == date.Date);
                    
                    // Fallback logic: 1. Direct match for date, 2. Last submitted roster before date
                    var employeeRosters = roster.Where(r => r.Date.Date <= date.Date).ToList();
                    var dayRoster = employeeRosters.FirstOrDefault(r => r.Date.Date == date.Date) 
                                 ?? employeeRosters.FirstOrDefault(); // roster is already ordered DESC

                    var dayName = date.ToString("ddd");

                    // Determine if weekend (Friday/Saturday or from Roster)
                    bool isWeekend = dayRoster?.IsOffDay ?? (dayName == "Fri"); 
                    // Priority: 1. Attendance Record Shift (if processed), 2. Roster (Direct or Fallback), 3. Default Employee Shift
                    string? dailyShift = (attendance != null && attendance.Shift != null) 
                        ? attendance.Shift.NameEn 
                        : (dayRoster?.Shift?.NameEn ?? employee.Shift?.NameEn);
                    
                    int? dailyShiftId = (attendance != null && attendance.ShiftId.HasValue)
                        ? attendance.ShiftId
                        : (dayRoster?.ShiftId ?? employee.ShiftId);

                    bool dailyIsOffDay = (attendance != null && attendance.IsOffDay)
                        ? true
                        : (dayRoster?.IsOffDay ?? (dayName == "Fri"));
                    

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

                        // Calculate total hours based on actual In/Out times if available
                        decimal totalHours = 0;
                        decimal otHours = attendance.OTHours;

                        if (employee.IsOtEnabled && attendance.InTime.HasValue && attendance.OutTime.HasValue)
                        {
                            var duration = attendance.OutTime.Value - attendance.InTime.Value;
                            totalHours = (decimal)duration.TotalHours;

                            // Recalculate OT using the 45-minute rounding rule
                            // Total Hours = 9 (standard) + OT
                            decimal rawOt = totalHours - 9;
                            if (rawOt > 0)
                            {
                                int otMinutes = (int)((rawOt - (int)rawOt) * 60);
                                otHours = (int)rawOt + (otMinutes >= 45 ? 1 : 0);
                            }
                            else
                            {
                                otHours = 0;
                            }
                        }
                        else if (employee.IsOtEnabled && (attendance.Status == "Present" || attendance.Status == "Late"))
                        {
                            // Fallback to existing logic if no Punches but Status is Present
                            otHours = attendance.OTHours; 
                            totalHours = 9 + otHours;
                        }
                        else
                        {
                            otHours = 0;
                            totalHours = (attendance.Status == "Present" || attendance.Status == "Late") ? 9 : 0;
                        }

                        record = new JobCardDto
                        {
                            Date = date.ToString("dd MMM"),
                            Day = dayName,
                            Status = attendance.Status,
                            InTime = attendance.InTime?.ToString("HH:mm") ?? "-",
                            OutTime = attendance.OutTime?.ToString("HH:mm") ?? "-",
                            LateMinutes = lateMinutes,
                            EarlyMinutes = 0,
                            OTHours = otHours, // Rounding handled above
                            TotalHours = Math.Round(totalHours, 2),
                            Shift = dailyShift,
                            ShiftId = dailyShiftId,
                            IsOffDay = dailyIsOffDay,
                            Remarks = attendance.Status == "Absent" ? "Uninformed" : ""
                        };

                        // Update counters
                        if (attendance.Status == "Present" || attendance.Status == "Late") presentDays++;
                        else if (attendance.Status == "Absent") absentDays++;
                        else if (attendance.Status == "Off Day" || attendance.IsOffDay) weekendDays++;
                        else if (attendance.Status == "Holiday") holidayDays++;

                        totalOt += otHours;
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
                            Shift = dailyShift,
                            ShiftId = dailyShiftId,
                            IsOffDay = true,
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
                        EmployeeCard = employee.Id,
                        EmployeeId = employee.EmployeeId,
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
                        TotalOTHours = totalOt,
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
                return StatusCode(500,
                    new { message = "An error occurred while fetching the job card.", error = ex.Message });
            }
        }

// POST: api/attendance/manual-entry
        [HttpPost("manual-entry")]
        public async Task<ActionResult<ManualAttendanceResponseDto>> CreateManualEntry(
            [FromBody] ManualAttendanceDto dto)
        {
            try
            {
// Get current user
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

// Validate employee exists using business key
                var employee = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.EmployeeId == dto.EmployeeId && e.CompanyId == dto.CompanyId);

                if (employee == null)
                    return NotFound(new { message = "Employee not found" });

                int dbEmployeeId = employee.Id;

// Check if attendance already exists for this date
                var existing = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeCard == dbEmployeeId && a.Date.Date == dto.Date.Date);

                if (existing != null)
                {
                    // Update existing record
                    existing.InTime = dto.InTime;
                    existing.OutTime = dto.OutTime;
                    existing.Status = dto.Status;
                    existing.Reason = dto.Reason;
                    existing.Remarks = dto.Remarks;
                    existing.IsManual = true;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = userName;

                    // Add manual log entries if In/Out times are provided
                    if (dto.InTime.HasValue)
                    {
                        _context.AttendanceLogs.Add(new AttendanceLog
                        {
                            EmployeeCard = dbEmployeeId,
                            EmployeeId = employee.EmployeeId,
                            CompanyId = employee.CompanyId,
                            LogTime = dto.InTime.Value,
                            DeviceId = "Manual",
                            VerificationMode = "Manual",
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    if (dto.OutTime.HasValue)
                    {
                        _context.AttendanceLogs.Add(new AttendanceLog
                        {
                            EmployeeCard = dbEmployeeId,
                            EmployeeId = employee.EmployeeId,
                            CompanyId = employee.CompanyId,
                            LogTime = dto.OutTime.Value,
                            DeviceId = "Manual",
                            VerificationMode = "Manual",
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    await _context.SaveChangesAsync();

                    return Ok(new ManualAttendanceResponseDto
                    {
                        Id = existing.Id,
                        EmployeeId = employee.EmployeeId,
                        CompanyId = employee.CompanyId ?? 0,
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
                        EmployeeCard = dbEmployeeId,
                        EmployeeId = employee.EmployeeId,
                        CompanyId = employee.CompanyId,
                        Date = dto.Date.Date,
                        InTime = dto.InTime,
                        OutTime = dto.OutTime,
                        Status = dto.Status,
                        OTHours = 0, // Can be calculated based on shift
                        IsManual = true,
                        Reason = dto.Reason,
                        Remarks = dto.Remarks,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userName
                    };

                    _context.Attendances.Add(attendance);

                    // Add manual log entries if In/Out times are provided
                    if (dto.InTime.HasValue)
                    {
                        _context.AttendanceLogs.Add(new AttendanceLog
                        {
                            EmployeeCard = dbEmployeeId,
                            EmployeeId = employee.EmployeeId,
                            CompanyId = employee.CompanyId,
                            LogTime = dto.InTime.Value,
                            DeviceId = "Manual",
                            VerificationMode = "Manual",
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    if (dto.OutTime.HasValue)
                    {
                        _context.AttendanceLogs.Add(new AttendanceLog
                        {
                            EmployeeCard = dbEmployeeId,
                            EmployeeId = employee.EmployeeId,
                            CompanyId = employee.CompanyId,
                            LogTime = dto.OutTime.Value,
                            DeviceId = "Manual",
                            VerificationMode = "Manual",
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    await _context.SaveChangesAsync();

                    return Ok(new ManualAttendanceResponseDto
                    {
                        Id = attendance.Id,
                        EmployeeId = employee.EmployeeId,
                        CompanyId = employee.CompanyId ?? 0,
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
                return StatusCode(500,
                    new { message = "An error occurred while creating manual entry.", error = ex.Message });
            }
        }

// POST: api/attendance/manual-entry/bulk
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulkManualEntry([FromBody] List<ManualAttendanceDto> dtos)
        {
            try
            {
                if (dtos == null || !dtos.Any())
                    return BadRequest("No attendance data provided");

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

                var employeeIds = dtos.Select(d => d.EmployeeId).Distinct().ToList();
                var companyId = dtos.FirstOrDefault()?.CompanyId ?? 0;

                var employees = await _context.Employees
                    .Where(e => employeeIds.Contains(e.EmployeeId) && e.CompanyId == companyId)
                    .ToDictionaryAsync(e => e.EmployeeId);

                var dbEmployeeIds = employees.Values.Select(e => e.Id).ToList();
                var dates = dtos.Select(d => d.Date.Date).Distinct().ToList();
                var existingAttendances = await _context.Attendances
                    .Where(a => dbEmployeeIds.Contains(a.EmployeeCard) && dates.Contains(a.Date.Date))
                    .ToListAsync();

                var newAttendances = new List<Attendance>();
                int updatedCount = 0;

                foreach (var dto in dtos)
                {
                    if (!employees.TryGetValue(dto.EmployeeId, out var employee)) continue;

                    var existing = existingAttendances
                        .FirstOrDefault(a => a.EmployeeCard == employee.Id && a.Date.Date == dto.Date.Date);

                    if (existing != null)
                    {
                        existing.InTime = dto.InTime;
                        existing.OutTime = dto.OutTime;
                        existing.Status = dto.Status;
                        existing.Reason = dto.Reason;
                        existing.Remarks = dto.Remarks;
                        existing.IsManual = true;
                        existing.UpdatedAt = DateTime.UtcNow;
                        existing.UpdatedBy = userName;
                        updatedCount++;

                        // Add manual log entries if In/Out times are provided
                        if (dto.InTime.HasValue)
                        {
                            _context.AttendanceLogs.Add(new AttendanceLog
                            {
                                EmployeeCard = employee.Id,
                                EmployeeId = employee.EmployeeId,
                                CompanyId = employee.CompanyId,
                                LogTime = dto.InTime.Value,
                                DeviceId = "Manual",
                                VerificationMode = "Manual",
                                CreatedAt = DateTime.UtcNow
                            });
                        }

                        if (dto.OutTime.HasValue)
                        {
                            _context.AttendanceLogs.Add(new AttendanceLog
                            {
                                EmployeeCard = employee.Id,
                                EmployeeId = employee.EmployeeId,
                                CompanyId = employee.CompanyId,
                                LogTime = dto.OutTime.Value,
                                DeviceId = "Manual",
                                VerificationMode = "Manual",
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                    else
                    {
                        var attendance = new Attendance
                        {
                            EmployeeCard = employee.Id,
                            EmployeeId = employee.EmployeeId,
                            CompanyId = employee.CompanyId,
                            Date = dto.Date.Date,
                            InTime = dto.InTime,
                            OutTime = dto.OutTime,
                            Status = dto.Status,
                            OTHours = 0, // Calculate if needed
                            IsManual = true,
                            Reason = dto.Reason,
                            Remarks = dto.Remarks,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userName
                        };
                        newAttendances.Add(attendance);

                        // Add manual log entries if In/Out times are provided
                        if (dto.InTime.HasValue)
                        {
                            _context.AttendanceLogs.Add(new AttendanceLog
                            {
                                EmployeeCard = employee.Id,
                                EmployeeId = employee.EmployeeId,
                                CompanyId = employee.CompanyId,
                                LogTime = dto.InTime.Value,
                                DeviceId = "Manual",
                                VerificationMode = "Manual",
                                CreatedAt = DateTime.UtcNow
                            });
                        }

                        if (dto.OutTime.HasValue)
                        {
                            _context.AttendanceLogs.Add(new AttendanceLog
                            {
                                EmployeeCard = employee.Id,
                                EmployeeId = employee.EmployeeId,
                                CompanyId = employee.CompanyId,
                                LogTime = dto.OutTime.Value,
                                DeviceId = "Manual",
                                VerificationMode = "Manual",
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }


                if (newAttendances.Any())
                    _context.Attendances.AddRange(newAttendances);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message =
                        $"Processed {dtos.Count} entries. Created: {newAttendances.Count}, Updated: {updatedCount}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "An error occurred while processing bulk entries.", error = ex.Message });
            }
        }

// GET: api/attendance/manual-entry/history
        [HttpGet("manual-entry/history")]
        public async Task<ActionResult<IEnumerable<ManualAttendanceHistoryDto>>> GetManualEntryHistory(
            [FromQuery] string? employeeId,
            [FromQuery] int? companyId,
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

                if (!string.IsNullOrEmpty(employeeId))
                {
                    if (companyId.HasValue)
                    {
                        query = query.Where(a =>
                            a.Employee!.EmployeeId == employeeId && a.CompanyId == companyId.Value);
                    }
                    else
                    {
                        query = query.Where(a => a.Employee!.EmployeeId == employeeId);
                    }
                }
                else if (companyId.HasValue)
                {
                    query = query.Where(a => a.CompanyId == companyId.Value);
                }

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
                        EmployeeId = a.Employee!.EmployeeId,
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
                return StatusCode(500,
                    new { message = "An error occurred while fetching manual entry history.", error = ex.Message });
            }
        }

// GET: api/attendance/missing-entries
        [HttpGet("missing-entries")]
        public async Task<ActionResult<MissingEntryResponseDto>> GetMissingEntries(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] int? companyId,
            [FromQuery] int? departmentId,
            [FromQuery] int? designationId,
            [FromQuery] int? sectionId,
            [FromQuery] string? searchTerm)
        {
            try
            {
// Start with filtered employees
                var employeesQuery = _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Designation)
                    .Include(e => e.Shift)
                    .Where(e => e.IsActive);

// Use the same CommonFilterDto properties for employee filtering
                if (companyId.HasValue)
                    employeesQuery = employeesQuery.Where(e => e.CompanyId == companyId.Value);
                if (departmentId.HasValue)
                    if (departmentId.HasValue)
                        employeesQuery = employeesQuery.Where(e => e.DepartmentId == departmentId.Value);
                if (designationId.HasValue)
                    employeesQuery = employeesQuery.Where(e => e.DesignationId == designationId.Value);
                if (sectionId.HasValue) employeesQuery = employeesQuery.Where(e => e.SectionId == sectionId.Value);
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    employeesQuery = employeesQuery.Where(e =>
                        e.EmployeeId.Contains(searchTerm) ||
                        e.FullNameEn.Contains(searchTerm));
                }

                var employees = await employeesQuery.ToListAsync();
                var employeeIds = employees.Select(e => e.Id).ToList();

// Get attendance for these employees in the range
                var attendances = await _context.Attendances
                    .Where(a => employeeIds.Contains(a.EmployeeCard) &&
                                a.Date.Date >= fromDate.Date &&
                                a.Date.Date <= toDate.Date)
                    .ToListAsync();

                var missingEntries = new List<MissingEntryDto>();
                int idCounter = 1;

                foreach (var employee in employees)
                {
                    for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
                    {
                        // Check if an attendance record exists for this employee/date
                        var attendance = attendances.FirstOrDefault(a =>
                            a.EmployeeCard == employee.Id && a.Date.Date == date.Date);

                        bool isMissingIn = false;
                        bool isMissingOut = false;
                        string missingType = "";
                        string entryStatus = "Pending";

                        if (attendance != null)
                        {
                            isMissingIn = !attendance.InTime.HasValue;
                            isMissingOut = !attendance.OutTime.HasValue;

                            // Only include if EXACTLY ONE punch is missing
                            if (isMissingIn ^ isMissingOut)
                            {
                                missingType = isMissingIn ? "In Time" : "Out Time";
                                entryStatus = "Pending";
                            }
                        }
                        // We no longer handle the 'else' (attendance == null) case
                        // because the user wants to exclude "Both (No Punch)" cases.

                        if (!string.IsNullOrEmpty(missingType))
                        {
                            missingEntries.Add(new MissingEntryDto
                            {
                                Id = idCounter++,
                                EmployeeCard = employee.Id,
                                EmployeeId = employee.EmployeeId,
                                EmployeeName = employee.FullNameEn,
                                CompanyId = employee.CompanyId,
                                Department = employee.Department?.NameEn ?? "N/A",
                                Designation = employee.Designation?.NameEn ?? "N/A",
                                Shift = employee.Shift?.NameEn,
                                Date = date,
                                InTime = attendance?.InTime,
                                OutTime = attendance?.OutTime,
                                MissingType = missingType,
                                Status = entryStatus
                            });
                        }
                    }
                }

// Calculate summary from the final list
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
                return StatusCode(500,
                    new { message = "An error occurred while fetching missing entries.", error = ex.Message });
            }
        }

// GET: api/attendance/absenteeism-records
        [HttpGet("absenteeism-records")]
        public async Task<ActionResult<AbsenteeismResponseDto>> GetAbsenteeismRecords(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] int? companyId,
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

                if (companyId.HasValue)
                    query = query.Where(a => a.CompanyId == companyId.Value);

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
                    .GroupBy(a => a.EmployeeCard)
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
                                EmployeeCard = attendance.EmployeeCard,
                                EmployeeId = attendance.Employee!.EmployeeId,
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
                return StatusCode(500,
                    new { message = "An error occurred while fetching absenteeism records.", error = ex.Message });
            }
        }

// GET: api/attendance/daily-ot-sheet
        [HttpGet("daily-ot-sheet")]
        public async Task<ActionResult<OTSheetResponseDto>> GetDailyOtSheet([FromQuery] CommonFilterDto filters)
        {
            try
            {
                var date = filters.Date ?? DateTime.Today;
                var query = _context.Attendances
                    .Include(a => a.Employee)
                    .ThenInclude(e => e!.Department)
                    .Include(a => a.Employee)
                    .ThenInclude(e => e!.Designation)
                    .Where(a => a.Date.Date == date.Date)
                    .Where(a => a.OTHours > 0);

                query = ApplyAttendanceFilters(query, filters);

                var records = await query
                    .Select(a => new DailyOTSheetDto
                    {
                        Id = a.Id,
                        EmployeeCard = a.EmployeeCard,
                        EmployeeId = a.Employee!.EmployeeId,
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
                return StatusCode(500,
                    new { message = "An error occurred while fetching OT sheet.", error = ex.Message });
            }
        }

// GET: api/attendance/daily-ot-summary
        [HttpGet("daily-ot-summary")]
        public async Task<ActionResult<OTSummaryResponseDto>> GetDailyOtSummary([FromQuery] CommonFilterDto filters)
        {
            try
            {
                var date = filters.Date ?? DateTime.Today;
                var query = _context.Attendances
                    .Include(a => a.Employee)
                    .ThenInclude(e => e!.Department)
                    .Where(a => a.Date.Date == date.Date)
                    .Where(a => a.OTHours > 0);

                query = ApplyAttendanceFilters(query, filters);
                var records = await query.ToListAsync();

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
                return StatusCode(500,
                    new { message = "An error occurred while fetching OT summary.", error = ex.Message });
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
                var inTime = status == "Absent" || status == "On Leave"
                    ? (DateTime?)null
                    : DateTime.Today.AddHours(9); // 09:00 AM
                var outTime = status == "Absent" || status == "On Leave"
                    ? (DateTime?)null
                    : DateTime.Today.AddHours(18); // 06:00 PM

                _context.Attendances.Add(new Attendance
                {
                    EmployeeCard = emp.Id,
                    Date = date.Date,
                    InTime = inTime,
                    OutTime = outTime,
                    Status = status,
                    OTHours = (status == "Present" && emp.IsOtEnabled) ? (decimal)(random.NextDouble() * 2) : 0,
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
                var empIds = employees.Select(e => e.Id).ToList();

                // Pre-fetch ALL rosters for these employees up to endDate for fallback logic
                var allRosters = await _context.EmployeeShiftRosters
                    .Where(r => empIds.Contains(r.EmployeeId) && r.Date.Date <= endDate)
                    .OrderByDescending(r => r.Date)
                    .ToListAsync();

                int processedCount = 0;

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    // Pre-filter rosters for the current date or earlier
                    var relevantRosters = allRosters.Where(r => r.Date.Date <= date.Date).ToList();


                    var leaves = await _context.LeaveApplications
                        .Where(l => l.Status == "Approved" && l.StartDate.Date <= date.Date &&
                                    l.EndDate.Date >= date.Date)
                        .ToListAsync();

                    var existingAttendances = await _context.Attendances
                        .Where(a => a.Date.Date == date.Date)
                        .ToListAsync();

                    foreach (var emp in employees)
                    {
                        // Fallback logic: 1. Direct match for date, 2. Last submitted roster before date
                        var employeeRosters = relevantRosters.Where(r => r.EmployeeId == emp.Id).ToList();
                        var roster = employeeRosters.FirstOrDefault(r => r.Date.Date == date.Date) 
                                     ?? employeeRosters.OrderByDescending(r => r.Date).FirstOrDefault();

                        var leave = leaves.FirstOrDefault(l => l.EmployeeId == emp.Id);
                        var attendance = existingAttendances.FirstOrDefault(a => a.EmployeeCard == emp.Id);

                        // If manual attendance exists, skip processing for this employee/date
                        if (attendance != null && attendance.IsManual)
                        {
                            processedCount++;
                            continue;
                        }

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
                        else if (attendance != null && (attendance.InTime.HasValue ||
                                                        attendance.OutTime.HasValue))
                        {
                            // Re-classify In/Out times based on user rules (cutoff at 11:00 AM)
                            var rawIn = attendance.InTime;
                            var rawOut = attendance.OutTime;

                            TimeSpan? actualIn = null;
                            TimeSpan? actualOut = null;

                            var cutoff = new TimeSpan(11, 0, 0); // 11:00 AM

                            // Collect all unique non-null times
                            var allTimes = new List<TimeSpan>();
                            if (rawIn.HasValue) allTimes.Add(rawIn.Value.TimeOfDay);
                            if (rawOut.HasValue && (!rawIn.HasValue || rawIn.Value != rawOut.Value))
                                allTimes.Add(rawOut.Value.TimeOfDay);

                            foreach (var t in allTimes)
                            {
                                if (t <= cutoff)
                                {
                                    if (actualIn == null || t < actualIn) actualIn = t; // Earlier punch is In
                                }
                                else
                                {
                                    if (actualOut == null || t > actualOut) actualOut = t; // Later punch is Out
                                }
                            }

                            attendance.InTime = actualIn.HasValue ? date.Date.Add(actualIn.Value) : (DateTime?)null;
                            attendance.OutTime = actualOut.HasValue ? date.Date.Add(actualOut.Value) : (DateTime?)null;

                            if (attendance.InTime.HasValue)
                            {
                                // Calculate Late
                                var inTime = attendance.InTime.Value.TimeOfDay;
                                var shiftInTime = ParseTime(shift.InTime);
                                var lateLimit = ParseTime(shift.LateInTime) ??
                                                shiftInTime?.Add(TimeSpan.FromMinutes(15));

                                if (inTime > lateLimit)
                                {
                                    status = "Late";
                                }
                                else
                                {
                                    status = "Present";
                                }
                            }
                            else if (attendance.OutTime.HasValue)
                            {
                                status = "Present (Out Only)";
                            }

                            // Calculate OT
                            if (emp.IsOtEnabled && attendance.OutTime.HasValue &&
                                !string.IsNullOrEmpty(shift.OutTime))
                            {
                                var outTime = attendance.OutTime.Value.TimeOfDay;
                                var shiftOutTime = ParseTime(shift.OutTime);

                                if (outTime > shiftOutTime)
                                {
                                    var diff = (outTime - shiftOutTime.Value).TotalHours;
                                    otHours = (decimal)Math.Max(0, Math.Floor(diff));
                                }
                            }
                        }

                        // Update or Create Attendance Record
                        if (attendance != null)
                        {
                            attendance.Status = status;
                            attendance.OTHours = otHours;
                            attendance.ShiftId = shiftId;
                            attendance.IsOffDay = status == "Off Day";
                            attendance.UpdatedAt = DateTime.UtcNow;
                            attendance.UpdatedBy = userName;
                        }
                        else
                        {
                            _context.Attendances.Add(new Attendance
                            {
                                EmployeeCard = emp.Id,
                                Date = date,
                                Status = status,
                                OTHours = otHours,
                                ShiftId = shiftId,
                                IsOffDay = status == "Off Day",
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = userName
                            });
                        }

                        processedCount++;
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new
                {
                    message =
                        $"Successfully processed {processedCount} records from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error during processing: " + ex.Message });
            }
        }

        [HttpPost("bulk-manual-entry")]
        public async Task<ActionResult> BulkManualEntry([FromBody] BulkManualAttendanceDto dto)
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
                var updatedCount = 0;
                var createdCount = 0;

                var employees = await _context.Employees
                    .Include(e => e.Shift)
                    .Where(e => dto.EmployeeIds.Contains(e.EmployeeId) &&
                                (!dto.CompanyId.HasValue || e.CompanyId == dto.CompanyId.Value))
                    .ToDictionaryAsync(e => e.EmployeeId);

                foreach (var empId in dto.EmployeeIds)
                {
                    if (!employees.TryGetValue(empId, out var employee)) continue;

                    var existing = await _context.Attendances
                        .FirstOrDefaultAsync(a => a.EmployeeCard == employee.Id && a.Date.Date == dto.Date.Date);

                    decimal calculateOt = 0;
                    if (employee.IsOtEnabled && employee.Shift != null && dto.OutTime.HasValue &&
                        !string.IsNullOrEmpty(employee.Shift.OutTime) &&
                        TimeSpan.TryParse(employee.Shift.OutTime, out var officeOutTimeValue))
                    {
                        var officeOutDateTime = dto.Date.Date.Add(officeOutTimeValue);

                        // Handle overnight shift: If Office Out < Actual In, Office Out is next day
                        if (employee.Shift.ActualInTime != null &&
                            TimeSpan.TryParse(employee.Shift.ActualInTime, out var aIn) &&
                            officeOutTimeValue < aIn)
                        {
                            officeOutDateTime = dto.Date.Date.AddDays(1).Add(officeOutTimeValue);
                        }

                        if (dto.OutTime.Value > officeOutDateTime)
                        {
                            var otDuration = dto.OutTime.Value - officeOutDateTime;
                            double totalMinutes = otDuration.TotalMinutes;

                            int fullHours = (int)(totalMinutes / 60);
                            int remainingMinutes = (int)(totalMinutes % 60);

                            if (remainingMinutes >= 45) calculateOt = fullHours + 1;
                            else calculateOt = fullHours;
                        }
                    }

                    if (existing != null)
                    {
                        existing.InTime = dto.InTime;
                        existing.OutTime = dto.OutTime;
                        existing.Status = dto.Status;
                        existing.Reason = dto.Reason;
                        existing.OTHours = calculateOt;
                        existing.IsManual = true;
                        existing.UpdatedAt = DateTime.UtcNow;
                        existing.UpdatedBy = userName;
                        updatedCount++;

                        // Add manual log entries if In/Out times are provided
                        if (dto.InTime.HasValue)
                        {
                            _context.AttendanceLogs.Add(new AttendanceLog
                            {
                                EmployeeCard = employee.Id,
                                EmployeeId = employee.EmployeeId,
                                CompanyId = employee.CompanyId,
                                LogTime = dto.InTime.Value,
                                DeviceId = "Manual",
                                VerificationMode = "Manual",
                                CreatedAt = DateTime.UtcNow
                            });
                        }

                        if (dto.OutTime.HasValue)
                        {
                            _context.AttendanceLogs.Add(new AttendanceLog
                            {
                                EmployeeCard = employee.Id,
                                EmployeeId = employee.EmployeeId,
                                CompanyId = employee.CompanyId,
                                LogTime = dto.OutTime.Value,
                                DeviceId = "Manual",
                                VerificationMode = "Manual",
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                    else
                    {
                        var attendance = new Attendance
                        {
                            EmployeeCard = employee.Id,
                            EmployeeId = employee.EmployeeId,
                            CompanyId = employee.CompanyId,
                            Date = dto.Date.Date,
                            InTime = dto.InTime,
                            OutTime = dto.OutTime,
                            Status = dto.Status,
                            Reason = dto.Reason,
                            OTHours = calculateOt,
                            IsManual = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userName
                        };
                        _context.Attendances.Add(attendance);

                        // Add manual log entries if In/Out times are provided
                        if (dto.InTime.HasValue)
                        {
                            _context.AttendanceLogs.Add(new AttendanceLog
                            {
                                EmployeeCard = employee.Id,
                                EmployeeId = employee.EmployeeId,
                                CompanyId = employee.CompanyId,
                                LogTime = dto.InTime.Value,
                                DeviceId = "Manual",
                                VerificationMode = "Manual",
                                CreatedAt = DateTime.UtcNow
                            });
                        }

                        if (dto.OutTime.HasValue)
                        {
                            _context.AttendanceLogs.Add(new AttendanceLog
                            {
                                EmployeeCard = employee.Id,
                                EmployeeId = employee.EmployeeId,
                                CompanyId = employee.CompanyId,
                                LogTime = dto.OutTime.Value,
                                DeviceId = "Manual",
                                VerificationMode = "Manual",
                                CreatedAt = DateTime.UtcNow
                            });
                        }

                        createdCount++;
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new
                {
                    message =
                        $"Successfully processed {updatedCount + createdCount} records. (Updated: {updatedCount}, Created: {createdCount})"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error during bulk manual entry: " + ex.Message });
            }
        }

        [HttpPost("delete-attendance")]
        public async Task<ActionResult> DeleteAttendance([FromBody] DeleteAttendanceDto dto)
        {
            try
            {
                var query = _context.Attendances.AsQueryable();

                query = query.Where(a => a.Date.Date >= dto.FromDate.Date && a.Date.Date <= dto.ToDate.Date);

                if (dto.EmployeeIds != null && dto.EmployeeIds.Any())
                {
                    var dbEmployeeIds = await _context.Employees
                        .Where(e => dto.EmployeeIds.Contains(e.EmployeeId) &&
                                    (!dto.CompanyId.HasValue || e.CompanyId == dto.CompanyId.Value))
                        .Select(e => e.Id)
                        .ToListAsync();

                    query = query.Where(a => dbEmployeeIds.Contains(a.EmployeeCard));
                }
                else if (dto.CompanyId.HasValue)
                {
                    query = query.Where(a => a.CompanyId == dto.CompanyId.Value);
                }

                if (dto.DepartmentId.HasValue)
                {
                    query = query.Where(a => a.Employee!.DepartmentId == dto.DepartmentId.Value);
                }

                if (dto.SectionId.HasValue)
                {
                    query = query.Where(a => a.Employee!.SectionId == dto.SectionId.Value);
                }

                var recordsToDelete = await query.ToListAsync();
                _context.Attendances.RemoveRange(recordsToDelete);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Successfully deleted {recordsToDelete.Count} attendance records." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error during attendance deletion: " + ex.Message });
            }
        }

// DELETE: api/attendance/delete-all
        [HttpDelete("delete-all")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> DeleteAllAttendanceData()
        {
            try
            {
// Execute truncate or delete commands
// Using ExecuteSqlRaw for efficiency and to reset identity seeds if possible (though Truncate requires permissions)
// Fallback to RemoveRange if Truncate is risky for FKs, but user asked to delete ALL data from these tables.
// Since AttendanceLogs and Attendances have FKs to Employee/Company but are leaf nodes mostly,
// we should be careful.
// EF Core way is safer:

                var logs = await _context.AttendanceLogs.ToListAsync();
                _context.AttendanceLogs.RemoveRange(logs);

                var attendances = await _context.Attendances.ToListAsync();
                _context.Attendances.RemoveRange(attendances);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Successfully deleted {logs.Count} logs and {attendances.Count} attendance records."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "An error occurred while deleting all attendance data.", error = ex.Message });
            }
        }

        private bool IsWeekend(DateTime date, string? weekends)
        {
            if (string.IsNullOrEmpty(weekends)) return date.DayOfWeek == DayOfWeek.Friday;
            var dayName = date.DayOfWeek.ToString();
            return weekends.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Any(w => w.Trim().Equals(dayName, StringComparison.OrdinalIgnoreCase));
        }

        private TimeSpan? ParseTime(string? timeStr)
        {
            if (string.IsNullOrWhiteSpace(timeStr)) return null;
            if (DateTime.TryParse(timeStr, out DateTime dt)) return dt.TimeOfDay;
            return null;
        }

        private IQueryable<Attendance> ApplyAttendanceFilters(IQueryable<Attendance> query, CommonFilterDto filters)
        {
            if (filters.Date.HasValue)
                query = query.Where(a => a.Date.Date == filters.Date.Value.Date);

            if (filters.StartDate.HasValue)
                query = query.Where(a => a.Date.Date >= filters.StartDate.Value.Date);

            if (filters.EndDate.HasValue)
                query = query.Where(a => a.Date.Date <= filters.EndDate.Value.Date);

            if (filters.CompanyId.HasValue && filters.CompanyId > 0)
                query = query.Where(a =>
                    a.CompanyId == filters.CompanyId.Value ||
                    (a.Employee != null && a.Employee.CompanyId == filters.CompanyId.Value));

            if (!string.IsNullOrEmpty(filters.CompanyName))
                query = query.Where(a => a.Employee != null && a.Employee.CompanyName == filters.CompanyName);

            if (filters.DepartmentId.HasValue)
                query = query.Where(a => a.Employee != null && a.Employee.DepartmentId == filters.DepartmentId.Value);

            if (filters.SectionId.HasValue)
                query = query.Where(a => a.Employee != null && a.Employee.SectionId == filters.SectionId.Value);

            if (filters.DesignationId.HasValue)
                query = query.Where(a => a.Employee != null && a.Employee.DesignationId == filters.DesignationId.Value);

            if (filters.LineId.HasValue)
                query = query.Where(a => a.Employee != null && a.Employee.LineId == filters.LineId.Value);

            if (filters.GroupId.HasValue)
                query = query.Where(a => a.Employee != null && a.Employee.GroupId == filters.GroupId.Value);

            if (filters.ShiftId.HasValue)
                query = query.Where(a => a.Employee != null && a.Employee.ShiftId == filters.ShiftId.Value);

            if (filters.FloorId.HasValue)
                query = query.Where(a => a.Employee != null && a.Employee.FloorId == filters.FloorId.Value);

            if (!string.IsNullOrEmpty(filters.Gender))
                query = query.Where(a => a.Employee != null && a.Employee.Gender == filters.Gender);

            if (!string.IsNullOrEmpty(filters.Religion))
                query = query.Where(a => a.Employee != null && a.Employee.Religion == filters.Religion);

            if (!string.IsNullOrEmpty(filters.Status) && filters.Status != "all")
                query = query.Where(a => a.Status == filters.Status);

            if (!string.IsNullOrEmpty(filters.EmployeeId))
                query = query.Where(a => a.Employee != null && a.Employee.EmployeeId == filters.EmployeeId);

            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                query = query.Where(a => a.Employee != null && (a.Employee.FullNameEn.Contains(filters.SearchTerm) ||
                                                                a.Employee.EmployeeId == filters.SearchTerm));
            }

            return query;
        }

        private IQueryable<Employee> ApplyEmployeeFilters(IQueryable<Employee> query, CommonFilterDto filters)
        {
            if (filters.CompanyId.HasValue && filters.CompanyId > 0)
                query = query.Where(e => e.CompanyId == filters.CompanyId.Value);

            if (!string.IsNullOrEmpty(filters.CompanyName))
                query = query.Where(e => e.CompanyName == filters.CompanyName);

            if (filters.DepartmentId.HasValue)
                query = query.Where(e => e.DepartmentId == filters.DepartmentId.Value);

            if (filters.SectionId.HasValue)
                query = query.Where(e => e.SectionId == filters.SectionId.Value);

            if (filters.DesignationId.HasValue)
                query = query.Where(e => e.DesignationId == filters.DesignationId.Value);

            if (filters.LineId.HasValue)
                query = query.Where(e => e.LineId == filters.LineId.Value);

            if (filters.GroupId.HasValue)
                query = query.Where(e => e.GroupId == filters.GroupId.Value);

            if (filters.ShiftId.HasValue)
                query = query.Where(e => e.ShiftId == filters.ShiftId.Value);

            if (filters.FloorId.HasValue)
                query = query.Where(e => e.FloorId == filters.FloorId.Value);

            if (filters.IsActive.HasValue)
                query = query.Where(e => e.IsActive == filters.IsActive.Value);

            if (!string.IsNullOrEmpty(filters.EmployeeId))
                query = query.Where(e => e.EmployeeId == filters.EmployeeId);

            if (!string.IsNullOrEmpty(filters.Gender))
                query = query.Where(e => e.Gender == filters.Gender);

            if (!string.IsNullOrEmpty(filters.Religion))
                query = query.Where(e => e.Religion == filters.Religion);

            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                query = query.Where(e => e.FullNameEn.Contains(filters.SearchTerm) ||
                                         e.EmployeeId == filters.SearchTerm);
            }

            return query;
        }
    }
}

