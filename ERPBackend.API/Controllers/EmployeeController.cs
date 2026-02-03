using ERPBackend.Core.DTOs;
using ERPBackend.Core.Entities;
using ERPBackend.Core.Models;
using ERPBackend.Core.Constants;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public EmployeeController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/employee
        [HttpGet]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer)]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees(
            [FromQuery] int? departmentId,
            [FromQuery] int? designationId,
            [FromQuery] string? status,
            [FromQuery] bool? isActive,
            [FromQuery] string? employeeId)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Section)
                .Include(e => e.Designation)
                .Include(e => e.Line)
                .Include(e => e.Shift)
                .Include(e => e.Group)
                .Include(e => e.Floor)
                .AsQueryable();

            if (departmentId.HasValue)
                query = query.Where(e => e.DepartmentId == departmentId.Value);

            if (designationId.HasValue)
                query = query.Where(e => e.DesignationId == designationId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(e => e.Status == status);

            if (isActive.HasValue)
                query = query.Where(e => e.IsActive == isActive.Value);

            if (!string.IsNullOrEmpty(employeeId))
                query = query.Where(e => e.EmployeeId.Contains(employeeId));

            var employees = await query
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    EmployeeId = e.EmployeeId,
                    FullNameEn = e.FullNameEn,
                    FullNameBn = e.FullNameBn,
                    NID = e.NID,
                    Proximity = e.Proximity,
                    DateOfBirth = e.DateOfBirth,
                    Gender = e.Gender,
                    Religion = e.Religion,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department != null ? e.Department.NameEn : null,
                    SectionId = e.SectionId,
                    SectionName = e.Section != null ? e.Section.NameEn : null,
                    DesignationId = e.DesignationId,
                    DesignationName = e.Designation != null ? e.Designation.NameEn : null,
                    LineId = e.LineId,
                    LineName = e.Line != null ? e.Line.NameEn : null,
                    ShiftId = e.ShiftId,
                    ShiftName = e.Shift != null ? e.Shift.NameEn : null,
                    GroupId = e.GroupId,
                    GroupName = e.Group != null ? e.Group.NameEn : null,
                    FloorId = e.FloorId,
                    FloorName = e.Floor != null ? e.Floor.NameEn : null,
                    Status = e.Status,
                    JoinDate = e.JoinDate,
                    ProfileImageUrl = e.ProfileImageUrl,
                    SignatureImageUrl = e.SignatureImageUrl,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    PresentAddress = e.PresentAddress,
                    PermanentAddress = e.PermanentAddress,
                    FatherNameEn = e.FatherNameEn,
                    FatherNameBn = e.FatherNameBn,
                    MotherNameEn = e.MotherNameEn,
                    MotherNameBn = e.MotherNameBn,
                    MaritalStatus = e.MaritalStatus,
                    SpouseNameEn = e.SpouseNameEn,
                    SpouseNameBn = e.SpouseNameBn,
                    SpouseOccupation = e.SpouseOccupation,
                    SpouseContact = e.SpouseContact,
                    BasicSalary = e.BasicSalary,
                    HouseRent = e.HouseRent,
                    MedicalAllowance = e.MedicalAllowance,
                    Conveyance = e.Conveyance,
                    FoodAllowance = e.FoodAllowance,
                    OtherAllowance = e.OtherAllowance,
                    GrossSalary = e.GrossSalary,
                    BankName = e.BankName,
                    BankBranchName = e.BankBranchName,
                    BankAccountNo = e.BankAccountNo,
                    BankRoutingNo = e.BankRoutingNo,
                    BankAccountType = e.BankAccountType,
                    EmergencyContactName = e.EmergencyContactName,
                    EmergencyContactRelation = e.EmergencyContactRelation,
                    EmergencyContactPhone = e.EmergencyContactPhone,
                    EmergencyContactAddress = e.EmergencyContactAddress,
                    IsActive = e.IsActive,
                    IsOTEnabled = e.IsOTEnabled,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return Ok(employees);
        }

        // GET: api/employee/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer)]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Section)
                .Include(e => e.Designation)
                .Include(e => e.Line)
                .Include(e => e.Shift)
                .Include(e => e.Group)
                .Include(e => e.Floor)
                .Where(e => e.Id == id)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    EmployeeId = e.EmployeeId,
                    FullNameEn = e.FullNameEn,
                    FullNameBn = e.FullNameBn,
                    NID = e.NID,
                    Proximity = e.Proximity,
                    DateOfBirth = e.DateOfBirth,
                    Gender = e.Gender,
                    Religion = e.Religion,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department != null ? e.Department.NameEn : null,
                    SectionId = e.SectionId,
                    SectionName = e.Section != null ? e.Section.NameEn : null,
                    DesignationId = e.DesignationId,
                    DesignationName = e.Designation != null ? e.Designation.NameEn : null,
                    LineId = e.LineId,
                    LineName = e.Line != null ? e.Line.NameEn : null,
                    ShiftId = e.ShiftId,
                    ShiftName = e.Shift != null ? e.Shift.NameEn : null,
                    GroupId = e.GroupId,
                    GroupName = e.Group != null ? e.Group.NameEn : null,
                    FloorId = e.FloorId,
                    FloorName = e.Floor != null ? e.Floor.NameEn : null,
                    Status = e.Status,
                    JoinDate = e.JoinDate,
                    ProfileImageUrl = e.ProfileImageUrl,
                    SignatureImageUrl = e.SignatureImageUrl,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    PresentAddress = e.PresentAddress,
                    PermanentAddress = e.PermanentAddress,
                    FatherNameEn = e.FatherNameEn,
                    FatherNameBn = e.FatherNameBn,
                    MotherNameEn = e.MotherNameEn,
                    MotherNameBn = e.MotherNameBn,
                    MaritalStatus = e.MaritalStatus,
                    SpouseNameEn = e.SpouseNameEn,
                    SpouseNameBn = e.SpouseNameBn,
                    SpouseOccupation = e.SpouseOccupation,
                    SpouseContact = e.SpouseContact,
                    BasicSalary = e.BasicSalary,
                    HouseRent = e.HouseRent,
                    MedicalAllowance = e.MedicalAllowance,
                    Conveyance = e.Conveyance,
                    FoodAllowance = e.FoodAllowance,
                    OtherAllowance = e.OtherAllowance,
                    GrossSalary = e.GrossSalary,
                    BankName = e.BankName,
                    BankBranchName = e.BankBranchName,
                    BankAccountNo = e.BankAccountNo,
                    BankRoutingNo = e.BankRoutingNo,
                    BankAccountType = e.BankAccountType,
                    EmergencyContactName = e.EmergencyContactName,
                    EmergencyContactRelation = e.EmergencyContactRelation,
                    EmergencyContactPhone = e.EmergencyContactPhone,
                    EmergencyContactAddress = e.EmergencyContactAddress,
                    IsActive = e.IsActive,
                    IsOTEnabled = e.IsOTEnabled,
                    CreatedAt = e.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager)]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee([FromBody] CreateEmployeeDto dto)
        {
            // Use provided Employee ID or generate one if empty
            string employeeId = dto.EmployeeId;

            if (string.IsNullOrWhiteSpace(employeeId))
            {
                var lastEmployee = await _context.Employees
                    .OrderByDescending(e => e.Id)
                    .FirstOrDefaultAsync();

                var nextId = (lastEmployee?.Id ?? 0) + 1;
                employeeId = $"EMP{nextId:D6}"; // EMP000001, EMP000002, etc.
            }
            else
            {
                // Check if Employee ID already exists
                var exists = await _context.Employees.AnyAsync(e => e.EmployeeId == employeeId);
                if (exists)
                {
                    return BadRequest(new { message = $"Employee ID '{employeeId}' already exists." });
                }
            }

            var employee = new Employee
            {
                EmployeeId = employeeId,
                FullNameEn = dto.FullNameEn,
                FullNameBn = dto.FullNameBn,
                NID = dto.NID,
                Proximity = dto.Proximity,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Religion = dto.Religion,
                DepartmentId = dto.DepartmentId,
                SectionId = dto.SectionId,
                DesignationId = dto.DesignationId,
                LineId = dto.LineId,
                ShiftId = dto.ShiftId,
                GroupId = dto.GroupId,
                FloorId = dto.FloorId,
                Status = dto.Status,
                JoinDate = dto.JoinDate,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                PresentAddress = dto.PresentAddress,
                PermanentAddress = dto.PermanentAddress,
                FatherNameEn = dto.FatherNameEn,
                FatherNameBn = dto.FatherNameBn,
                MotherNameEn = dto.MotherNameEn,
                MotherNameBn = dto.MotherNameBn,
                MaritalStatus = dto.MaritalStatus,
                SpouseNameEn = dto.SpouseNameEn,
                SpouseNameBn = dto.SpouseNameBn,
                SpouseOccupation = dto.SpouseOccupation,
                SpouseContact = dto.SpouseContact,
                BasicSalary = dto.BasicSalary,
                HouseRent = dto.HouseRent,
                MedicalAllowance = dto.MedicalAllowance,
                Conveyance = dto.Conveyance,
                FoodAllowance = dto.FoodAllowance,
                OtherAllowance = dto.OtherAllowance,
                GrossSalary = dto.GrossSalary,
                BankName = dto.BankName,
                BankBranchName = dto.BankBranchName,
                BankAccountNo = dto.BankAccountNo,
                BankRoutingNo = dto.BankRoutingNo,
                BankAccountType = dto.BankAccountType,
                EmergencyContactName = dto.EmergencyContactName,
                EmergencyContactRelation = dto.EmergencyContactRelation,
                EmergencyContactPhone = dto.EmergencyContactPhone,
                EmergencyContactAddress = dto.EmergencyContactAddress,

                IsActive = true,
                IsOTEnabled = dto.IsOTEnabled,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var createdEmployee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Section)
                .Include(e => e.Designation)
                .Include(e => e.Line)
                .Where(e => e.Id == employee.Id)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    EmployeeId = e.EmployeeId,
                    FullNameEn = e.FullNameEn,
                    FullNameBn = e.FullNameBn,
                    NID = e.NID,
                    Proximity = e.Proximity,
                    DateOfBirth = e.DateOfBirth,
                    Gender = e.Gender,
                    Religion = e.Religion,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department != null ? e.Department.NameEn : null,
                    SectionId = e.SectionId,
                    SectionName = e.Section != null ? e.Section.NameEn : null,
                    DesignationId = e.DesignationId,
                    DesignationName = e.Designation != null ? e.Designation.NameEn : null,
                    LineId = e.LineId,
                    LineName = e.Line != null ? e.Line.NameEn : null,
                    Status = e.Status,
                    JoinDate = e.JoinDate,
                    ProfileImageUrl = e.ProfileImageUrl,
                    SignatureImageUrl = e.SignatureImageUrl,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    PresentAddress = e.PresentAddress,
                    PermanentAddress = e.PermanentAddress,
                    FatherNameEn = e.FatherNameEn,
                    FatherNameBn = e.FatherNameBn,
                    MotherNameEn = e.MotherNameEn,
                    MotherNameBn = e.MotherNameBn,
                    MaritalStatus = e.MaritalStatus,
                    SpouseNameEn = e.SpouseNameEn,
                    SpouseNameBn = e.SpouseNameBn,
                    SpouseOccupation = e.SpouseOccupation,
                    SpouseContact = e.SpouseContact,
                    BasicSalary = e.BasicSalary,
                    HouseRent = e.HouseRent,
                    MedicalAllowance = e.MedicalAllowance,
                    Conveyance = e.Conveyance,
                    FoodAllowance = e.FoodAllowance,
                    OtherAllowance = e.OtherAllowance,
                    GrossSalary = e.GrossSalary,
                    BankName = e.BankName,
                    BankBranchName = e.BankBranchName,
                    BankAccountNo = e.BankAccountNo,
                    BankRoutingNo = e.BankRoutingNo,
                    BankAccountType = e.BankAccountType,
                    EmergencyContactName = e.EmergencyContactName,
                    EmergencyContactRelation = e.EmergencyContactRelation,
                    EmergencyContactPhone = e.EmergencyContactPhone,
                    EmergencyContactAddress = e.EmergencyContactAddress,
                    IsActive = e.IsActive,
                    IsOTEnabled = e.IsOTEnabled,
                    CreatedAt = e.CreatedAt
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, createdEmployee);
        }

        // PUT: api/employee/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager)]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound();

            employee.FullNameEn = dto.FullNameEn;
            employee.FullNameBn = dto.FullNameBn;
            employee.NID = dto.NID;
            employee.Proximity = dto.Proximity;
            employee.DateOfBirth = dto.DateOfBirth;
            employee.Gender = dto.Gender;
            employee.Religion = dto.Religion;
            employee.DepartmentId = dto.DepartmentId;
            employee.SectionId = dto.SectionId;
            employee.DesignationId = dto.DesignationId;
            employee.LineId = dto.LineId;
            employee.ShiftId = dto.ShiftId;
            employee.GroupId = dto.GroupId;
            employee.FloorId = dto.FloorId;
            employee.Status = dto.Status;
            employee.JoinDate = dto.JoinDate;
            employee.Email = dto.Email;
            employee.PhoneNumber = dto.PhoneNumber;
            employee.PresentAddress = dto.PresentAddress;
            employee.PermanentAddress = dto.PermanentAddress;
            employee.FatherNameEn = dto.FatherNameEn;
            employee.FatherNameBn = dto.FatherNameBn;
            employee.MotherNameEn = dto.MotherNameEn;
            employee.MotherNameBn = dto.MotherNameBn;
            employee.MaritalStatus = dto.MaritalStatus;
            employee.SpouseNameEn = dto.SpouseNameEn;
            employee.SpouseNameBn = dto.SpouseNameBn;
            employee.SpouseOccupation = dto.SpouseOccupation;
            employee.SpouseContact = dto.SpouseContact;
            employee.BasicSalary = dto.BasicSalary;
            employee.HouseRent = dto.HouseRent;
            employee.MedicalAllowance = dto.MedicalAllowance;
            employee.Conveyance = dto.Conveyance;
            employee.FoodAllowance = dto.FoodAllowance;
            employee.OtherAllowance = dto.OtherAllowance;
            employee.GrossSalary = dto.GrossSalary;
            employee.BankName = dto.BankName;
            employee.BankBranchName = dto.BankBranchName;
            employee.BankAccountNo = dto.BankAccountNo;
            employee.BankRoutingNo = dto.BankRoutingNo;
            employee.BankAccountType = dto.BankAccountType;
            employee.EmergencyContactName = dto.EmergencyContactName;
            employee.EmergencyContactRelation = dto.EmergencyContactRelation;
            employee.EmergencyContactPhone = dto.EmergencyContactPhone;
            employee.EmergencyContactAddress = dto.EmergencyContactAddress;

            employee.IsActive = dto.IsActive;
            employee.IsOTEnabled = dto.IsOTEnabled;
            employee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/employee/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin)]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound();

            // Soft delete
            employee.IsActive = false;
            employee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/employee/search
        [HttpGet("search")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer)]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> SearchEmployees([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Search query cannot be empty");

            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Section)
                .Include(e => e.Designation)
                .Include(e => e.Line)
                .Where(e => e.IsActive &&
                            (e.EmployeeId.Contains(query) ||
                             e.FullNameEn.Contains(query) ||
                             (e.FullNameBn != null && e.FullNameBn.Contains(query)) ||
                             (e.NID != null && e.NID.Contains(query)) ||
                             (e.Email != null && e.Email.Contains(query))))
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    EmployeeId = e.EmployeeId,
                    FullNameEn = e.FullNameEn,
                    FullNameBn = e.FullNameBn,
                    NID = e.NID,
                    DateOfBirth = e.DateOfBirth,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department != null ? e.Department.NameEn : null,
                    SectionId = e.SectionId,
                    SectionName = e.Section != null ? e.Section.NameEn : null,
                    DesignationId = e.DesignationId,
                    DesignationName = e.Designation != null ? e.Designation.NameEn : null,
                    LineId = e.LineId,
                    LineName = e.Line != null ? e.Line.NameEn : null,
                    Status = e.Status,
                    JoinDate = e.JoinDate,
                    ProfileImageUrl = e.ProfileImageUrl,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    IsActive = e.IsActive,
                    CreatedAt = e.CreatedAt
                })
                .Take(50)
                .ToListAsync();

            return Ok(employees);
        }

        // GET: api/employee/export-template
        [HttpGet("export-template")]
        [AllowAnonymous]
        public IActionResult ExportTemplate()
        {
            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Employee Data");

            // Headers
            // Basic Info (1-12)
            worksheet.Cells[1, 1].Value = "Full Name (EN)";
            worksheet.Cells[1, 2].Value = "Full Name (BN)";
            worksheet.Cells[1, 3].Value = "NID";
            worksheet.Cells[1, 4].Value = "Date of Birth";
            worksheet.Cells[1, 5].Value = "Department";
            worksheet.Cells[1, 6].Value = "Section";
            worksheet.Cells[1, 7].Value = "Designation";
            worksheet.Cells[1, 8].Value = "Line";
            worksheet.Cells[1, 9].Value = "Status";
            worksheet.Cells[1, 10].Value = "Join Date";
            worksheet.Cells[1, 11].Value = "Email";
            worksheet.Cells[1, 12].Value = "Phone Number";

            // Present Address (13-19)
            worksheet.Cells[1, 13].Value = "Present Address (EN)";
            worksheet.Cells[1, 14].Value = "Present Address (BN)";
            worksheet.Cells[1, 15].Value = "Present Division";
            worksheet.Cells[1, 16].Value = "Present District";
            worksheet.Cells[1, 17].Value = "Present Thana";
            worksheet.Cells[1, 18].Value = "Present Post Office";
            worksheet.Cells[1, 19].Value = "Present Postal Code";

            // Permanent Address (20-26)
            worksheet.Cells[1, 20].Value = "Permanent Address (EN)";
            worksheet.Cells[1, 21].Value = "Permanent Address (BN)";
            worksheet.Cells[1, 22].Value = "Permanent Division";
            worksheet.Cells[1, 23].Value = "Permanent District";
            worksheet.Cells[1, 24].Value = "Permanent Thana";
            worksheet.Cells[1, 25].Value = "Permanent Post Office";
            worksheet.Cells[1, 26].Value = "Permanent Postal Code";

            // Family Info (27-35)
            worksheet.Cells[1, 27].Value = "Father Name (EN)";
            worksheet.Cells[1, 28].Value = "Father Name (BN)";
            worksheet.Cells[1, 29].Value = "Mother Name (EN)";
            worksheet.Cells[1, 30].Value = "Mother Name (BN)";
            worksheet.Cells[1, 31].Value = "Marital Status";
            worksheet.Cells[1, 32].Value = "Spouse Name (EN)";
            worksheet.Cells[1, 33].Value = "Spouse Name (BN)";
            worksheet.Cells[1, 34].Value = "Spouse Occupation";
            worksheet.Cells[1, 35].Value = "Spouse Contact";

            // Salary Info (36-42)
            worksheet.Cells[1, 36].Value = "Gross Salary";
            worksheet.Cells[1, 37].Value = "Basic Salary (Auto)";
            worksheet.Cells[1, 38].Value = "House Rent (Auto)";
            worksheet.Cells[1, 39].Value = "Medical (Auto)";
            worksheet.Cells[1, 40].Value = "Conveyance (Auto)";
            worksheet.Cells[1, 41].Value = "Food (Auto)";
            worksheet.Cells[1, 42].Value = "Other (Auto)";

            // Account Info (43-47)
            worksheet.Cells[1, 43].Value = "Bank Name";
            worksheet.Cells[1, 44].Value = "Bank Branch";
            worksheet.Cells[1, 45].Value = "Account No";
            worksheet.Cells[1, 46].Value = "Routing No";
            worksheet.Cells[1, 47].Value = "Account Type";

            // Emergency Contact (48-51)
            worksheet.Cells[1, 48].Value = "Emergency Name";
            worksheet.Cells[1, 49].Value = "Emergency Relation";
            worksheet.Cells[1, 50].Value = "Emergency Phone";
            worksheet.Cells[1, 51].Value = "Emergency Address";

            // Style headers
            using (var range = worksheet.Cells[1, 1, 1, 51])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Instructions as comments
            worksheet.Cells[1, 36]
                .AddComment("Enter Gross Salary. Other components can be auto-calculated if left blank.", "System");

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);

            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Employee_Template.xlsx");
        }

        // GET: api/employee/export-demo
        [HttpGet("export-demo")]
        [AllowAnonymous]
        public IActionResult ExportDemo()
        {
            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Employee Data");

            // Headers
            // Basic Info (1-12)
            worksheet.Cells[1, 1].Value = "Full Name (EN)";
            worksheet.Cells[1, 2].Value = "Full Name (BN)";
            worksheet.Cells[1, 3].Value = "NID";
            worksheet.Cells[1, 4].Value = "Date of Birth";
            worksheet.Cells[1, 5].Value = "Department";
            worksheet.Cells[1, 6].Value = "Section";
            worksheet.Cells[1, 7].Value = "Designation";
            worksheet.Cells[1, 8].Value = "Line";
            worksheet.Cells[1, 9].Value = "Status";
            worksheet.Cells[1, 10].Value = "Join Date";
            worksheet.Cells[1, 11].Value = "Email";
            worksheet.Cells[1, 12].Value = "Phone Number";

            // Present Address (13-19)
            worksheet.Cells[1, 13].Value = "Present Address (EN)";
            worksheet.Cells[1, 14].Value = "Present Address (BN)";
            worksheet.Cells[1, 15].Value = "Present Division";
            worksheet.Cells[1, 16].Value = "Present District";
            worksheet.Cells[1, 17].Value = "Present Thana";
            worksheet.Cells[1, 18].Value = "Present Post Office";
            worksheet.Cells[1, 19].Value = "Present Postal Code";

            // Permanent Address (20-26)
            worksheet.Cells[1, 20].Value = "Permanent Address (EN)";
            worksheet.Cells[1, 21].Value = "Permanent Address (BN)";
            worksheet.Cells[1, 22].Value = "Permanent Division";
            worksheet.Cells[1, 23].Value = "Permanent District";
            worksheet.Cells[1, 24].Value = "Permanent Thana";
            worksheet.Cells[1, 25].Value = "Permanent Post Office";
            worksheet.Cells[1, 26].Value = "Permanent Postal Code";

            // Family Info (27-35)
            worksheet.Cells[1, 27].Value = "Father Name (EN)";
            worksheet.Cells[1, 28].Value = "Father Name (BN)";
            worksheet.Cells[1, 29].Value = "Mother Name (EN)";
            worksheet.Cells[1, 30].Value = "Mother Name (BN)";
            worksheet.Cells[1, 31].Value = "Marital Status";
            worksheet.Cells[1, 32].Value = "Spouse Name (EN)";
            worksheet.Cells[1, 33].Value = "Spouse Name (BN)";
            worksheet.Cells[1, 34].Value = "Spouse Occupation";
            worksheet.Cells[1, 35].Value = "Spouse Contact";

            // Salary Info (36-42)
            worksheet.Cells[1, 36].Value = "Gross Salary";
            worksheet.Cells[1, 37].Value = "Basic Salary (Auto)";
            worksheet.Cells[1, 38].Value = "House Rent (Auto)";
            worksheet.Cells[1, 39].Value = "Medical (Auto)";
            worksheet.Cells[1, 40].Value = "Conveyance (Auto)";
            worksheet.Cells[1, 41].Value = "Food (Auto)";
            worksheet.Cells[1, 42].Value = "Other (Auto)";

            // Account Info (43-47)
            worksheet.Cells[1, 43].Value = "Bank Name";
            worksheet.Cells[1, 44].Value = "Bank Branch";
            worksheet.Cells[1, 45].Value = "Account No";
            worksheet.Cells[1, 46].Value = "Routing No";
            worksheet.Cells[1, 47].Value = "Account Type";

            // Emergency Contact (48-51)
            worksheet.Cells[1, 48].Value = "Emergency Name";
            worksheet.Cells[1, 49].Value = "Emergency Relation";
            worksheet.Cells[1, 50].Value = "Emergency Phone";
            worksheet.Cells[1, 51].Value = "Emergency Address";
            worksheet.Cells[1, 52].Value = "Shift";
            worksheet.Cells[1, 53].Value = "Group";
            worksheet.Cells[1, 54].Value = "Floor";
            worksheet.Cells[1, 55].Value = "OT Status";

            // Style headers
            using (var range = worksheet.Cells[1, 1, 1, 55])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Demo data - 1 row sample for brevity, but let's add full sample
            int r = 2;
            worksheet.Cells[r, 1].Value = "John Doe";
            worksheet.Cells[r, 2].Value = "জন ডো";
            worksheet.Cells[r, 3].Value = "1234567890";
            worksheet.Cells[r, 4].Value = "1990-01-15";
            worksheet.Cells[r, 5].Value = "Engineering";
            worksheet.Cells[r, 6].Value = "Backend";
            worksheet.Cells[r, 7].Value = "Senior Developer";
            worksheet.Cells[r, 8].Value = "Line 01";
            worksheet.Cells[r, 9].Value = "Active";
            worksheet.Cells[r, 10].Value = DateTime.Now.ToString("yyyy-MM-dd");
            worksheet.Cells[r, 11].Value = "john@example.com";
            worksheet.Cells[r, 12].Value = "+880 1712345678";

            // Present Address
            worksheet.Cells[r, 13].Value = "123 Main St";
            worksheet.Cells[r, 14].Value = "১২৩ মেইন রোড";
            worksheet.Cells[r, 15].Value = "Dhaka";
            worksheet.Cells[r, 16].Value = "Dhaka";
            worksheet.Cells[r, 17].Value = "Gulshan";
            worksheet.Cells[r, 18].Value = "Gulshan Model Town";
            worksheet.Cells[r, 19].Value = "1212";

            // Permanent Address
            worksheet.Cells[r, 20].Value = "456 Village Rd";
            worksheet.Cells[r, 21].Value = "৪৫৬ গ্রাম রোড";
            worksheet.Cells[r, 22].Value = "Chittagong";
            worksheet.Cells[r, 23].Value = "Comilla";
            worksheet.Cells[r, 24].Value = "Kotwali";
            worksheet.Cells[r, 25].Value = "Main PO";
            worksheet.Cells[r, 26].Value = "3500";

            // Family
            worksheet.Cells[r, 27].Value = "Father Doe";
            worksheet.Cells[r, 28].Value = "ফাদার ডো";
            worksheet.Cells[r, 29].Value = "Mother Doe";
            worksheet.Cells[r, 30].Value = "মাদার ডো";
            worksheet.Cells[r, 31].Value = "Married"; // Single, Married, Widowed, Divorced
            worksheet.Cells[r, 32].Value = "Mrs. Doe";
            worksheet.Cells[r, 33].Value = "মিসেস ডো";
            worksheet.Cells[r, 34].Value = "Housewife";
            worksheet.Cells[r, 35].Value = "01700000000";

            // Salary
            worksheet.Cells[r, 36].Value = 25000; // Gross
            // Auto-calc fields left blank or filled for demo

            // Account
            worksheet.Cells[r, 43].Value = "Islami Bank Bangladesh Limited";
            worksheet.Cells[r, 44].Value = "Chawrasta";
            worksheet.Cells[r, 45].Value = "205099887766";
            worksheet.Cells[r, 46].Value = "123456789";
            worksheet.Cells[r, 47].Value = "Savings";

            // Emergency
            worksheet.Cells[r, 48].Value = "Brother Doe";
            worksheet.Cells[r, 49].Value = "Brother";
            worksheet.Cells[r, 50].Value = "01800000000";
            worksheet.Cells[r, 51].Value = "Same as Present";
            worksheet.Cells[r, 52].Value = "General";
            worksheet.Cells[r, 53].Value = "A";
            worksheet.Cells[r, 54].Value = "1st Floor";
            worksheet.Cells[r, 55].Value = "Yes";

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);

            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Employee_Demo_Data.xlsx");
        }

        // POST: api/employee/import
        [HttpPost("import")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager)]
        public async Task<ActionResult<EmployeeImportResultDto>> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var result = new EmployeeImportResultDto();

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                using var package = new OfficeOpenXml.ExcelPackage(stream);

                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    result.Errors.Add(new ImportError
                        { RowNumber = 0, Field = "File", Message = "No worksheet found" });
                    return BadRequest(result);
                }

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount < 2)
                {
                    result.Errors.Add(new ImportError
                        { RowNumber = 0, Field = "File", Message = "No data rows found" });
                    return BadRequest(result);
                }

                // Load reference data
                var departments = await _context.Departments.ToDictionaryAsync(d => d.NameEn, d => d);
                var sections = await _context.Sections.Include(s => s.Department).ToListAsync();
                var designations = await _context.Designations.ToDictionaryAsync(d => d.NameEn, d => d);
                var lines = await _context.Lines.ToDictionaryAsync(l => l.NameEn, l => l);
                var shifts = await _context.Shifts.ToDictionaryAsync(s => s.NameEn, s => s);
                var groups = await _context.Groups.ToDictionaryAsync(g => g.NameEn, g => g);
                var floors = await _context.Floors.ToDictionaryAsync(f => f.NameEn, f => f);

                // Address References
                // For simplified matching, we'll try to match by English name (case insensitive)
                var divisions = await _context.Divisions.ToListAsync();
                var districts = await _context.Districts.Include(d => d.Division).ToListAsync();
                var thanas = await _context.Thanas.Include(t => t.District).ToListAsync();
                var postOffices = await _context.PostOffices.Include(p => p.District).ToListAsync();

                for (int row = 2; row <= rowCount; row++)
                {
                    result.TotalRows++;

                    try
                    {
                        // Helper to get text
                        string GetVal(int col) => worksheet.Cells[row, col].Text.Trim();

                        // Basic Info
                        var fullNameEn = GetVal(1);
                        var fullNameBn = GetVal(2);
                        var nid = GetVal(3);
                        var dobText = GetVal(4);
                        var deptName = GetVal(5);
                        var secName = GetVal(6);
                        var desigName = GetVal(7);
                        var lineName = GetVal(8);
                        var status = GetVal(9);
                        var joinDateText = GetVal(10);
                        var email = GetVal(11);
                        var phone = GetVal(12);
                        var shiftName = GetVal(52);
                        var groupName = GetVal(53);
                        var floorName = GetVal(54);
                        var otStatusVal = GetVal(55);

                        // Present Address
                        var preAddrEn = GetVal(13);
                        var preAddrBn = GetVal(14);
                        var preDiv = GetVal(15);
                        var preDist = GetVal(16);
                        var preThana = GetVal(17);
                        var prePO = GetVal(18);
                        var prePC = GetVal(19);

                        // Permanent Address
                        var perAddrEn = GetVal(20);
                        var perAddrBn = GetVal(21);
                        var perDiv = GetVal(22);
                        var perDist = GetVal(23);
                        var perThana = GetVal(24);
                        var perPO = GetVal(25);
                        var perPC = GetVal(26);

                        // Family
                        var fNameEn = GetVal(27);
                        var fNameBn = GetVal(28);
                        var mNameEn = GetVal(29);
                        var mNameBn = GetVal(30);
                        var marital = GetVal(31);
                        var sNameEn = GetVal(32);
                        var sNameBn = GetVal(33);
                        var sOcc = GetVal(34);
                        var sCont = GetVal(35);

                        // Salary
                        var grossText = GetVal(36);
                        var basicText = GetVal(37); // Optional, can be calculated

                        // Account
                        var bankName = GetVal(43);
                        var bankBranch = GetVal(44);
                        var accNo = GetVal(45);
                        var routeNo = GetVal(46);
                        var accType = GetVal(47);

                        // Emergency
                        var emName = GetVal(48);
                        var emRel = GetVal(49);
                        var emPhone = GetVal(50);
                        var emAddr = GetVal(51);

                        // Validation
                        if (string.IsNullOrEmpty(fullNameEn))
                        {
                            result.Errors.Add(new ImportError
                                { RowNumber = row, Field = "Full Name (EN)", Message = "Required field" });
                            result.ErrorCount++;
                            continue;
                        }

                        if (!departments.ContainsKey(deptName))
                        {
                            result.Errors.Add(new ImportError
                            {
                                RowNumber = row, Field = "Department", Message = $"Department '{deptName}' not found"
                            });
                            result.ErrorCount++;
                            continue;
                        }

                        if (!designations.ContainsKey(desigName))
                        {
                            result.Errors.Add(new ImportError
                            {
                                RowNumber = row, Field = "Designation", Message = $"Designation '{desigName}' not found"
                            });
                            result.ErrorCount++;
                            continue;
                        }

                        // Parse Dates
                        DateTime? dob = null;
                        if (!string.IsNullOrEmpty(dobText) && DateTime.TryParse(dobText, out var dobParsed))
                            dob = dobParsed;

                        if (!DateTime.TryParse(joinDateText, out var joinDate)) joinDate = DateTime.Now;

                        var department = departments[deptName];
                        var designation = designations[desigName];

                        // Section
                        int? sectionId = null;
                        if (!string.IsNullOrEmpty(secName))
                        {
                            var section = sections.FirstOrDefault(s =>
                                s.NameEn.Equals(secName, StringComparison.OrdinalIgnoreCase) &&
                                s.DepartmentId == department.Id);
                            sectionId = section?.Id;
                        }

                        // Line
                        int? lineId = null;
                        if (!string.IsNullOrEmpty(lineName) && lines.ContainsKey(lineName)) lineId = lines[lineName].Id;

                        // Resolve Address IDs
                        // Helper for address lookup
                        int? ResolveDiv(string name) => divisions
                            .FirstOrDefault(d => d.NameEn.Equals(name, StringComparison.OrdinalIgnoreCase))?.Id;

                        int? ResolveDist(string name, int? divId) => districts.FirstOrDefault(d =>
                            d.NameEn.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                            (!divId.HasValue || d.DivisionId == divId))?.Id;

                        int? ResolveThana(string name, int? distId) => thanas.FirstOrDefault(t =>
                            t.NameEn.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                            (!distId.HasValue || t.DistrictId == distId))?.Id;

                        int? ResolvePO(string name, int? distId) => postOffices.FirstOrDefault(p =>
                            p.NameEn.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                            (!distId.HasValue || p.DistrictId == distId))?.Id;

                        // Present
                        int? preDivId = string.IsNullOrEmpty(preDiv) ? null : ResolveDiv(preDiv);
                        int? preDistId = string.IsNullOrEmpty(preDist) ? null : ResolveDist(preDist, preDivId);
                        int? preThanaId = string.IsNullOrEmpty(preThana) ? null : ResolveThana(preThana, preDistId);
                        int? prePOId = string.IsNullOrEmpty(prePO) ? null : ResolvePO(prePO, preDistId);

                        // If Postal code is empty but PO is found, use it
                        string prePCFinal = prePC;
                        if (string.IsNullOrEmpty(prePCFinal) && prePOId.HasValue)
                        {
                            var po = postOffices.FirstOrDefault(p => p.Id == prePOId);
                            if (po != null) prePCFinal = po.Code;
                        }

                        // Permanent
                        int? perDivId = string.IsNullOrEmpty(perDiv) ? null : ResolveDiv(perDiv);
                        int? perDistId = string.IsNullOrEmpty(perDist) ? null : ResolveDist(perDist, perDivId);
                        int? perThanaId = string.IsNullOrEmpty(perThana) ? null : ResolveThana(perThana, perDistId);
                        int? perPOId = string.IsNullOrEmpty(perPO) ? null : ResolvePO(perPO, perDistId);
                        string perPCFinal = perPC;
                        if (string.IsNullOrEmpty(perPCFinal) && perPOId.HasValue)
                        {
                            var po = postOffices.FirstOrDefault(p => p.Id == perPOId);
                            if (po != null) perPCFinal = po.Code;
                        }

                        // Salary Logic
                        decimal gross = 0;
                        decimal.TryParse(grossText, out gross);

                        decimal basic = 0;
                        decimal houseRent = 0;
                        decimal medical = 0;
                        decimal food = 0;
                        decimal conveyance = 0;
                        decimal other = 0;

                        if (gross > 0)
                        {
                            // Apply formula
                            medical = 750;
                            food = 1250;
                            conveyance = 450;
                            other = 0;

                            decimal fixedTotal = medical + food + conveyance + other;
                            if (gross > fixedTotal)
                            {
                                basic = (gross - fixedTotal) / 1.5m;
                                houseRent = gross - fixedTotal - basic;
                            }

                            // Rounding
                            basic = Math.Round(basic, 2);
                            houseRent = Math.Round(houseRent, 2);
                        }

                        // Generate ID
                        // Check if an ID column exists in Excel (we didn't add one in template, assuming auto-gen)
                        // Actually, let's auto-gen to be safe
                        var lastId = await _context.Employees.OrderByDescending(e => e.Id).Select(e => e.Id)
                            .FirstOrDefaultAsync();
                        // Note: In loop this is tricky because we are adding to context but not saving yet.
                        // We should track nextId locally.
                        // Simple approach: create a temporary ID based on current max + created count
                        // Real implementation might need transaction or sequence, but for now:

                        // Better: Generate string ID. 
                        // But wait, EF Core auto-increments the PK 'Id'. We need 'EmployeeId' string.
                        // We can generate a provisional one.

                        int? shiftId = !string.IsNullOrEmpty(shiftName) && shifts.TryGetValue(shiftName, out var sVal)
                            ? sVal.Id
                            : null;
                        int? groupId = !string.IsNullOrEmpty(groupName) && groups.TryGetValue(groupName, out var gVal)
                            ? gVal.Id
                            : null;
                        int? floorId = !string.IsNullOrEmpty(floorName) && floors.TryGetValue(floorName, out var fVal)
                            ? fVal.Id
                            : null;

                        // Let's just create new Employee object
                        var employee = new Employee
                        {
                            // We will set EmployeeId after saving or use a Guid/Timestamp logic if not strict sequential needed during bulk
                            // For strict sequential 'EMP000001', we need to be careful.
                            // Let's defer EmployeeId generation to just before adding, capturing the latest from DB? 
                            // No, db update is at end.
                            // Let's generate a placeholder or calculate based on result.CreatedCount + offset
                            EmployeeId = "TEMP", // Will fix below

                            FullNameEn = fullNameEn,
                            FullNameBn = fullNameBn,
                            NID = nid,
                            DateOfBirth = dob,

                            DepartmentId = department.Id,
                            SectionId = sectionId,
                            DesignationId = designation.Id,
                            LineId = lineId,
                            ShiftId = shiftId,
                            GroupId = groupId,
                            FloorId = floorId,

                            Status = string.IsNullOrEmpty(status) ? "Active" : status,
                            JoinDate = joinDate,
                            IsOTEnabled = otStatusVal.Equals("Yes", StringComparison.OrdinalIgnoreCase) ||
                                          otStatusVal.Equals("True", StringComparison.OrdinalIgnoreCase) ||
                                          otStatusVal.Equals("1"),
                            Email = email,
                            PhoneNumber = phone,

                            // Address
                            PresentAddress = preAddrEn,
                            PresentAddressBn = preAddrBn,
                            PresentDivisionId = preDivId,
                            PresentDistrictId = preDistId,
                            PresentThanaId = preThanaId,
                            PresentPostOfficeId = prePOId,
                            PresentPostalCode = prePCFinal,

                            PermanentAddress = perAddrEn,
                            PermanentAddressBn = perAddrBn,
                            PermanentDivisionId = perDivId,
                            PermanentDistrictId = perDistId,
                            PermanentThanaId = perThanaId,
                            PermanentPostOfficeId = perPOId,
                            PermanentPostalCode = perPCFinal,

                            // Family
                            FatherNameEn = fNameEn,
                            FatherNameBn = fNameBn,
                            MotherNameEn = mNameEn,
                            MotherNameBn = mNameBn,
                            MaritalStatus = marital,
                            SpouseNameEn = sNameEn,
                            SpouseNameBn = sNameBn,
                            SpouseOccupation = sOcc,
                            SpouseContact = sCont,

                            // Salary
                            GrossSalary = gross,
                            BasicSalary = basic,
                            HouseRent = houseRent,
                            MedicalAllowance = medical,
                            FoodAllowance = food,
                            Conveyance = conveyance,
                            OtherAllowance = other,

                            // Account
                            BankName = bankName,
                            BankBranchName = bankBranch,
                            BankAccountNo = accNo,
                            BankRoutingNo = routeNo,
                            BankAccountType = accType,

                            // Emergency
                            EmergencyContactName = emName,
                            EmergencyContactRelation = emRel,
                            EmergencyContactPhone = emPhone,
                            EmergencyContactAddress = emAddr,

                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Employees.Add(employee);
                        result.CreatedCount++;
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new ImportError
                        {
                            RowNumber = row,
                            Field = "General",
                            Message = ex.Message
                        });
                        result.ErrorCount++;
                    }
                }

                if (result.SuccessCount > 0)
                {
                    // We need to fix EmployeeIDs before saving
                    // Strategy: Get max numeric ID from DB, then assign sequential IDs
                    // Limitation: This is not thread-safe in high concurrency but sufficient here.

                    // Actually, we can save changes first to get PKs, then update EmployeeIds?
                    // Or manual calc.
                    var maxId = await _context.Employees.MaxAsync(e => (int?)e.Id) ?? 0;
                    var addedEmployees = _context.ChangeTracker.Entries<Employee>()
                        .Where(e => e.State == EntityState.Added)
                        .Select(e => e.Entity)
                        .ToList();

                    int currentId = maxId;
                    foreach (var emp in addedEmployees)
                    {
                        currentId++;
                        emp.EmployeeId = $"EMP{currentId:D6}";
                    }

                    await _context.SaveChangesAsync();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportError { RowNumber = 0, Field = "File", Message = ex.Message });
                return BadRequest(result);
            }
        }

        [HttpPost("upload-image")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager)]
        public async Task<IActionResult> UploadImage(IFormFile file, string type = "profile")
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Only .jpg, .jpeg, and .png files are allowed");

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "employees", type);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/employees/{type}/{fileName}";
            return Ok(new { url });
        }
    }

    public class EmployeeImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int CreatedCount { get; set; }
        public int UpdatedCount { get; set; }
        public List<ImportError> Errors { get; set; } = new();
        public List<ImportError> Warnings { get; set; } = new();
    }

    public class ImportError
    {
        public int RowNumber { get; set; }
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
