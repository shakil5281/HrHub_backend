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
            [FromQuery] int? sectionId,
            [FromQuery] int? designationId,
            [FromQuery] int? lineId,
            [FromQuery] int? shiftId,
            [FromQuery] int? groupId,
            [FromQuery] int? floorId,
            [FromQuery] string? status,
            [FromQuery] bool? isActive,
            [FromQuery] string? employeeId,
            [FromQuery] string? searchTerm)
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

            if (sectionId.HasValue)
                query = query.Where(e => e.SectionId == sectionId.Value);

            if (designationId.HasValue)
                query = query.Where(e => e.DesignationId == designationId.Value);

            if (lineId.HasValue)
                query = query.Where(e => e.LineId == lineId.Value);

            if (shiftId.HasValue)
                query = query.Where(e => e.ShiftId == shiftId.Value);

            if (groupId.HasValue)
                query = query.Where(e => e.GroupId == groupId.Value);

            if (floorId.HasValue)
                query = query.Where(e => e.FloorId == floorId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(e => e.Status == status);

            if (isActive.HasValue)
                query = query.Where(e => e.IsActive == isActive.Value);

            if (!string.IsNullOrEmpty(employeeId))
                query = query.Where(e => e.EmployeeId.Contains(employeeId));

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => 
                    e.EmployeeId.Contains(searchTerm) || 
                    e.FullNameEn.Contains(searchTerm) || 
                    (e.FullNameBn != null && e.FullNameBn.Contains(searchTerm)) ||
                    (e.PhoneNumber != null && e.PhoneNumber.Contains(searchTerm))
                );
            }

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


        // GET: api/employee/export
        [HttpGet("export")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," + UserRoles.HrOfficer)]
        public async Task<IActionResult> ExportEmployees(
            [FromQuery] int? departmentId,
            [FromQuery] int? sectionId,
            [FromQuery] int? designationId,
            [FromQuery] int? lineId,
            [FromQuery] int? shiftId,
            [FromQuery] int? groupId,
            [FromQuery] int? floorId,
            [FromQuery] string? status,
            [FromQuery] bool? isActive,
            [FromQuery] string? employeeId,
            [FromQuery] string? searchTerm)
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

            if (departmentId.HasValue) query = query.Where(e => e.DepartmentId == departmentId.Value);
            if (sectionId.HasValue) query = query.Where(e => e.SectionId == sectionId.Value);
            if (designationId.HasValue) query = query.Where(e => e.DesignationId == designationId.Value);
            if (lineId.HasValue) query = query.Where(e => e.LineId == lineId.Value);
            if (shiftId.HasValue) query = query.Where(e => e.ShiftId == shiftId.Value);
            if (groupId.HasValue) query = query.Where(e => e.GroupId == groupId.Value);
            if (floorId.HasValue) query = query.Where(e => e.FloorId == floorId.Value);
            if (!string.IsNullOrEmpty(status)) query = query.Where(e => e.Status == status);
            if (isActive.HasValue) query = query.Where(e => e.IsActive == isActive.Value);
            if (!string.IsNullOrEmpty(employeeId)) query = query.Where(e => e.EmployeeId.Contains(employeeId));
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => 
                    e.EmployeeId.Contains(searchTerm) || 
                    e.FullNameEn.Contains(searchTerm) || 
                    (e.FullNameBn != null && e.FullNameBn.Contains(searchTerm)) ||
                    (e.PhoneNumber != null && e.PhoneNumber.Contains(searchTerm))
                );
            }

            var employees = await query.OrderByDescending(e => e.CreatedAt).ToListAsync();

            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Employees");

            // Headers
            var headers = new[]
            {
                "SL", "ID", "Name (EN)", "Designation", "Department", "Section", 
                "Line", "Shift", "Status", "Join Date", "Phone", "Email", "Gross Salary"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            int row = 2;
            foreach (var emp in employees)
            {
                worksheet.Cells[row, 1].Value = row - 1;
                worksheet.Cells[row, 2].Value = emp.EmployeeId;
                worksheet.Cells[row, 3].Value = emp.FullNameEn;
                worksheet.Cells[row, 4].Value = emp.Designation?.NameEn;
                worksheet.Cells[row, 5].Value = emp.Department?.NameEn;
                worksheet.Cells[row, 6].Value = emp.Section?.NameEn;
                worksheet.Cells[row, 7].Value = emp.Line?.NameEn;
                worksheet.Cells[row, 8].Value = emp.Shift?.NameEn;
                worksheet.Cells[row, 9].Value = emp.Status;
                worksheet.Cells[row, 10].Value = emp.JoinDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 11].Value = emp.PhoneNumber;
                worksheet.Cells[row, 12].Value = emp.Email;
                worksheet.Cells[row, 13].Value = emp.GrossSalary;
                row++;
            }

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"Employee_List_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        // GET: api/employee/export-template
        [HttpGet("export-template")]
        [AllowAnonymous]
        public IActionResult ExportTemplate()
        {
            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Employee Data");

            // Headers
            // SL (1)
            worksheet.Cells[1, 1].Value = "SL";

            // ID Info (2-3)
            worksheet.Cells[1, 2].Value = "Employee ID";
            worksheet.Cells[1, 3].Value = "Card ID (Proximity)";

            // Basic Info (4-17)
            worksheet.Cells[1, 4].Value = "Full Name (EN)";
            worksheet.Cells[1, 5].Value = "Full Name (BN)";
            worksheet.Cells[1, 6].Value = "NID";
            worksheet.Cells[1, 7].Value = "Date of Birth";
            worksheet.Cells[1, 8].Value = "Gender";
            worksheet.Cells[1, 9].Value = "Religion";
            worksheet.Cells[1, 10].Value = "Department";
            worksheet.Cells[1, 11].Value = "Section";
            worksheet.Cells[1, 12].Value = "Designation";
            worksheet.Cells[1, 13].Value = "Line";
            worksheet.Cells[1, 14].Value = "Status";
            worksheet.Cells[1, 15].Value = "Join Date";
            worksheet.Cells[1, 16].Value = "Email";
            worksheet.Cells[1, 17].Value = "Phone Number";

            // Present Address (18-24)
            worksheet.Cells[1, 18].Value = "Present Address (EN)";
            worksheet.Cells[1, 19].Value = "Present Address (BN)";
            worksheet.Cells[1, 20].Value = "Present Division";
            worksheet.Cells[1, 21].Value = "Present District";
            worksheet.Cells[1, 22].Value = "Present Thana";
            worksheet.Cells[1, 23].Value = "Present Post Office";
            worksheet.Cells[1, 24].Value = "Present Postal Code";

            // Permanent Address (25-31)
            worksheet.Cells[1, 25].Value = "Permanent Address (EN)";
            worksheet.Cells[1, 26].Value = "Permanent Address (BN)";
            worksheet.Cells[1, 27].Value = "Permanent Division";
            worksheet.Cells[1, 28].Value = "Permanent District";
            worksheet.Cells[1, 29].Value = "Permanent Thana";
            worksheet.Cells[1, 30].Value = "Permanent Post Office";
            worksheet.Cells[1, 31].Value = "Permanent Postal Code";

            // Family Info (32-40)
            worksheet.Cells[1, 32].Value = "Father Name (EN)";
            worksheet.Cells[1, 33].Value = "Father Name (BN)";
            worksheet.Cells[1, 34].Value = "Mother Name (EN)";
            worksheet.Cells[1, 35].Value = "Mother Name (BN)";
            worksheet.Cells[1, 36].Value = "Marital Status";
            worksheet.Cells[1, 37].Value = "Spouse Name (EN)";
            worksheet.Cells[1, 38].Value = "Spouse Name (BN)";
            worksheet.Cells[1, 39].Value = "Spouse Occupation";
            worksheet.Cells[1, 40].Value = "Spouse Contact";

            // Salary Info (41-47)
            worksheet.Cells[1, 41].Value = "Gross Salary";
            worksheet.Cells[1, 42].Value = "Basic Salary (Auto)";
            worksheet.Cells[1, 43].Value = "House Rent (Auto)";
            worksheet.Cells[1, 44].Value = "Medical (Auto)";
            worksheet.Cells[1, 45].Value = "Conveyance (Auto)";
            worksheet.Cells[1, 46].Value = "Food (Auto)";
            worksheet.Cells[1, 47].Value = "Other (Auto)";

            // Account Info (48-52)
            worksheet.Cells[1, 48].Value = "Bank Name";
            worksheet.Cells[1, 49].Value = "Bank Branch";
            worksheet.Cells[1, 50].Value = "Account No";
            worksheet.Cells[1, 51].Value = "Routing No";
            worksheet.Cells[1, 52].Value = "Account Type";

            // Emergency Contact (53-56)
            worksheet.Cells[1, 53].Value = "Emergency Name";
            worksheet.Cells[1, 54].Value = "Emergency Relation";
            worksheet.Cells[1, 55].Value = "Emergency Phone";
            worksheet.Cells[1, 56].Value = "Emergency Address";

            // Extras (57-60)
            worksheet.Cells[1, 57].Value = "Shift";
            worksheet.Cells[1, 58].Value = "Group";
            worksheet.Cells[1, 59].Value = "Floor";
            worksheet.Cells[1, 60].Value = "OT Status";

            // Style headers
            using (var range = worksheet.Cells[1, 1, 1, 60])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Instructions as comments
            worksheet.Cells[1, 2]
                .AddComment("Optional. If left blank, ID will be auto-generated.", "System");
            worksheet.Cells[1, 41]
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
            // SL (1)
            worksheet.Cells[1, 1].Value = "SL";

            // ID Info (2-3)
            worksheet.Cells[1, 2].Value = "Employee ID";
            worksheet.Cells[1, 3].Value = "Card ID (Proximity)";

            // Basic Info (4-17)
            worksheet.Cells[1, 4].Value = "Full Name (EN)";
            worksheet.Cells[1, 5].Value = "Full Name (BN)";
            worksheet.Cells[1, 6].Value = "NID";
            worksheet.Cells[1, 7].Value = "Date of Birth";
            worksheet.Cells[1, 8].Value = "Gender";
            worksheet.Cells[1, 9].Value = "Religion";
            worksheet.Cells[1, 10].Value = "Department";
            worksheet.Cells[1, 11].Value = "Section";
            worksheet.Cells[1, 12].Value = "Designation";
            worksheet.Cells[1, 13].Value = "Line";
            worksheet.Cells[1, 14].Value = "Status";
            worksheet.Cells[1, 15].Value = "Join Date";
            worksheet.Cells[1, 16].Value = "Email";
            worksheet.Cells[1, 17].Value = "Phone Number";

            // Present Address (18-24)
            worksheet.Cells[1, 18].Value = "Present Address (EN)";
            worksheet.Cells[1, 19].Value = "Present Address (BN)";
            worksheet.Cells[1, 20].Value = "Present Division";
            worksheet.Cells[1, 21].Value = "Present District";
            worksheet.Cells[1, 22].Value = "Present Thana";
            worksheet.Cells[1, 23].Value = "Present Post Office";
            worksheet.Cells[1, 24].Value = "Present Postal Code";

            // Permanent Address (25-31)
            worksheet.Cells[1, 25].Value = "Permanent Address (EN)";
            worksheet.Cells[1, 26].Value = "Permanent Address (BN)";
            worksheet.Cells[1, 27].Value = "Permanent Division";
            worksheet.Cells[1, 28].Value = "Permanent District";
            worksheet.Cells[1, 29].Value = "Permanent Thana";
            worksheet.Cells[1, 30].Value = "Permanent Post Office";
            worksheet.Cells[1, 31].Value = "Permanent Postal Code";

            // Family Info (32-40)
            worksheet.Cells[1, 32].Value = "Father Name (EN)";
            worksheet.Cells[1, 33].Value = "Father Name (BN)";
            worksheet.Cells[1, 34].Value = "Mother Name (EN)";
            worksheet.Cells[1, 35].Value = "Mother Name (BN)";
            worksheet.Cells[1, 36].Value = "Marital Status";
            worksheet.Cells[1, 37].Value = "Spouse Name (EN)";
            worksheet.Cells[1, 38].Value = "Spouse Name (BN)";
            worksheet.Cells[1, 39].Value = "Spouse Occupation";
            worksheet.Cells[1, 40].Value = "Spouse Contact";

            // Salary Info (41-47)
            worksheet.Cells[1, 41].Value = "Gross Salary";
            worksheet.Cells[1, 42].Value = "Basic Salary (Auto)";
            worksheet.Cells[1, 43].Value = "House Rent (Auto)";
            worksheet.Cells[1, 44].Value = "Medical (Auto)";
            worksheet.Cells[1, 45].Value = "Conveyance (Auto)";
            worksheet.Cells[1, 46].Value = "Food (Auto)";
            worksheet.Cells[1, 47].Value = "Other (Auto)";

            // Account Info (48-52)
            worksheet.Cells[1, 48].Value = "Bank Name";
            worksheet.Cells[1, 49].Value = "Bank Branch";
            worksheet.Cells[1, 50].Value = "Account No";
            worksheet.Cells[1, 51].Value = "Routing No";
            worksheet.Cells[1, 52].Value = "Account Type";

            // Emergency Contact (53-56)
            worksheet.Cells[1, 53].Value = "Emergency Name";
            worksheet.Cells[1, 54].Value = "Emergency Relation";
            worksheet.Cells[1, 55].Value = "Emergency Phone";
            worksheet.Cells[1, 56].Value = "Emergency Address";

            // Extras
            worksheet.Cells[1, 57].Value = "Shift";
            worksheet.Cells[1, 58].Value = "Group";
            worksheet.Cells[1, 59].Value = "Floor";
            worksheet.Cells[1, 60].Value = "OT Status";

            // Style headers
            using (var range = worksheet.Cells[1, 1, 1, 60])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Demo data - 1 row sample
            int r = 2;
            worksheet.Cells[r, 1].Value = 1; // SL
            worksheet.Cells[r, 2].Value = "EMP000123";
            worksheet.Cells[r, 3].Value = "100200300";
            worksheet.Cells[r, 4].Value = "John Doe";
            worksheet.Cells[r, 5].Value = "জন ডো";
            worksheet.Cells[r, 6].Value = "1234567890";
            worksheet.Cells[r, 7].Value = "1990-01-15";
            worksheet.Cells[r, 8].Value = "Male";
            worksheet.Cells[r, 9].Value = "Islam";
            worksheet.Cells[r, 10].Value = "Engineering";
            worksheet.Cells[r, 11].Value = "Backend";
            worksheet.Cells[r, 12].Value = "Senior Developer";
            worksheet.Cells[r, 13].Value = "Line 01";
            worksheet.Cells[r, 14].Value = "Active";
            worksheet.Cells[r, 15].Value = DateTime.Now.ToString("yyyy-MM-dd");
            worksheet.Cells[r, 16].Value = "john@example.com";
            worksheet.Cells[r, 17].Value = "+880 1712345678";

            // Present Address
            worksheet.Cells[r, 18].Value = "123 Main St";
            worksheet.Cells[r, 19].Value = "১২২৩ মেইন রোড";
            worksheet.Cells[r, 20].Value = "Dhaka";
            worksheet.Cells[r, 21].Value = "Dhaka";
            worksheet.Cells[r, 22].Value = "Gulshan";
            worksheet.Cells[r, 23].Value = "Gulshan Model Town";
            worksheet.Cells[r, 24].Value = "1212";

            // Permanent Address
            worksheet.Cells[r, 25].Value = "456 Village Rd";
            worksheet.Cells[r, 26].Value = "৪৫৬ গ্রাম রোড";
            worksheet.Cells[r, 27].Value = "Chittagong";
            worksheet.Cells[r, 28].Value = "Comilla";
            worksheet.Cells[r, 29].Value = "Kotwali";
            worksheet.Cells[r, 30].Value = "Main PO";
            worksheet.Cells[r, 31].Value = "3500";

            // Family
            worksheet.Cells[r, 32].Value = "Father Doe";
            worksheet.Cells[r, 33].Value = "ফাদার ডো";
            worksheet.Cells[r, 34].Value = "Mother Doe";
            worksheet.Cells[r, 35].Value = "মাদার ডো";
            worksheet.Cells[r, 36].Value = "Married";
            worksheet.Cells[r, 37].Value = "Mrs. Doe";
            worksheet.Cells[r, 38].Value = "মিসেস ডো";
            worksheet.Cells[r, 39].Value = "Housewife";
            worksheet.Cells[r, 40].Value = "01700000000";

            // Salary
            worksheet.Cells[r, 41].Value = 25000;

            // Account
            worksheet.Cells[r, 48].Value = "Islami Bank Bangladesh Limited";
            worksheet.Cells[r, 49].Value = "Chawrasta";
            worksheet.Cells[r, 50].Value = "205099887766";
            worksheet.Cells[r, 51].Value = "123456789";
            worksheet.Cells[r, 52].Value = "Savings";

            // Emergency
            worksheet.Cells[r, 53].Value = "Brother Doe";
            worksheet.Cells[r, 54].Value = "Brother";
            worksheet.Cells[r, 55].Value = "01800000000";
            worksheet.Cells[r, 56].Value = "Same as Present";

            worksheet.Cells[r, 57].Value = "General";
            worksheet.Cells[r, 58].Value = "A";
            worksheet.Cells[r, 59].Value = "1st Floor";
            worksheet.Cells[r, 60].Value = "Yes";

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

                // Load reference data safely (handling potential duplicates in database)
                var departments = (await _context.Departments.ToListAsync())
                    .GroupBy(x => x.NameEn, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var sections = await _context.Sections.Include(s => s.Department).ToListAsync();

                var designations = (await _context.Designations.ToListAsync())
                    .GroupBy(x => x.NameEn, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var lines = (await _context.Lines.ToListAsync())
                    .GroupBy(x => x.NameEn, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var shifts = (await _context.Shifts.ToListAsync())
                    .GroupBy(x => x.NameEn, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var groups = (await _context.Groups.ToListAsync())
                    .GroupBy(x => x.NameEn, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var floors = (await _context.Floors.ToListAsync())
                    .GroupBy(x => x.NameEn, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                // Address References
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
                        var providedEmpId = GetVal(2);
                        var proximity = GetVal(3);
                        var fullNameEn = GetVal(4);
                        var fullNameBn = GetVal(5);
                        var nid = GetVal(6);
                        var dobText = GetVal(7);
                        var gender = GetVal(8);
                        var religion = GetVal(9);
                        var deptName = GetVal(10);
                        var secName = GetVal(11);
                        var desigName = GetVal(12);
                        var lineName = GetVal(13);
                        var status = GetVal(14);
                        var joinDateText = GetVal(15);
                        var email = GetVal(16);
                        var phone = GetVal(17);

                        // Present Address
                        var preAddrEn = GetVal(18);
                        var preAddrBn = GetVal(19);
                        var preDiv = GetVal(20);
                        var preDist = GetVal(21);
                        var preThana = GetVal(22);
                        var prePO = GetVal(23);
                        var prePC = GetVal(24);

                        // Permanent Address
                        var perAddrEn = GetVal(25);
                        var perAddrBn = GetVal(26);
                        var perDiv = GetVal(27);
                        var perDist = GetVal(28);
                        var perThana = GetVal(29);
                        var perPO = GetVal(30);
                        var perPC = GetVal(31);

                        // Family
                        var fNameEn = GetVal(32);
                        var fNameBn = GetVal(33);
                        var mNameEn = GetVal(34);
                        var mNameBn = GetVal(35);
                        var marital = GetVal(36);
                        var sNameEn = GetVal(37);
                        var sNameBn = GetVal(38);
                        var sOcc = GetVal(39);
                        var sCont = GetVal(40);

                        // Salary
                        var grossText = GetVal(41);
                        var basicText = GetVal(42);

                        // Account
                        var bankName = GetVal(48);
                        var bankBranch = GetVal(49);
                        var accNo = GetVal(50);
                        var routeNo = GetVal(51);
                        var accType = GetVal(52);

                        // Emergency
                        var emName = GetVal(53);
                        var emRel = GetVal(54);
                        var emPhone = GetVal(55);
                        var emAddr = GetVal(56);

                        // Extras
                        var shiftName = GetVal(57);
                        var groupName = GetVal(58);
                        var floorName = GetVal(59);
                        var otStatusVal = GetVal(60);

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

                        // Resolve Address IDs Lookups
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

                        decimal basic = 0, houseRent = 0, medical = 0, food = 0, conveyance = 0, other = 0;

                        if (gross > 0)
                        {
                            medical = 750; food = 1250; conveyance = 450; other = 0;
                            decimal fixedTotal = medical + food + conveyance + other;
                            if (gross > fixedTotal)
                            {
                                basic = (gross - fixedTotal) / 1.5m;
                                houseRent = gross - fixedTotal - basic;
                            }
                            basic = Math.Round(basic, 2);
                            houseRent = Math.Round(houseRent, 2);
                        }

                        int? shiftId = !string.IsNullOrEmpty(shiftName) && shifts.TryGetValue(shiftName, out var sVal) ? sVal.Id : null;
                        int? groupId = !string.IsNullOrEmpty(groupName) && groups.TryGetValue(groupName, out var gVal) ? gVal.Id : null;
                        int? floorId = !string.IsNullOrEmpty(floorName) && floors.TryGetValue(floorName, out var fVal) ? fVal.Id : null;

                        var employee = new Employee
                        {
                            EmployeeId = string.IsNullOrEmpty(providedEmpId) ? "TEMP" : providedEmpId,
                            Proximity = proximity,
                            FullNameEn = fullNameEn,
                            FullNameBn = fullNameBn,
                            NID = nid,
                            DateOfBirth = dob,
                            Gender = gender,
                            Religion = religion,
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
                            FatherNameEn = fNameEn,
                            FatherNameBn = fNameBn,
                            MotherNameEn = mNameEn,
                            MotherNameBn = mNameBn,
                            MaritalStatus = marital,
                            SpouseNameEn = sNameEn,
                            SpouseNameBn = sNameBn,
                            SpouseOccupation = sOcc,
                            SpouseContact = sCont,
                            GrossSalary = gross,
                            BasicSalary = basic,
                            HouseRent = houseRent,
                            MedicalAllowance = medical,
                            FoodAllowance = food,
                            Conveyance = conveyance,
                            OtherAllowance = other,
                            BankName = bankName,
                            BankBranchName = bankBranch,
                            BankAccountNo = accNo,
                            BankRoutingNo = routeNo,
                            BankAccountType = accType,
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
                        result.Errors.Add(new ImportError { RowNumber = row, Field = "General", Message = ex.Message });
                        result.ErrorCount++;
                    }
                }

                if (result.SuccessCount > 0)
                {
                    var addedEmployees = _context.ChangeTracker.Entries<Employee>()
                        .Where(e => e.State == EntityState.Added)
                        .Select(e => e.Entity)
                        .ToList();

                    if (addedEmployees.Any(e => e.EmployeeId == "TEMP"))
                    {
                        var maxIdStr = await _context.Employees.Where(e => e.EmployeeId.StartsWith("EMP"))
                            .Select(e => e.EmployeeId.Substring(3))
                            .ToListAsync();
                        
                        int nextNumericId = maxIdStr.Count > 0 ? maxIdStr.Select(s => int.TryParse(s, out var i) ? i : 0).Max() : 0;

                        foreach (var emp in addedEmployees.Where(e => e.EmployeeId == "TEMP"))
                        {
                            nextNumericId++;
                            emp.EmployeeId = $"EMP{nextNumericId:D6}";
                        }
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
