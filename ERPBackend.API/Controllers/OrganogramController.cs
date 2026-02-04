using ERPBackend.Core.Constants;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrganogramController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrganogramController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Departments ---
        [HttpGet("departments")]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments(int? companyId)
        {
            var query = _context.Departments.AsQueryable();
            if (companyId.HasValue)
            {
                query = query.Where(d => d.CompanyId == companyId);
            }

            return await query
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    NameEn = d.NameEn,
                    NameBn = d.NameBn,
                    CompanyId = d.CompanyId,
                    CompanyName = d.Company != null ? d.Company.CompanyNameEn : null
                })
                .ToListAsync();
        }

        [HttpPost("departments")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<ActionResult<DepartmentDto>> CreateDepartment(CreateDepartmentDto dto)
        {
            var dept = new Department { NameEn = dto.NameEn, NameBn = dto.NameBn, CompanyId = dto.CompanyId };
            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();
            return Ok(new DepartmentDto
                { Id = dept.Id, NameEn = dept.NameEn, NameBn = dept.NameBn, CompanyId = dept.CompanyId });
        }

        [HttpPut("departments/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> UpdateDepartment(int id, CreateDepartmentDto dto)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound();
            dept.NameEn = dto.NameEn;
            dept.NameBn = dto.NameBn;
            dept.CompanyId = dto.CompanyId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("departments/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound();
            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Sections ---
        [HttpGet("sections")]
        public async Task<ActionResult<IEnumerable<SectionDto>>> GetSections(int? departmentId)
        {
            var query = _context.Sections.AsQueryable();
            if (departmentId.HasValue)
            {
                query = query.Where(s => s.DepartmentId == departmentId);
            }

            return await query
                .Select(s => new SectionDto
                {
                    Id = s.Id,
                    NameEn = s.NameEn,
                    NameBn = s.NameBn,
                    DepartmentId = s.DepartmentId,
                    DepartmentName = s.Department != null ? s.Department.NameEn : null
                })
                .ToListAsync();
        }

        [HttpPost("sections")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<ActionResult<SectionDto>> CreateSection(CreateSectionDto dto)
        {
            var section = new Section { NameEn = dto.NameEn, NameBn = dto.NameBn, DepartmentId = dto.DepartmentId };
            _context.Sections.Add(section);
            await _context.SaveChangesAsync();
            return Ok(new SectionDto
            {
                Id = section.Id, NameEn = section.NameEn, NameBn = section.NameBn, DepartmentId = section.DepartmentId
            });
        }

        [HttpPut("sections/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> UpdateSection(int id, CreateSectionDto dto)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null) return NotFound();
            section.NameEn = dto.NameEn;
            section.NameBn = dto.NameBn;
            section.DepartmentId = dto.DepartmentId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("sections/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> DeleteSection(int id)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null) return NotFound();
            _context.Sections.Remove(section);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Designations ---
        [HttpGet("designations")]
        public async Task<ActionResult<IEnumerable<DesignationDto>>> GetDesignations(int? sectionId)
        {
            var query = _context.Designations.AsQueryable();
            if (sectionId.HasValue)
            {
                query = query.Where(d => d.SectionId == sectionId);
            }

            return await query
                .Select(d => new DesignationDto
                {
                    Id = d.Id,
                    NameEn = d.NameEn,
                    NameBn = d.NameBn,
                    NightBill = d.NightBill,
                    HolidayBill = d.HolidayBill,
                    AttendanceBonus = d.AttendanceBonus,
                    SectionId = d.SectionId,
                    SectionName = d.Section != null ? d.Section.NameEn : null
                })
                .ToListAsync();
        }

        [HttpPost("designations")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<ActionResult<DesignationDto>> CreateDesignation(CreateDesignationDto dto)
        {
            var designation = new Designation
            {
                NameEn = dto.NameEn,
                NameBn = dto.NameBn,
                NightBill = dto.NightBill,
                HolidayBill = dto.HolidayBill,
                AttendanceBonus = dto.AttendanceBonus,
                SectionId = dto.SectionId
            };
            _context.Designations.Add(designation);
            await _context.SaveChangesAsync();
            return Ok(new DesignationDto
            {
                Id = designation.Id,
                NameEn = designation.NameEn,
                NameBn = designation.NameBn,
                NightBill = designation.NightBill,
                HolidayBill = designation.HolidayBill,
                AttendanceBonus = designation.AttendanceBonus,
                SectionId = designation.SectionId
            });
        }

        [HttpPut("designations/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> UpdateDesignation(int id, CreateDesignationDto dto)
        {
            var designation = await _context.Designations.FindAsync(id);
            if (designation == null) return NotFound();
            designation.NameEn = dto.NameEn;
            designation.NameBn = dto.NameBn;
            designation.NightBill = dto.NightBill;
            designation.HolidayBill = dto.HolidayBill;
            designation.AttendanceBonus = dto.AttendanceBonus;
            designation.SectionId = dto.SectionId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("designations/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> DeleteDesignation(int id)
        {
            var designation = await _context.Designations.FindAsync(id);
            if (designation == null) return NotFound();
            _context.Designations.Remove(designation);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Lines ---
        [HttpGet("lines")]
        public async Task<ActionResult<IEnumerable<LineDto>>> GetLines(int? sectionId)
        {
            var query = _context.Lines.AsQueryable();
            if (sectionId.HasValue)
            {
                query = query.Where(l => l.SectionId == sectionId);
            }

            return await query
                .Select(l => new LineDto
                {
                    Id = l.Id,
                    NameEn = l.NameEn,
                    NameBn = l.NameBn,
                    SectionId = l.SectionId,
                    SectionName = l.Section != null ? l.Section.NameEn : null
                })
                .ToListAsync();
        }

        [HttpPost("lines")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<ActionResult<LineDto>> CreateLine(CreateLineDto dto)
        {
            var line = new Line { NameEn = dto.NameEn, NameBn = dto.NameBn, SectionId = dto.SectionId };
            _context.Lines.Add(line);
            await _context.SaveChangesAsync();
            return Ok(new LineDto
                { Id = line.Id, NameEn = line.NameEn, NameBn = line.NameBn, SectionId = line.SectionId });
        }

        [HttpPut("lines/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> UpdateLine(int id, CreateLineDto dto)
        {
            var line = await _context.Lines.FindAsync(id);
            if (line == null) return NotFound();
            line.NameEn = dto.NameEn;
            line.NameBn = dto.NameBn;
            line.SectionId = dto.SectionId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("lines/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> DeleteLine(int id)
        {
            var line = await _context.Lines.FindAsync(id);
            if (line == null) return NotFound();
            _context.Lines.Remove(line);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        // --- Groups ---
        [HttpGet("groups")]
        public async Task<ActionResult<IEnumerable<GroupDto>>> GetGroups()
        {
            return await _context.Groups
                .Select(g => new GroupDto
                {
                    Id = g.Id,
                    NameEn = g.NameEn,
                    NameBn = g.NameBn
                })
                .ToListAsync();
        }

        [HttpPost("groups")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<ActionResult<GroupDto>> CreateGroup(CreateGroupDto dto)
        {
            var group = new Group { NameEn = dto.NameEn, NameBn = dto.NameBn };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
            return Ok(new GroupDto { Id = group.Id, NameEn = group.NameEn, NameBn = group.NameBn });
        }

        [HttpPut("groups/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> UpdateGroup(int id, CreateGroupDto dto)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null) return NotFound();
            group.NameEn = dto.NameEn;
            group.NameBn = dto.NameBn;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("groups/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null) return NotFound();
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Floors ---
        [HttpGet("floors")]
        public async Task<ActionResult<IEnumerable<FloorDto>>> GetFloors()
        {
            return await _context.Floors
                .Select(f => new FloorDto
                {
                    Id = f.Id,
                    NameEn = f.NameEn,
                    NameBn = f.NameBn
                })
                .ToListAsync();
        }

        [HttpPost("floors")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<ActionResult<FloorDto>> CreateFloor(CreateFloorDto dto)
        {
            var floor = new Floor { NameEn = dto.NameEn, NameBn = dto.NameBn };
            _context.Floors.Add(floor);
            await _context.SaveChangesAsync();
            return Ok(new FloorDto { Id = floor.Id, NameEn = floor.NameEn, NameBn = floor.NameBn });
        }

        [HttpPut("floors/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> UpdateFloor(int id, CreateFloorDto dto)
        {
            var floor = await _context.Floors.FindAsync(id);
            if (floor == null) return NotFound();
            floor.NameEn = dto.NameEn;
            floor.NameBn = dto.NameBn;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("floors/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> DeleteFloor(int id)
        {
            var floor = await _context.Floors.FindAsync(id);
            if (floor == null) return NotFound();
            _context.Floors.Remove(floor);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Excel Import/Export ---
        [HttpGet("export-template")]
        [AllowAnonymous]
        public IActionResult ExportTemplate()
        {
            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Organogram Data");

            // Headers
            worksheet.Cells[1, 1].Value = "Company Name";
            worksheet.Cells[1, 2].Value = "Department Name (English)";
            worksheet.Cells[1, 3].Value = "Department Name (Bangla)";
            worksheet.Cells[1, 4].Value = "Section Name (English)";
            worksheet.Cells[1, 5].Value = "Section Name (Bangla)";
            worksheet.Cells[1, 6].Value = "Designation Name (English)";
            worksheet.Cells[1, 7].Value = "Designation Name (Bangla)";
            worksheet.Cells[1, 8].Value = "Night Bill";
            worksheet.Cells[1, 9].Value = "Holiday Bill";
            worksheet.Cells[1, 10].Value = "Attendance Bonus";
            worksheet.Cells[1, 11].Value = "Line Name (English)";
            worksheet.Cells[1, 12].Value = "Line Name (Bangla)";

            // Style headers
            using (var range = worksheet.Cells[1, 1, 1, 12])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }

            // Sample data
            worksheet.Cells[2, 1].Value = "Example Corp";
            worksheet.Cells[2, 2].Value = "Human Resources";
            worksheet.Cells[2, 3].Value = "মানব সম্পদ";
            worksheet.Cells[2, 4].Value = "Recruitment";
            worksheet.Cells[2, 5].Value = "নিয়োগ";
            worksheet.Cells[2, 6].Value = "HR Manager";
            worksheet.Cells[2, 7].Value = "এইচআর ম্যানেজার";
            worksheet.Cells[2, 8].Value = 100;
            worksheet.Cells[2, 9].Value = 200;
            worksheet.Cells[2, 10].Value = 500;
            worksheet.Cells[2, 11].Value = "Line 1";
            worksheet.Cells[2, 12].Value = "লাইন ১";

            worksheet.Cells[3, 1].Value = "Example Corp";
            worksheet.Cells[3, 2].Value = "Production";
            worksheet.Cells[3, 3].Value = "উৎপাদন";
            worksheet.Cells[3, 4].Value = "Cutting";
            worksheet.Cells[3, 5].Value = "কাটিং";
            worksheet.Cells[3, 6].Value = "Supervisor";
            worksheet.Cells[3, 7].Value = "সুপারভাইজার";
            worksheet.Cells[3, 8].Value = 50;
            worksheet.Cells[3, 9].Value = 100;
            worksheet.Cells[3, 10].Value = 300;
            worksheet.Cells[3, 11].Value = "Line A";
            worksheet.Cells[3, 12].Value = "লাইন এ";

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Organogram_Template.xlsx");
        }

        [HttpPost("import")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.ItOfficer)]
        public async Task<ActionResult<OrganogramImportResultDto>> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a valid Excel file.");

            var result = new OrganogramImportResultDto();

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new OfficeOpenXml.ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    result.Errors.Add(new ImportErrorDto
                        { RowNumber = 0, Field = "File", Message = "No worksheet found in the Excel file." });
                    return BadRequest(result);
                }

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount <= 1)
                {
                    result.Errors.Add(new ImportErrorDto
                        { RowNumber = 0, Field = "File", Message = "No data rows found." });
                    return BadRequest(result);
                }

                result.TotalRows = rowCount - 1; // Exclude header

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var companyName = worksheet.Cells[row, 1].Text?.Trim();
                        var deptNameEn = worksheet.Cells[row, 2].Text?.Trim();
                        var deptNameBn = worksheet.Cells[row, 3].Text?.Trim();
                        var sectNameEn = worksheet.Cells[row, 4].Text?.Trim();
                        var sectNameBn = worksheet.Cells[row, 5].Text?.Trim();
                        var desigNameEn = worksheet.Cells[row, 6].Text?.Trim();
                        var desigNameBn = worksheet.Cells[row, 7].Text?.Trim();

                        decimal.TryParse(worksheet.Cells[row, 8].Text, out var nightBill);
                        decimal.TryParse(worksheet.Cells[row, 9].Text, out var holidayBill);
                        decimal.TryParse(worksheet.Cells[row, 10].Text, out var attendanceBonus);

                        var lineNameEn = worksheet.Cells[row, 11].Text?.Trim();
                        var lineNameBn = worksheet.Cells[row, 12].Text?.Trim();

                        // Validate required fields
                        if (string.IsNullOrEmpty(companyName))
                        {
                            result.Errors.Add(new ImportErrorDto
                                { RowNumber = row, Field = "Company Name", Message = "Company name is required." });
                            result.ErrorCount++;
                            continue;
                        }

                        if (string.IsNullOrEmpty(deptNameEn))
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                RowNumber = row, Field = "Department Name (EN)",
                                Message = "Department name (English) is required."
                            });
                            result.ErrorCount++;
                            continue;
                        }

                        // Find or skip if company doesn't exist
                        var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyNameEn == companyName);
                        if (company == null)
                        {
                            result.Warnings.Add(new ImportWarningDto
                                { RowNumber = row, Message = $"Company '{companyName}' not found. Row skipped." });
                            result.WarningCount++;
                            continue;
                        }

                        // Upsert Department
                        var dept = await _context.Departments.FirstOrDefaultAsync(d =>
                            d.NameEn == deptNameEn && d.CompanyId == company.Id);
                        if (dept == null)
                        {
                            dept = new Department
                            {
                                NameEn = deptNameEn,
                                NameBn = deptNameBn ?? "",
                                CompanyId = company.Id
                            };
                            _context.Departments.Add(dept);
                            await _context.SaveChangesAsync();
                            result.CreatedCount++;
                        }
                        else
                        {
                            dept.NameBn = deptNameBn ?? dept.NameBn;
                            await _context.SaveChangesAsync();
                            result.UpdatedCount++;
                            result.Warnings.Add(new ImportWarningDto
                                { RowNumber = row, Message = $"Department '{deptNameEn}' already exists. Updated." });
                            result.WarningCount++;
                        }

                        // Upsert Section (if provided)
                        if (!string.IsNullOrEmpty(sectNameEn))
                        {
                            var section = await _context.Sections.FirstOrDefaultAsync(s =>
                                s.NameEn == sectNameEn && s.DepartmentId == dept.Id);
                            if (section == null)
                            {
                                section = new Section
                                {
                                    NameEn = sectNameEn,
                                    NameBn = sectNameBn ?? "",
                                    DepartmentId = dept.Id
                                };
                                _context.Sections.Add(section);
                                await _context.SaveChangesAsync();
                                result.CreatedCount++;
                            }
                            else
                            {
                                section.NameBn = sectNameBn ?? section.NameBn;
                                await _context.SaveChangesAsync();
                                result.UpdatedCount++;
                            }

                            // Upsert Designation (if provided)
                            if (!string.IsNullOrEmpty(desigNameEn))
                            {
                                var designation = await _context.Designations.FirstOrDefaultAsync(d =>
                                    d.NameEn == desigNameEn && d.SectionId == section.Id);
                                if (designation == null)
                                {
                                    designation = new Designation
                                    {
                                        NameEn = desigNameEn,
                                        NameBn = desigNameBn ?? "",
                                        NightBill = nightBill,
                                        HolidayBill = holidayBill,
                                        AttendanceBonus = attendanceBonus,
                                        SectionId = section.Id
                                    };
                                    _context.Designations.Add(designation);
                                    await _context.SaveChangesAsync();
                                    result.CreatedCount++;
                                }
                                else
                                {
                                    designation.NameBn = desigNameBn ?? designation.NameBn;
                                    designation.NightBill = nightBill;
                                    designation.HolidayBill = holidayBill;
                                    designation.AttendanceBonus = attendanceBonus;
                                    await _context.SaveChangesAsync();
                                    result.UpdatedCount++;
                                }
                            }

                            // Upsert Line (if provided)
                            if (!string.IsNullOrEmpty(lineNameEn))
                            {
                                var line = await _context.Lines.FirstOrDefaultAsync(l =>
                                    l.NameEn == lineNameEn && l.SectionId == section.Id);
                                if (line == null)
                                {
                                    line = new Line
                                    {
                                        NameEn = lineNameEn,
                                        NameBn = lineNameBn ?? "",
                                        SectionId = section.Id
                                    };
                                    _context.Lines.Add(line);
                                    await _context.SaveChangesAsync();
                                    result.CreatedCount++;
                                }
                                else
                                {
                                    line.NameBn = lineNameBn ?? line.NameBn;
                                    await _context.SaveChangesAsync();
                                    result.UpdatedCount++;
                                }
                            }
                        }

                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new ImportErrorDto
                            { RowNumber = row, Field = "General", Message = ex.Message });
                        result.ErrorCount++;
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportErrorDto
                    { RowNumber = 0, Field = "File", Message = $"Failed to process file: {ex.Message}" });
                return BadRequest(result);
            }
        }
    }
}
