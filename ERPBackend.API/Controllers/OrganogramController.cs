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
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments([FromQuery] int? companyId)
        {
            var query = _context.Departments.AsQueryable();
            if (companyId.HasValue)
            {
                query = query.Where(d => d.CompanyId == companyId);
            }

            return await query
                .Include(d => d.Company)
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
        public async Task<ActionResult<IEnumerable<SectionDto>>> GetSections(int? companyId, int? departmentId)
        {
            var query = _context.Sections.AsQueryable();
            if (companyId.HasValue)
            {
                query = query.Where(s => s.CompanyId == companyId);
            }

            if (departmentId.HasValue)
            {
                query = query.Where(s => s.DepartmentId == departmentId);
            }

            return await query
                .Include(s => s.Company)
                .Include(s => s.Department)
                .Select(s => new SectionDto
                {
                    Id = s.Id,
                    NameEn = s.NameEn,
                    NameBn = s.NameBn,
                    CompanyId = s.CompanyId,
                    CompanyName = s.Company != null ? s.Company.CompanyNameEn : null,
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
            var section = new Section
            {
                NameEn = dto.NameEn, NameBn = dto.NameBn, CompanyId = dto.CompanyId, DepartmentId = dto.DepartmentId
            };
            _context.Sections.Add(section);
            await _context.SaveChangesAsync();
            return Ok(new SectionDto
            {
                Id = section.Id, NameEn = section.NameEn, NameBn = section.NameBn, CompanyId = section.CompanyId,
                DepartmentId = section.DepartmentId
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
            section.CompanyId = dto.CompanyId;
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
        public async Task<ActionResult<IEnumerable<DesignationDto>>> GetDesignations(int? companyId, int? departmentId,
            int? sectionId)
        {
            var query = _context.Designations.AsQueryable();
            if (companyId.HasValue)
            {
                query = query.Where(d => d.CompanyId == companyId);
            }

            if (departmentId.HasValue)
            {
                query = query.Where(d => d.DepartmentId == departmentId);
            }

            if (sectionId.HasValue)
            {
                query = query.Where(d => d.SectionId == sectionId);
            }

            return await query
                .Include(d => d.Company)
                .Include(d => d.Department)
                .Include(d => d.Section)
                .Select(d => new DesignationDto
                {
                    Id = d.Id,
                    NameEn = d.NameEn,
                    NameBn = d.NameBn,
                    NightBill = d.NightBill,
                    TiffinBill = d.TiffinBill,
                    IfterBill = d.IfterBill,
                    HolidayBill = d.HolidayBill,
                    AttendanceBonus = d.AttendanceBonus,
                    CompanyId = d.CompanyId,
                    CompanyName = d.Company != null ? d.Company.CompanyNameEn : null,
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.Department != null ? d.Department.NameEn : null,
                    SectionId = d.SectionId,
                    SectionName = d.Section != null ? d.Section.NameEn : null,
                    IsNightBillEligible = d.IsNightBillEligible,
                    IsStaff = d.IsStaff
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
                TiffinBill = dto.TiffinBill,
                IfterBill = dto.IfterBill,
                HolidayBill = dto.HolidayBill,
                AttendanceBonus = dto.AttendanceBonus,
                CompanyId = dto.CompanyId,
                DepartmentId = dto.DepartmentId,
                SectionId = dto.SectionId,
                IsNightBillEligible = dto.IsNightBillEligible,
                IsStaff = dto.IsStaff
            };
            _context.Designations.Add(designation);
            await _context.SaveChangesAsync();
            return Ok(new DesignationDto
            {
                Id = designation.Id,
                NameEn = designation.NameEn,
                NameBn = designation.NameBn,
                NightBill = designation.NightBill,
                TiffinBill = designation.TiffinBill,
                IfterBill = designation.IfterBill,
                HolidayBill = designation.HolidayBill,
                AttendanceBonus = designation.AttendanceBonus,
                CompanyId = designation.CompanyId,
                DepartmentId = designation.DepartmentId,
                SectionId = designation.SectionId,
                IsNightBillEligible = designation.IsNightBillEligible,
                IsStaff = designation.IsStaff
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
            designation.TiffinBill = dto.TiffinBill;
            designation.IfterBill = dto.IfterBill;
            designation.HolidayBill = dto.HolidayBill;
            designation.AttendanceBonus = dto.AttendanceBonus;
            designation.CompanyId = dto.CompanyId;
            designation.DepartmentId = dto.DepartmentId;
            designation.SectionId = dto.SectionId;
            designation.IsNightBillEligible = dto.IsNightBillEligible;
            designation.IsStaff = dto.IsStaff;
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
        public async Task<ActionResult<IEnumerable<LineDto>>> GetLines(int? companyId, int? departmentId,
            int? sectionId)
        {
            var query = _context.Lines.AsQueryable();
            if (companyId.HasValue)
            {
                query = query.Where(l => l.CompanyId == companyId);
            }

            if (departmentId.HasValue)
            {
                query = query.Where(l => l.DepartmentId == departmentId);
            }

            if (sectionId.HasValue)
            {
                query = query.Where(l => l.SectionId == sectionId);
            }

            return await query
                .Include(l => l.Company)
                .Include(l => l.Department)
                .Include(l => l.Section)
                .Select(l => new LineDto
                {
                    Id = l.Id,
                    NameEn = l.NameEn,
                    NameBn = l.NameBn,
                    CompanyId = l.CompanyId,
                    CompanyName = l.Company != null ? l.Company.CompanyNameEn : null,
                    DepartmentId = l.DepartmentId,
                    DepartmentName = l.Department != null ? l.Department.NameEn : null,
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
            var line = new Line
            {
                NameEn = dto.NameEn,
                NameBn = dto.NameBn,
                CompanyId = dto.CompanyId,
                DepartmentId = dto.DepartmentId,
                SectionId = dto.SectionId
            };
            _context.Lines.Add(line);
            await _context.SaveChangesAsync();
            return Ok(new LineDto
            {
                Id = line.Id,
                NameEn = line.NameEn,
                NameBn = line.NameBn,
                CompanyId = line.CompanyId,
                DepartmentId = line.DepartmentId,
                SectionId = line.SectionId
            });
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
            line.CompanyId = dto.CompanyId;
            line.DepartmentId = dto.DepartmentId;
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
        public async Task<ActionResult<IEnumerable<GroupDto>>> GetGroups([FromQuery] int? companyId, [FromQuery] string? companyName)
        {
            var query = _context.Groups.AsQueryable();
            if (companyId.HasValue)
            {
                query = query.Where(g => g.CompanyId == companyId);
            }
            else if (!string.IsNullOrEmpty(companyName))
            {
                query = query.Where(g => g.CompanyName == companyName);
            }

            return await query
                .Select(g => new GroupDto
                {
                    Id = g.Id,
                    NameEn = g.NameEn,
                    NameBn = g.NameBn,
                    CompanyId = g.CompanyId,
                    CompanyName = g.CompanyName
                })
                .ToListAsync();
        }

        [HttpPost("groups")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<ActionResult<GroupDto>> CreateGroup(CreateGroupDto dto)
        {
            var group = new Group
                { NameEn = dto.NameEn, NameBn = dto.NameBn, CompanyId = dto.CompanyId, CompanyName = dto.CompanyName };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
            return Ok(new GroupDto
            {
                Id = group.Id, NameEn = group.NameEn, NameBn = group.NameBn, CompanyId = group.CompanyId,
                CompanyName = group.CompanyName
            });
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
            group.CompanyId = dto.CompanyId;
            group.CompanyName = dto.CompanyName;
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
        public async Task<ActionResult<IEnumerable<FloorDto>>> GetFloors([FromQuery] int? companyId, [FromQuery] string? companyName)
        {
            var query = _context.Floors.AsQueryable();
            if (companyId.HasValue)
            {
                query = query.Where(f => f.CompanyId == companyId);
            }
            else if (!string.IsNullOrEmpty(companyName))
            {
                query = query.Where(f => f.CompanyName == companyName);
            }

            return await query
                .Select(f => new FloorDto
                {
                    Id = f.Id,
                    NameEn = f.NameEn,
                    NameBn = f.NameBn,
                    CompanyId = f.CompanyId,
                    CompanyName = f.CompanyName
                })
                .ToListAsync();
        }

        [HttpPost("floors")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<ActionResult<FloorDto>> CreateFloor(CreateFloorDto dto)
        {
            var floor = new Floor
                { NameEn = dto.NameEn, NameBn = dto.NameBn, CompanyId = dto.CompanyId, CompanyName = dto.CompanyName };
            _context.Floors.Add(floor);
            await _context.SaveChangesAsync();
            return Ok(new FloorDto
            {
                Id = floor.Id, NameEn = floor.NameEn, NameBn = floor.NameBn, CompanyId = floor.CompanyId,
                CompanyName = floor.CompanyName
            });
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
            floor.CompanyId = dto.CompanyId;
            floor.CompanyName = dto.CompanyName;
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

        // --- Excel Export Template ---
        [HttpGet("export-template")]
        [AllowAnonymous]
        public IActionResult ExportTemplate()
        {
            using var package = new OfficeOpenXml.ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Organogram Data");
            string[] headers = {
                "Company Name","Department Name (English)","Department Name (Bangla)",
                "Section Name (English)","Section Name (Bangla)",
                "Designation Name (English)","Designation Name (Bangla)",
                "Night Bill","Tiffin Bill","Ifter Bill","Holiday Bill","Attendance Bonus",
                "Line Name (English)","Line Name (Bangla)"
            };
            for (int i = 0; i < headers.Length; i++) ws.Cells[1, i+1].Value = headers[i];
            using (var hdr = ws.Cells[1,1,1,14]) {
                hdr.Style.Font.Bold = true;
                hdr.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                hdr.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(16,133,69));
                hdr.Style.Font.Color.SetColor(System.Drawing.Color.White);
                hdr.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }
            object?[][] rows = {
                new object?[]{"Ekushe Fashions Ltd","Production","\u09aa\u09cd\u09b0\u09cb\u09a1\u09be\u0995\u09b6\u09a8","Sewing","\u09b8\u09c1\u0987","General Operator","\u09b8\u09be\u09a7\u09be\u09b0\u09a3 \u0985\u09aa\u09be\u09b0\u09c7\u099f\u09b0",0,0,0,0,725,"Line - 1","\u09b2\u09be\u0987\u09a8 - \u09e7"},
                new object?[]{"","","","","","Helper","\u09b9\u09c7\u09b2\u09cd\u09aa\u09be\u09b0",0,0,0,0,725,"Line - 2","\u09b2\u09be\u0987\u09a8 - \u09e8"},
                new object?[]{"","","","","","Supervisor","\u09b8\u09c1\u09aa\u09be\u09b0\u09ad\u09be\u0987\u099c\u09be\u09b0",200,200,0,0,300,"Line - 3","\u09b2\u09be\u0987\u09a8 - \u09e9"},
                new object?[]{"","","","Finishing","\u09ab\u09bf\u09a8\u09bf\u09b6\u09bf\u0982","Packingman","\u09aa\u09cd\u09af\u09be\u0995\u09bf\u0982\u09ae\u09cd\u09af\u09be\u09a8",0,0,0,0,725,"Finishing","\u09ab\u09bf\u09a8\u09bf\u09b6\u09bf\u0982"},
            };
            for (int r = 0; r < rows.Length; r++)
                for (int c = 0; c < rows[r].Length; c++)
                    ws.Cells[r+2, c+1].Value = rows[r][c];
            ws.Cells.AutoFitColumns();
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;
            return File(stream,"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","Organogram_Template.xlsx");
        }

        // --- Excel Bulk Import ---
        // Rules:
        //   Company/Dept/Section: persist from previous row when blank
        //   Section  : upsert by (NameEn + DepartmentId + CompanyId)
        //   Designation: upsert by (NameEn + SectionId) - optional per row
        //   Line       : upsert by (NameEn + SectionId) - optional per row
        //   Both Designation and Line are independent and can appear on same row
        [HttpPost("import")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," + UserRoles.ItOfficer)]
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
                var ws = package.Workbook.Worksheets.FirstOrDefault();
                if (ws == null) { result.Errors.Add(new ImportErrorDto{RowNumber=0,Field="File",Message="No worksheet found."}); return BadRequest(result); }
                int rowCount = ws.Dimension?.Rows ?? 0;
                if (rowCount <= 1) { result.Errors.Add(new ImportErrorDto{RowNumber=0,Field="File",Message="No data rows found."}); return BadRequest(result); }
                result.TotalRows = rowCount - 1;
                var companies = await _context.Companies.AsNoTracking().ToListAsync();
                Company? lastCompany = null;
                Department? lastDept = null;
                Section? lastSection = null;
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var tx = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        for (int row = 2; row <= rowCount; row++)
                        {
                            string C(int col) => ws.Cells[row, col].Text?.Trim() ?? "";
                            var companyName = C(1); var deptNameEn = C(2); var deptNameBn = C(3);
                            var sectNameEn  = C(4); var sectNameBn = C(5);
                            var desigNameEn = C(6); var desigNameBn = C(7);
                            decimal.TryParse(C(8), out var nightBill);
                            decimal.TryParse(C(9), out var tiffinBill);
                            decimal.TryParse(C(10), out var ifterBill);
                            decimal.TryParse(C(11), out var holidayBill);
                            decimal.TryParse(C(12), out var attendanceBonus);
                            var lineNameEn = C(13); var lineNameBn = C(14);

                            if (string.IsNullOrEmpty(companyName) && string.IsNullOrEmpty(deptNameEn) &&
                                string.IsNullOrEmpty(sectNameEn)  && string.IsNullOrEmpty(desigNameEn) &&
                                string.IsNullOrEmpty(lineNameEn)) { result.TotalRows--; continue; }

                            // Company
                            if (!string.IsNullOrEmpty(companyName)) {
                                var found = companies.FirstOrDefault(c => c.CompanyNameEn.Equals(companyName, StringComparison.OrdinalIgnoreCase));
                                if (found == null) { result.Warnings.Add(new ImportWarningDto{RowNumber=row,Message=$"Company '{companyName}' not found."}); result.WarningCount++; continue; }
                                if (lastCompany == null || lastCompany.Id != found.Id) { lastDept = null; lastSection = null; }
                                lastCompany = found;
                            }
                            if (lastCompany == null) { result.Warnings.Add(new ImportWarningDto{RowNumber=row,Message="No company context."}); result.WarningCount++; continue; }
                            var co = lastCompany;

                            // Department
                            if (!string.IsNullOrEmpty(deptNameEn)) {
                                var d = await _context.Departments.FirstOrDefaultAsync(x => x.NameEn == deptNameEn && x.CompanyId == co.Id);
                                if (d == null) { d = new Department{NameEn=deptNameEn,NameBn=deptNameBn,CompanyId=co.Id}; _context.Departments.Add(d); await _context.SaveChangesAsync(); result.CreatedCount++; }
                                else if (!string.IsNullOrEmpty(deptNameBn) && d.NameBn != deptNameBn) { d.NameBn = deptNameBn; result.UpdatedCount++; }
                                if (lastDept == null || lastDept.Id != d.Id) lastSection = null;
                                lastDept = d;
                            }
                            if (lastDept == null) { result.Warnings.Add(new ImportWarningDto{RowNumber=row,Message="No department context."}); result.WarningCount++; continue; }
                            var dept = lastDept;

                            // Section
                            if (!string.IsNullOrEmpty(sectNameEn)) {
                                var s = await _context.Sections.FirstOrDefaultAsync(x => x.NameEn == sectNameEn && x.DepartmentId == dept.Id && x.CompanyId == co.Id);
                                if (s == null) { s = new Section{NameEn=sectNameEn,NameBn=sectNameBn,CompanyId=co.Id,DepartmentId=dept.Id}; _context.Sections.Add(s); await _context.SaveChangesAsync(); result.CreatedCount++; }
                                else { bool ch=false; if (!string.IsNullOrEmpty(sectNameBn) && s.NameBn!=sectNameBn){s.NameBn=sectNameBn;ch=true;} if(s.DepartmentId!=dept.Id){s.DepartmentId=dept.Id;ch=true;} if(ch)result.UpdatedCount++; }
                                lastSection = s;
                            }
                            if (lastSection == null) { result.Warnings.Add(new ImportWarningDto{RowNumber=row,Message="No section context."}); result.WarningCount++; continue; }
                            var sect = lastSection;
                            bool worked = false;

                            // Designation (upsert by NameEn + SectionId)
                            if (!string.IsNullOrEmpty(desigNameEn)) {
                                var desig = await _context.Designations.FirstOrDefaultAsync(x => x.NameEn == desigNameEn && x.SectionId == sect.Id);
                                if (desig == null) {
                                    _context.Designations.Add(new Designation{NameEn=desigNameEn,NameBn=desigNameBn,CompanyId=co.Id,DepartmentId=dept.Id,SectionId=sect.Id,NightBill=nightBill,TiffinBill=tiffinBill,IfterBill=ifterBill,HolidayBill=holidayBill,AttendanceBonus=attendanceBonus});
                                    result.CreatedCount++;
                                } else {
                                    bool ch=false;
                                    if(!string.IsNullOrEmpty(desigNameBn)&&desig.NameBn!=desigNameBn){desig.NameBn=desigNameBn;ch=true;}
                                    if(desig.NightBill!=nightBill){desig.NightBill=nightBill;ch=true;}
                                    if(desig.TiffinBill!=tiffinBill){desig.TiffinBill=tiffinBill;ch=true;}
                                    if(desig.IfterBill!=ifterBill){desig.IfterBill=ifterBill;ch=true;}
                                    if(desig.HolidayBill!=holidayBill){desig.HolidayBill=holidayBill;ch=true;}
                                    if(desig.AttendanceBonus!=attendanceBonus){desig.AttendanceBonus=attendanceBonus;ch=true;}
                                    if(desig.DepartmentId!=dept.Id){desig.DepartmentId=dept.Id;ch=true;}
                                    if(ch)result.UpdatedCount++;
                                }
                                worked = true;
                            }

                            // Line (upsert by NameEn + SectionId)
                            if (!string.IsNullOrEmpty(lineNameEn)) {
                                var line = await _context.Lines.FirstOrDefaultAsync(x => x.NameEn == lineNameEn && x.SectionId == sect.Id);
                                if (line == null) {
                                    _context.Lines.Add(new Line{NameEn=lineNameEn,NameBn=lineNameBn,CompanyId=co.Id,DepartmentId=dept.Id,SectionId=sect.Id});
                                    result.CreatedCount++;
                                } else {
                                    bool ch=false;
                                    if(!string.IsNullOrEmpty(lineNameBn)&&line.NameBn!=lineNameBn){line.NameBn=lineNameBn;ch=true;}
                                    if(line.DepartmentId!=dept.Id){line.DepartmentId=dept.Id;ch=true;}
                                    if(line.SectionId!=sect.Id){line.SectionId=sect.Id;ch=true;}
                                    if(ch)result.UpdatedCount++;
                                }
                                worked = true;
                            }

                            await _context.SaveChangesAsync();
                            if (worked) result.SuccessCount++;
                        }
                        await tx.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await tx.RollbackAsync();
                        result.Errors.Add(new ImportErrorDto{RowNumber=0,Field="Transaction",Message=ex.InnerException?.Message??ex.Message});
                        result.ErrorCount = 1;
                    }
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Failed to process file: {ex.Message}" });
            }
        }
    }
}
