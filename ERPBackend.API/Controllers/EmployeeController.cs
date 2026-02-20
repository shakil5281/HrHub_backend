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
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees([FromQuery] CommonFilterDto filters)
        {
            try
            {
                var query = _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Section)
                    .Include(e => e.Designation)
                    .Include(e => e.Line)
                    .Include(e => e.Shift)
                    .Include(e => e.Group)
                    .Include(e => e.Floor)
                    .Include(e => e.Company)
                    .AsQueryable();

                // Applying filters
                if (!string.IsNullOrEmpty(filters.CompanyName))
                    query = query.Where(e =>
                        e.Company != null
                            ? e.Company.CompanyNameEn == filters.CompanyName
                            : e.CompanyName == filters.CompanyName);

                if (filters.CompanyId.HasValue && filters.CompanyId > 0)
                    query = query.Where(e => e.CompanyId == filters.CompanyId);

                if (filters.DepartmentId.HasValue)
                    query = query.Where(e => e.DepartmentId == filters.DepartmentId);

                if (filters.SectionId.HasValue)
                    query = query.Where(e => e.SectionId == filters.SectionId);

                if (filters.DesignationId.HasValue)
                    query = query.Where(e => e.DesignationId == filters.DesignationId);

                if (filters.LineId.HasValue)
                    query = query.Where(e => e.LineId == filters.LineId);

                if (filters.ShiftId.HasValue)
                    query = query.Where(e => e.ShiftId == filters.ShiftId);

                if (filters.GroupId.HasValue)
                    query = query.Where(e => e.GroupId == filters.GroupId);

                if (filters.FloorId.HasValue)
                    query = query.Where(e => e.FloorId == filters.FloorId);

                if (!string.IsNullOrEmpty(filters.Status))
                    query = query.Where(e => e.Status == filters.Status);

                if (filters.IsActive.HasValue)
                    query = query.Where(e => e.IsActive == filters.IsActive.Value);

                if (!string.IsNullOrEmpty(filters.EmployeeId))
                    query = query.Where(e => e.EmployeeId.Contains(filters.EmployeeId));

                if (filters.JoinDateFrom.HasValue)
                    query = query.Where(e => e.JoinDate >= filters.JoinDateFrom.Value);

                if (filters.JoinDateTo.HasValue)
                    query = query.Where(e => e.JoinDate <= filters.JoinDateTo.Value);

                if (!string.IsNullOrEmpty(filters.Gender))
                    query = query.Where(e => e.Gender == filters.Gender);

                if (!string.IsNullOrEmpty(filters.Religion))
                    query = query.Where(e => e.Religion == filters.Religion);

                if (!string.IsNullOrEmpty(filters.SearchTerm))
                {
                    query = query.Where(e =>
                        e.EmployeeId.Contains(filters.SearchTerm) ||
                        e.FullNameEn.Contains(filters.SearchTerm) ||
                        (e.FullNameBn != null && e.FullNameBn.Contains(filters.SearchTerm)) ||
                        (e.PhoneNumber != null && e.PhoneNumber.Contains(filters.SearchTerm))
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
                        Nid = e.Nid,
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
                        PresentAddressBn = e.PresentAddressBn,
                        PresentDivisionId = e.PresentDivisionId,
                        PresentDistrictId = e.PresentDistrictId,
                        PresentThanaId = e.PresentThanaId,
                        PresentPostOfficeId = e.PresentPostOfficeId,
                        PresentPostalCode = e.PresentPostalCode,
                        PermanentAddress = e.PermanentAddress,
                        PermanentAddressBn = e.PermanentAddressBn,
                        PermanentDivisionId = e.PermanentDivisionId,
                        PermanentDistrictId = e.PermanentDistrictId,
                        PermanentThanaId = e.PermanentThanaId,
                        PermanentPostOfficeId = e.PermanentPostOfficeId,
                        PermanentPostalCode = e.PermanentPostalCode,
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
                        CompanyId = e.CompanyId,
                        CompanyName = e.Company != null ? e.Company.CompanyNameEn : e.CompanyName,
                        BloodGroup = e.BloodGroup,
                        IsActive = e.IsActive,
                        IsOtEnabled = e.IsOtEnabled,
                        CreatedAt = e.CreatedAt
                    })
                    .ToListAsync();

                return Ok(employees);
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using a logger)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/employee/simple
        [HttpGet("simple")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer)]
        public async Task<ActionResult<IEnumerable<EmployeeSimpleDto>>> GetEmployeesSimple(
            [FromQuery] int? departmentId,
            [FromQuery] int? sectionId,
            [FromQuery] int? designationId,
            [FromQuery] int? lineId,
            [FromQuery] int? shiftId,
            [FromQuery] int? groupId,
            [FromQuery] int? floorId,
            [FromQuery] string? status,
            [FromQuery] bool? isActive,
            [FromQuery] string? searchTerm,
            [FromQuery] string? companyName)
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

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e =>
                    e.EmployeeId.Contains(searchTerm) ||
                    e.FullNameEn.Contains(searchTerm) ||
                    (e.FullNameBn != null && e.FullNameBn.Contains(searchTerm))
                );
            }

            if (!string.IsNullOrEmpty(companyName))
                query = query.Where(e => e.CompanyName == companyName);

            var employees = await query
                .OrderBy(e => e.EmployeeId)
                .Select(e => new EmployeeSimpleDto
                {
                    Id = e.Id,
                    EmployeeId = e.EmployeeId,
                    FullNameEn = e.FullNameEn,
                    CompanyId = e.CompanyId,
                    CompanyName = e.Company != null ? e.Company.CompanyNameEn : e.CompanyName,
                    DepartmentName = e.Department != null ? e.Department.NameEn : null,
                    DesignationName = e.Designation != null ? e.Designation.NameEn : null,
                    SectionName = e.Section != null ? e.Section.NameEn : null,
                    LineName = e.Line != null ? e.Line.NameEn : null,
                    Gender = e.Gender,
                    Religion = e.Religion,
                    ShiftName = e.Shift != null ? e.Shift.NameEn : null,
                    Status = e.Status,
                    GroupName = e.Group != null ? e.Group.NameEn : null,
                    FloorName = e.Floor != null ? e.Floor.NameEn : null
                })
                .ToListAsync();

            return Ok(employees);
        }

        // GET: api/employee/{employeeId}
        [HttpGet("{employeeId}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer)]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(string employeeId, [FromQuery] int companyId)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Section)
                .Include(e => e.Designation)
                .Include(e => e.Line)
                .Include(e => e.Shift)
                .Include(e => e.Group)
                .Include(e => e.Floor)
                .Include(e => e.Company)
                .Where(e => e.EmployeeId == employeeId && e.CompanyId == companyId)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    EmployeeId = e.EmployeeId,
                    FullNameEn = e.FullNameEn,
                    FullNameBn = e.FullNameBn,
                    Nid = e.Nid,
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
                    PresentAddressBn = e.PresentAddressBn,
                    PresentDivisionId = e.PresentDivisionId,
                    PresentDistrictId = e.PresentDistrictId,
                    PresentThanaId = e.PresentThanaId,
                    PresentPostOfficeId = e.PresentPostOfficeId,
                    PresentPostalCode = e.PresentPostalCode,
                    PermanentAddress = e.PermanentAddress,
                    PermanentAddressBn = e.PermanentAddressBn,
                    PermanentDivisionId = e.PermanentDivisionId,
                    PermanentDistrictId = e.PermanentDistrictId,
                    PermanentThanaId = e.PermanentThanaId,
                    PermanentPostOfficeId = e.PermanentPostOfficeId,
                    PermanentPostalCode = e.PermanentPostalCode,
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
                    CompanyName = e.CompanyName,
                    BloodGroup = e.BloodGroup,
                    IsActive = e.IsActive,
                    IsOtEnabled = e.IsOtEnabled,
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
            // Resolve CompanyId if CompanyName is provided but CompanyId is not
            int? companyId = dto.CompanyId;
            if (!companyId.HasValue && !string.IsNullOrEmpty(dto.CompanyName))
            {
                var company = await _context.Companies
                    .FirstOrDefaultAsync(c => c.CompanyNameEn == dto.CompanyName);
                if (company != null)
                {
                    companyId = company.Id;
                }
            }

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
                // Check if Employee ID already exists in the same company
                var exists = await _context.Employees.AnyAsync(e =>
                    e.EmployeeId == employeeId && e.CompanyId == companyId);
                if (exists)
                {
                    var companyNameForError = companyId.HasValue
                        ? (await _context.Companies.FindAsync(companyId.Value))?.CompanyNameEn
                        : "Default";
                    return BadRequest(new
                    {
                        message =
                            $"Employee ID '{employeeId}' already exists in company '{companyNameForError ?? "Default"}'."
                    });
                }
            }

            // Check if Proximity (Card ID) already exists in the same company
            if (!string.IsNullOrWhiteSpace(dto.Proximity))
            {
                var proximityExists = await _context.Employees.AnyAsync(e =>
                    e.Proximity == dto.Proximity && e.CompanyId == companyId);
                if (proximityExists)
                {
                    var companyNameForError = companyId.HasValue
                        ? (await _context.Companies.FindAsync(companyId.Value))?.CompanyNameEn
                        : "Default";
                    return BadRequest(new
                    {
                        message =
                            $"Proximity (Card ID) '{dto.Proximity}' is already assigned to another employee in company '{companyNameForError ?? "Default"}'."
                    });
                }
            }

            var employee = new Employee
            {
                EmployeeId = employeeId,
                FullNameEn = dto.FullNameEn,
                FullNameBn = dto.FullNameBn,
                Nid = dto.Nid,
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
                PresentAddressBn = dto.PresentAddressBn,
                PresentDivisionId = dto.PresentDivisionId,
                PresentDistrictId = dto.PresentDistrictId,
                PresentThanaId = dto.PresentThanaId,
                PresentPostOfficeId = dto.PresentPostOfficeId,
                PresentPostalCode = dto.PresentPostalCode,
                PermanentAddress = dto.PermanentAddress,
                PermanentAddressBn = dto.PermanentAddressBn,
                PermanentDivisionId = dto.PermanentDivisionId,
                PermanentDistrictId = dto.PermanentDistrictId,
                PermanentThanaId = dto.PermanentThanaId,
                PermanentPostOfficeId = dto.PermanentPostOfficeId,
                PermanentPostalCode = dto.PermanentPostalCode,
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
                CompanyId = companyId,
                CompanyName = dto.CompanyName,
                BloodGroup = dto.BloodGroup,

                IsActive = true,
                IsOtEnabled = dto.IsOtEnabled,
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
                    Nid = e.Nid,
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
                    PresentAddressBn = e.PresentAddressBn,
                    PresentDivisionId = e.PresentDivisionId,
                    PresentDistrictId = e.PresentDistrictId,
                    PresentThanaId = e.PresentThanaId,
                    PresentPostOfficeId = e.PresentPostOfficeId,
                    PresentPostalCode = e.PresentPostalCode,
                    PermanentAddress = e.PermanentAddress,
                    PermanentAddressBn = e.PermanentAddressBn,
                    PermanentDivisionId = e.PermanentDivisionId,
                    PermanentDistrictId = e.PermanentDistrictId,
                    PermanentThanaId = e.PermanentThanaId,
                    PermanentPostOfficeId = e.PermanentPostOfficeId,
                    PermanentPostalCode = e.PermanentPostalCode,
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
                    CompanyName = e.CompanyName,
                    BloodGroup = e.BloodGroup,
                    IsActive = e.IsActive,
                    IsOtEnabled = e.IsOtEnabled,
                    CreatedAt = e.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (createdEmployee == null)
                return StatusCode(500, new { message = "Failed to retrieve created employee details." });

            return CreatedAtAction(nameof(GetEmployee),
                new { employeeId = createdEmployee.EmployeeId, companyId = createdEmployee.CompanyId },
                createdEmployee);
        }

        // PUT: api/employee/{employeeId}
        [HttpPut("{employeeId}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager)]
        public async Task<IActionResult> UpdateEmployee(string employeeId, [FromBody] UpdateEmployeeDto dto,
            [FromQuery] int companyId)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId);

            if (employee == null)
                return NotFound();

            // Resolve CompanyId if CompanyName is provided but CompanyId is not
            // Prioritize companyId from query if provided, otherwise use dto.CompanyId
            int? effectiveCompanyId = companyId > 0 ? companyId : dto.CompanyId;

            if (!effectiveCompanyId.HasValue && !string.IsNullOrEmpty(dto.CompanyName))
            {
                var company = await _context.Companies
                    .FirstOrDefaultAsync(c => c.CompanyNameEn == dto.CompanyName);
                if (company != null)
                {
                    effectiveCompanyId = company.Id;
                }
            }

            employee.FullNameEn = dto.FullNameEn;
            employee.FullNameBn = dto.FullNameBn;
            employee.Nid = dto.Nid;
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
            employee.PresentAddressBn = dto.PresentAddressBn;
            employee.PresentDivisionId = dto.PresentDivisionId;
            employee.PresentDistrictId = dto.PresentDistrictId;
            employee.PresentThanaId = dto.PresentThanaId;
            employee.PresentPostOfficeId = dto.PresentPostOfficeId;
            employee.PresentPostalCode = dto.PresentPostalCode;
            employee.PermanentAddress = dto.PermanentAddress;
            employee.PermanentAddressBn = dto.PermanentAddressBn;
            employee.PermanentDivisionId = dto.PermanentDivisionId;
            employee.PermanentDistrictId = dto.PermanentDistrictId;
            employee.PermanentThanaId = dto.PermanentThanaId;
            employee.PermanentPostOfficeId = dto.PermanentPostOfficeId;
            employee.PermanentPostalCode = dto.PermanentPostalCode;
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
            employee.CompanyId = effectiveCompanyId;
            employee.CompanyName = dto.CompanyName;
            employee.BloodGroup = dto.BloodGroup;

            employee.IsActive = dto.IsActive;
            employee.IsOtEnabled = dto.IsOtEnabled;
            employee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/employee/{employeeId}
        [HttpDelete("{employeeId}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin)]
        public async Task<IActionResult> DeleteEmployee(string employeeId, [FromQuery] int companyId)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId);

            if (employee == null)
                return NotFound();

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/employee/mini
        [HttpGet("mini")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer)]
        public async Task<ActionResult<IEnumerable<EmployeeMiniDto>>> GetEmployeesMini(
            [FromQuery] int? departmentId,
            [FromQuery] int? sectionId,
            [FromQuery] int? designationId,
            [FromQuery] int? lineId,
            [FromQuery] int? shiftId,
            [FromQuery] int? groupId,
            [FromQuery] string? searchTerm)
        {
            var query = _context.Employees
                .Where(e => e.IsActive)
                .AsQueryable();

            if (departmentId.HasValue) query = query.Where(e => e.DepartmentId == departmentId.Value);
            if (sectionId.HasValue) query = query.Where(e => e.SectionId == sectionId.Value);
            if (designationId.HasValue) query = query.Where(e => e.DesignationId == designationId.Value);
            if (lineId.HasValue) query = query.Where(e => e.LineId == lineId.Value);
            if (shiftId.HasValue) query = query.Where(e => e.ShiftId == shiftId.Value);
            if (groupId.HasValue) query = query.Where(e => e.GroupId == groupId.Value);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e =>
                    e.EmployeeId.Contains(searchTerm) ||
                    e.FullNameEn.Contains(searchTerm)
                );
            }

            var employees = await query
                .Select(e => new EmployeeMiniDto
                {
                    Id = e.Id,
                    EmployeeId = e.EmployeeId,
                    FullNameEn = e.FullNameEn,
                    DepartmentName = e.Department != null ? e.Department.NameEn : null,
                    SectionName = e.Section != null ? e.Section.NameEn : null,
                    DesignationName = e.Designation != null ? e.Designation.NameEn : null,
                    LineName = e.Line != null ? e.Line.NameEn : null,
                    ShiftName = e.Shift != null ? e.Shift.NameEn : null,
                    GroupName = e.Group != null ? e.Group.NameEn : null
                })
                .ToListAsync();

            return Ok(employees);
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
                             (e.Nid != null && e.Nid.Contains(query)) ||
                             (e.Email != null && e.Email.Contains(query))))
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    EmployeeId = e.EmployeeId,
                    FullNameEn = e.FullNameEn,
                    FullNameBn = e.FullNameBn,
                    Nid = e.Nid,
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
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer)]
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
            [FromQuery] string? searchTerm,
            [FromQuery] string? companyName,
            [FromQuery] DateTime? joinDateFrom,
            [FromQuery] DateTime? joinDateTo,
            [FromQuery] string? gender,
            [FromQuery] string? religion)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Section)
                .Include(e => e.Designation)
                .Include(e => e.Line)
                .Include(e => e.Shift)
                .Include(e => e.Group)
                .Include(e => e.Floor)
                .Include(e => e.Company)
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

            if (!string.IsNullOrEmpty(companyName))
                query = query.Where(e => e.CompanyName == companyName);

            if (joinDateFrom.HasValue)
                query = query.Where(e => e.JoinDate >= joinDateFrom.Value);

            if (joinDateTo.HasValue)
                query = query.Where(e => e.JoinDate <= joinDateTo.Value);

            if (!string.IsNullOrEmpty(gender))
                query = query.Where(e => e.Gender == gender);

            if (!string.IsNullOrEmpty(religion))
                query = query.Where(e => e.Religion == religion);

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
                "SL", "ID", "Name (EN)", "Company", "Blood Group", "Designation", "Department", "Section",
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
                worksheet.Cells[row, 4].Value = emp.Company?.CompanyNameEn ?? emp.CompanyName;
                worksheet.Cells[row, 5].Value = emp.BloodGroup;
                worksheet.Cells[row, 6].Value = emp.Designation?.NameEn;
                worksheet.Cells[row, 7].Value = emp.Department?.NameEn;
                worksheet.Cells[row, 8].Value = emp.Section?.NameEn;
                worksheet.Cells[row, 9].Value = emp.Line?.NameEn;
                worksheet.Cells[row, 10].Value = emp.Shift?.NameEn;
                worksheet.Cells[row, 11].Value = emp.Status;
                worksheet.Cells[row, 12].Value = emp.JoinDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 13].Value = emp.PhoneNumber;
                worksheet.Cells[row, 14].Value = emp.Email;
                worksheet.Cells[row, 15].Value = emp.GrossSalary;
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
            worksheet.Cells[1, 1].Value = "SL";
            worksheet.Cells[1, 2].Value = "Employee ID";
            worksheet.Cells[1, 3].Value = "Card ID (Proximity)";
            worksheet.Cells[1, 4].Value = "Full Name (EN)";
            worksheet.Cells[1, 5].Value = "Full Name (BN)";
            worksheet.Cells[1, 6].Value = "Company Name";
            worksheet.Cells[1, 7].Value = "Blood Group";
            worksheet.Cells[1, 8].Value = "NID";
            worksheet.Cells[1, 9].Value = "Date of Birth";
            worksheet.Cells[1, 10].Value = "Gender";
            worksheet.Cells[1, 11].Value = "Religion";
            worksheet.Cells[1, 12].Value = "Department";
            worksheet.Cells[1, 13].Value = "Section";
            worksheet.Cells[1, 14].Value = "Designation";
            worksheet.Cells[1, 15].Value = "Line";
            worksheet.Cells[1, 16].Value = "Status";
            worksheet.Cells[1, 17].Value = "Join Date";
            worksheet.Cells[1, 18].Value = "Email";
            worksheet.Cells[1, 19].Value = "Phone Number";

            // Present Address
            worksheet.Cells[1, 20].Value = "Present Address (EN)";
            worksheet.Cells[1, 21].Value = "Present Address (BN)";
            worksheet.Cells[1, 22].Value = "Present Division";
            worksheet.Cells[1, 23].Value = "Present District";
            worksheet.Cells[1, 24].Value = "Present Thana";
            worksheet.Cells[1, 25].Value = "Present Post Office";
            worksheet.Cells[1, 26].Value = "Present Postal Code";

            // Permanent Address
            worksheet.Cells[1, 27].Value = "Permanent Address (EN)";
            worksheet.Cells[1, 28].Value = "Permanent Address (BN)";
            worksheet.Cells[1, 29].Value = "Permanent Division";
            worksheet.Cells[1, 30].Value = "Permanent District";
            worksheet.Cells[1, 31].Value = "Permanent Thana";
            worksheet.Cells[1, 32].Value = "Permanent Post Office";
            worksheet.Cells[1, 33].Value = "Permanent Postal Code";

            // Family Info
            worksheet.Cells[1, 34].Value = "Father Name (EN)";
            worksheet.Cells[1, 35].Value = "Father Name (BN)";
            worksheet.Cells[1, 36].Value = "Mother Name (EN)";
            worksheet.Cells[1, 37].Value = "Mother Name (BN)";
            worksheet.Cells[1, 38].Value = "Marital Status";
            worksheet.Cells[1, 39].Value = "Spouse Name (EN)";
            worksheet.Cells[1, 40].Value = "Spouse Name (BN)";
            worksheet.Cells[1, 41].Value = "Spouse Occupation";
            worksheet.Cells[1, 42].Value = "Spouse Contact";

            // Salary Info
            worksheet.Cells[1, 43].Value = "Gross Salary";
            worksheet.Cells[1, 44].Value = "Basic Salary (Auto)";
            worksheet.Cells[1, 45].Value = "House Rent (Auto)";
            worksheet.Cells[1, 46].Value = "Medical (Auto)";
            worksheet.Cells[1, 47].Value = "Conveyance (Auto)";
            worksheet.Cells[1, 48].Value = "Food (Auto)";
            worksheet.Cells[1, 49].Value = "Other (Auto)";

            // Account Info
            worksheet.Cells[1, 50].Value = "Bank Name";
            worksheet.Cells[1, 51].Value = "Bank Branch";
            worksheet.Cells[1, 52].Value = "Account No";
            worksheet.Cells[1, 53].Value = "Routing No";
            worksheet.Cells[1, 54].Value = "Account Type";

            // Emergency Contact
            worksheet.Cells[1, 55].Value = "Emergency Name";
            worksheet.Cells[1, 56].Value = "Emergency Relation";
            worksheet.Cells[1, 57].Value = "Emergency Phone";
            worksheet.Cells[1, 58].Value = "Emergency Address";

            // Extras
            worksheet.Cells[1, 59].Value = "Shift";
            worksheet.Cells[1, 60].Value = "Group";
            worksheet.Cells[1, 61].Value = "Floor";
            worksheet.Cells[1, 62].Value = "OT Status";

            using (var range = worksheet.Cells[1, 1, 1, 62])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

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
            worksheet.Cells[1, 1].Value = "SL";
            worksheet.Cells[1, 2].Value = "Employee ID";
            worksheet.Cells[1, 3].Value = "Card ID (Proximity)";
            worksheet.Cells[1, 4].Value = "Full Name (EN)";
            worksheet.Cells[1, 5].Value = "Full Name (BN)";
            worksheet.Cells[1, 6].Value = "Company Name";
            worksheet.Cells[1, 7].Value = "Blood Group";
            worksheet.Cells[1, 8].Value = "NID";
            worksheet.Cells[1, 9].Value = "Date of Birth";
            worksheet.Cells[1, 10].Value = "Gender";
            worksheet.Cells[1, 11].Value = "Religion";
            worksheet.Cells[1, 12].Value = "Department";
            worksheet.Cells[1, 13].Value = "Section";
            worksheet.Cells[1, 14].Value = "Designation";
            worksheet.Cells[1, 15].Value = "Line";
            worksheet.Cells[1, 16].Value = "Status";
            worksheet.Cells[1, 17].Value = "Join Date";
            worksheet.Cells[1, 18].Value = "Email";
            worksheet.Cells[1, 19].Value = "Phone Number";

            // Present Address (20-26)
            worksheet.Cells[1, 20].Value = "Present Address (EN)";
            worksheet.Cells[1, 21].Value = "Present Address (BN)";
            worksheet.Cells[1, 22].Value = "Present Division";
            worksheet.Cells[1, 23].Value = "Present District";
            worksheet.Cells[1, 24].Value = "Present Thana";
            worksheet.Cells[1, 25].Value = "Present Post Office";
            worksheet.Cells[1, 26].Value = "Present Postal Code";

            // Permanent Address (27-33)
            worksheet.Cells[1, 27].Value = "Permanent Address (EN)";
            worksheet.Cells[1, 28].Value = "Permanent Address (BN)";
            worksheet.Cells[1, 29].Value = "Permanent Division";
            worksheet.Cells[1, 30].Value = "Permanent District";
            worksheet.Cells[1, 31].Value = "Permanent Thana";
            worksheet.Cells[1, 32].Value = "Permanent Post Office";
            worksheet.Cells[1, 33].Value = "Permanent Postal Code";

            // Family Info (34-42)
            worksheet.Cells[1, 34].Value = "Father Name (EN)";
            worksheet.Cells[1, 35].Value = "Father Name (BN)";
            worksheet.Cells[1, 36].Value = "Mother Name (EN)";
            worksheet.Cells[1, 37].Value = "Mother Name (BN)";
            worksheet.Cells[1, 38].Value = "Marital Status";
            worksheet.Cells[1, 39].Value = "Spouse Name (EN)";
            worksheet.Cells[1, 40].Value = "Spouse Name (BN)";
            worksheet.Cells[1, 41].Value = "Spouse Occupation";
            worksheet.Cells[1, 42].Value = "Spouse Contact";

            // Salary Info (43-49)
            worksheet.Cells[1, 43].Value = "Gross Salary";
            worksheet.Cells[1, 44].Value = "Basic Salary (Auto)";
            worksheet.Cells[1, 45].Value = "House Rent (Auto)";
            worksheet.Cells[1, 46].Value = "Medical (Auto)";
            worksheet.Cells[1, 47].Value = "Conveyance (Auto)";
            worksheet.Cells[1, 48].Value = "Food (Auto)";
            worksheet.Cells[1, 49].Value = "Other (Auto)";

            // Account Info (50-54)
            worksheet.Cells[1, 50].Value = "Bank Name";
            worksheet.Cells[1, 51].Value = "Bank Branch";
            worksheet.Cells[1, 52].Value = "Account No";
            worksheet.Cells[1, 53].Value = "Routing No";
            worksheet.Cells[1, 54].Value = "Account Type";

            // Emergency Contact (55-58)
            worksheet.Cells[1, 55].Value = "Emergency Name";
            worksheet.Cells[1, 56].Value = "Emergency Relation";
            worksheet.Cells[1, 57].Value = "Emergency Phone";
            worksheet.Cells[1, 58].Value = "Emergency Address";

            // Extras (59-62)
            worksheet.Cells[1, 59].Value = "Shift";
            worksheet.Cells[1, 60].Value = "Group";
            worksheet.Cells[1, 61].Value = "Floor";
            worksheet.Cells[1, 62].Value = "OT Status";

            using (var range = worksheet.Cells[1, 1, 1, 62])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Demo data
            int row = 2;
            var demoEmployees = new[]
            {
                new
                {
                    Id = "EMP001", Proximity = "1001", NameEn = "John Doe", NameBn = "জন ডো", Company = "HRHub Ltd",
                    Blood = "O+", Nid = "1234567890123", Dob = "1990-01-15", Gender = "Male", Religion = "Islam",
                    Dept = "IT", Sec = "Software", Desig = "Senior Developer", Line = "L-101", Status = "Active",
                    Join = "2020-01-01", Email = "john.doe@hrhub.com", Phone = "01700000001",
                    PreAddr = "House 12, Road 5, Dhanmondi", PreAddrBn = "বাড়ি ১২, রোড ৫, ধানমন্ডি", PreDiv = "Dhaka",
                    PreDist = "Dhaka", PreThana = "Dhanmondi", PrePo = "Dhanmondi", PrePc = "1209",
                    PerAddr = "Village X, Thana Y", PerAddrBn = "গ্রাম ক, থানা খ", PerDiv = "Chattogram",
                    PerDist = "Comilla", PerThana = "Chauddagram", PerPo = "Miah Bazar", PerPc = "3500",
                    Father = "Robert Doe", FatherBn = "রবার্ট ডো", Mother = "Jane Doe", MotherBn = "জেন ডো",
                    Marital = "Married",
                    Spouse = "Alice Doe", SpouseBn = "এলিস ডো", SpouseOcc = "Teacher", SpousePh = "01700000002",
                    Gross = 85000, Bank = "Dutch Bangla Bank", Branch = "Dhanmondi", Acc = "123.101.5555",
                    Route = "123456789", Type = "Savings",
                    EmName = "Alice Doe", EmRel = "Wife", EmPh = "01700000002", EmAddr = "Same as Present",
                    Shift = "General", Group = "A", Floor = "5th Floor", Ot = "No"
                },
                new
                {
                    Id = "EMP002", Proximity = "1002", NameEn = "Rahim Uddin", NameBn = "রহিম উদ্দিন",
                    Company = "HRHub Ltd",
                    Blood = "B+", Nid = "9876543210987", Dob = "1992-05-20", Gender = "Male", Religion = "Islam",
                    Dept = "HR", Sec = "Recruitment", Desig = "HR Executive", Line = "L-102", Status = "Active",
                    Join = "2021-03-15", Email = "rahim.u@hrhub.com", Phone = "01800000001",
                    PreAddr = "Flat 4A, Green Road", PreAddrBn = "ফ্ল্যাট ৪এ, গ্রীন রোড", PreDiv = "Dhaka",
                    PreDist = "Dhaka", PreThana = "Tejgaon", PrePo = "Tejgaon", PrePc = "1215",
                    PerAddr = "Village Z, Thana W", PerAddrBn = "গ্রাম গ, থানা ঘ", PerDiv = "Rajshahi",
                    PerDist = "Bogra", PerThana = "Sadar", PerPo = "Sadar", PerPc = "5800",
                    Father = "Karim Uddin", FatherBn = "করিম উদ্দিন", Mother = "Fatema Begum", MotherBn = "ফাতেমা বেগম",
                    Marital = "Single",
                    Spouse = "", SpouseBn = "", SpouseOcc = "", SpousePh = "",
                    Gross = 45000, Bank = "Brac Bank", Branch = "Gulshan", Acc = "222.333.4444", Route = "987654321",
                    Type = "Salary",
                    EmName = "Karim Uddin", EmRel = "Father", EmPh = "01800000002", EmAddr = "Village Z, Bogra",
                    Shift = "Morning", Group = "B", Floor = "2nd Floor", Ot = "Yes"
                },
                new
                {
                    Id = "EMP003", Proximity = "1003", NameEn = "Sarah Khan", NameBn = "সারা খান",
                    Company = "HRHub Ltd",
                    Blood = "A-", Nid = "5555666677778", Dob = "1995-12-10", Gender = "Female", Religion = "Islam",
                    Dept = "Accounts", Sec = "Payroll", Desig = "Accountant", Line = "L-103", Status = "Active",
                    Join = "2022-06-01", Email = "sarah.k@hrhub.com", Phone = "01900000001",
                    PreAddr = "House 50, Uttara Sec 7", PreAddrBn = "বাড়ি ৫০, উত্তরা সেক্টর ৭", PreDiv = "Dhaka",
                    PreDist = "Dhaka", PreThana = "Uttara West", PrePo = "Uttara", PrePc = "1230",
                    PerAddr = "Sylhet Sadar", PerAddrBn = "সিলেট সদর", PerDiv = "Sylhet", PerDist = "Sylhet",
                    PerThana = "Sadar", PerPo = "Sadar", PerPc = "3100",
                    Father = "Kabir Khan", FatherBn = "কবির খান", Mother = "Salma Khan", MotherBn = "সালমা খান",
                    Marital = "Married",
                    Spouse = "Imran Ahmed", SpouseBn = "ইমরান আহমেদ", SpouseOcc = "Banker", SpousePh = "01900000002",
                    Gross = 55000, Bank = "City Bank", Branch = "Uttara", Acc = "111.222.3333", Route = "112233445",
                    Type = "Savings",
                    EmName = "Imran Ahmed", EmRel = "Husband", EmPh = "01900000002", EmAddr = "Same as Present",
                    Shift = "General", Group = "A", Floor = "3rd Floor", Ot = "No"
                }
            };

            foreach (var emp in demoEmployees)
            {
                worksheet.Cells[row, 1].Value = row - 1;
                worksheet.Cells[row, 2].Value = emp.Id;
                worksheet.Cells[row, 3].Value = emp.Proximity;
                worksheet.Cells[row, 4].Value = emp.NameEn;
                worksheet.Cells[row, 5].Value = emp.NameBn;
                worksheet.Cells[row, 6].Value = emp.Company;
                worksheet.Cells[row, 7].Value = emp.Blood;
                worksheet.Cells[row, 8].Value = emp.Nid;
                worksheet.Cells[row, 9].Value = emp.Dob;
                worksheet.Cells[row, 10].Value = emp.Gender;
                worksheet.Cells[row, 11].Value = emp.Religion;
                worksheet.Cells[row, 12].Value = emp.Dept;
                worksheet.Cells[row, 13].Value = emp.Sec;
                worksheet.Cells[row, 14].Value = emp.Desig;
                worksheet.Cells[row, 15].Value = emp.Line;
                worksheet.Cells[row, 16].Value = emp.Status;
                worksheet.Cells[row, 17].Value = emp.Join;
                worksheet.Cells[row, 18].Value = emp.Email;
                worksheet.Cells[row, 19].Value = emp.Phone;

                // Present Address
                worksheet.Cells[row, 20].Value = emp.PreAddr;
                worksheet.Cells[row, 21].Value = emp.PreAddrBn;
                worksheet.Cells[row, 22].Value = emp.PreDiv;
                worksheet.Cells[row, 23].Value = emp.PreDist;
                worksheet.Cells[row, 24].Value = emp.PreThana;
                worksheet.Cells[row, 25].Value = emp.PrePo;
                worksheet.Cells[row, 26].Value = emp.PrePc;

                // Permanent Address
                worksheet.Cells[row, 27].Value = emp.PerAddr;
                worksheet.Cells[row, 28].Value = emp.PerAddrBn;
                worksheet.Cells[row, 29].Value = emp.PerDiv;
                worksheet.Cells[row, 30].Value = emp.PerDist;
                worksheet.Cells[row, 31].Value = emp.PerThana;
                worksheet.Cells[row, 32].Value = emp.PerPo;
                worksheet.Cells[row, 33].Value = emp.PerPc;

                // Family
                worksheet.Cells[row, 34].Value = emp.Father;
                worksheet.Cells[row, 35].Value = emp.FatherBn;
                worksheet.Cells[row, 36].Value = emp.Mother;
                worksheet.Cells[row, 37].Value = emp.MotherBn;
                worksheet.Cells[row, 38].Value = emp.Marital;
                worksheet.Cells[row, 39].Value = emp.Spouse;
                worksheet.Cells[row, 40].Value = emp.SpouseBn;
                worksheet.Cells[row, 41].Value = emp.SpouseOcc;
                worksheet.Cells[row, 42].Value = emp.SpousePh;

                // Salary
                worksheet.Cells[row, 43].Value = emp.Gross;

                // Auto calc break down
                decimal gross = (decimal)emp.Gross;
                decimal medical = 750;
                decimal food = 1250;
                decimal conveyance = 450;
                decimal other = 0;
                decimal fixedTotal = medical + food + conveyance + other;
                decimal basic = 0;
                decimal houseRent = 0;

                if (gross > fixedTotal)
                {
                    basic = Math.Round((gross - fixedTotal) / 1.5m, 2);
                    houseRent = Math.Round(gross - fixedTotal - basic, 2);
                }

                worksheet.Cells[row, 44].Value = basic;
                worksheet.Cells[row, 45].Value = houseRent;
                worksheet.Cells[row, 46].Value = medical;
                worksheet.Cells[row, 47].Value = conveyance;
                worksheet.Cells[row, 48].Value = food;
                worksheet.Cells[row, 49].Value = other;

                // Bank
                worksheet.Cells[row, 50].Value = emp.Bank;
                worksheet.Cells[row, 51].Value = emp.Branch;
                worksheet.Cells[row, 52].Value = emp.Acc;
                worksheet.Cells[row, 53].Value = emp.Route;
                worksheet.Cells[row, 54].Value = emp.Type;

                // Emergency
                worksheet.Cells[row, 55].Value = emp.EmName;
                worksheet.Cells[row, 56].Value = emp.EmRel;
                worksheet.Cells[row, 57].Value = emp.EmPh;
                worksheet.Cells[row, 58].Value = emp.EmAddr;

                // Extras
                worksheet.Cells[row, 59].Value = emp.Shift;
                worksheet.Cells[row, 60].Value = emp.Group;
                worksheet.Cells[row, 61].Value = emp.Floor;
                worksheet.Cells[row, 62].Value = emp.Ot;

                row++;
            }

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
                    result.Errors.Add(new ImportErrorDto
                        { RowNumber = 0, Field = "File", Message = "No worksheet found" });
                    return BadRequest(result);
                }

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount < 2)
                {
                    result.Errors.Add(new ImportErrorDto
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

                var companies = (await _context.Companies.ToListAsync())
                    .GroupBy(x => x.CompanyNameEn, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var divisions = await _context.Divisions.ToListAsync();
                var districts = await _context.Districts.Include(d => d.Division).ToListAsync();
                var thanas = await _context.Thanas.Include(t => t.District).ToListAsync();
                var postOffices = await _context.PostOffices.Include(p => p.District).ToListAsync();

                // Existing employees for duplicate/update check
                // Fetch all employees to allow updates
                var allEmployees = await _context.Employees.ToListAsync();

                var employeeMap = allEmployees
                    .GroupBy(e => $"{e.EmployeeId?.Trim()}|{(e.CompanyName ?? "").Trim()}")
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var proximityMap = allEmployees
                    .Where(e => !string.IsNullOrEmpty(e.Proximity))
                    .GroupBy(e => $"{e.Proximity!.Trim()}|{(e.CompanyName ?? "").Trim()}")
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

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
                        var companyName = GetVal(6);
                        var bloodGroup = GetVal(7);
                        var nid = GetVal(8);
                        var dobText = GetVal(9);
                        var gender = GetVal(10);
                        var religion = GetVal(11);
                        var deptName = GetVal(12);
                        var secName = GetVal(13);
                        var desigName = GetVal(14);
                        var lineName = GetVal(15);
                        var status = GetVal(16);
                        var joinDateText = GetVal(17);
                        var email = GetVal(18);
                        var phone = GetVal(19);

                        // Present Address
                        var preAddrEn = GetVal(20);
                        var preAddrBn = GetVal(21);
                        var preDiv = GetVal(22);
                        var preDist = GetVal(23);
                        var preThana = GetVal(24);
                        var prePo = GetVal(25);
                        var prePc = GetVal(26);

                        // Permanent Address
                        var perAddrEn = GetVal(27);
                        var perAddrBn = GetVal(28);
                        var perDiv = GetVal(29);
                        var perDist = GetVal(30);
                        var perThana = GetVal(31);
                        var perPo = GetVal(32);
                        var perPc = GetVal(33);

                        // Family
                        var fNameEn = GetVal(34);
                        var fNameBn = GetVal(35);
                        var mNameEn = GetVal(36);
                        var mNameBn = GetVal(37);
                        var marital = GetVal(38);
                        var sNameEn = GetVal(39);
                        var sNameBn = GetVal(40);
                        var sOcc = GetVal(41);
                        var sCont = GetVal(42);

                        // Salary
                        var grossText = GetVal(43);

                        // Account
                        var bankName = GetVal(50);
                        var bankBranch = GetVal(51);
                        var accNo = GetVal(52);
                        var routeNo = GetVal(53);
                        var accType = GetVal(54);

                        // Emergency
                        var emName = GetVal(55);
                        var emRel = GetVal(56);
                        var emPhone = GetVal(57);
                        var emAddr = GetVal(58);

                        // Extras
                        var shiftName = GetVal(59);
                        var groupName = GetVal(60);
                        var floorName = GetVal(61);
                        var otStatusVal = GetVal(62);

                        // Validation
                        if (string.IsNullOrEmpty(fullNameEn))
                        {
                            result.Errors.Add(new ImportErrorDto
                                { RowNumber = row, Field = "Full Name (EN)", Message = "Required field" });
                            result.ErrorCount++;
                            continue;
                        }

                        Department? department;
                        if (departments.ContainsKey(deptName))
                        {
                            department = departments[deptName];
                        }
                        else
                        {
                            result.Warnings.Add(new ImportWarningDto
                            {
                                RowNumber = row, Field = "Department",
                                Message = $"Department '{deptName}' not found. Defaulting to first available."
                            });
                            result.WarningCount++;
                            department = departments.Values.FirstOrDefault();
                        }

                        if (department == null)
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                RowNumber = row, Field = "Department",
                                Message = "No departments found in system. Please create departments first."
                            });
                            result.ErrorCount++;
                            continue;
                        }

                        if (!designations.ContainsKey(desigName))
                        {
                            result.Errors.Add(new ImportErrorDto
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

                        int? ResolvePo(string name, int? distId) => postOffices.FirstOrDefault(p =>
                            p.NameEn.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                            (!distId.HasValue || p.DistrictId == distId))?.Id;

                        // Present
                        int? preDivId = string.IsNullOrEmpty(preDiv) ? null : ResolveDiv(preDiv);
                        int? preDistId = string.IsNullOrEmpty(preDist) ? null : ResolveDist(preDist, preDivId);
                        int? preThanaId = string.IsNullOrEmpty(preThana) ? null : ResolveThana(preThana, preDistId);
                        int? prePoId = string.IsNullOrEmpty(prePo) ? null : ResolvePo(prePo, preDistId);

                        string prePcFinal = prePc;
                        if (string.IsNullOrEmpty(prePcFinal) && prePoId.HasValue)
                        {
                            var po = postOffices.FirstOrDefault(p => p.Id == prePoId);
                            if (po != null) prePcFinal = po.Code;
                        }

                        // Permanent
                        int? perDivId = string.IsNullOrEmpty(perDiv) ? null : ResolveDiv(perDiv);
                        int? perDistId = string.IsNullOrEmpty(perDist) ? null : ResolveDist(perDist, perDivId);
                        int? perThanaId = string.IsNullOrEmpty(perThana) ? null : ResolveThana(perThana, perDistId);
                        int? perPoId = string.IsNullOrEmpty(perPo) ? null : ResolvePo(perPo, perDistId);
                        string perPcFinal = perPc;
                        if (string.IsNullOrEmpty(perPcFinal) && perPoId.HasValue)
                        {
                            var po = postOffices.FirstOrDefault(p => p.Id == perPoId);
                            if (po != null) perPcFinal = po.Code;
                        }

                        // Salary Logic
                        decimal gross = 0;
                        if (decimal.TryParse(grossText, out var parsedGross))
                        {
                            gross = parsedGross;
                        }

                        decimal basic = 0, houseRent = 0, medical = 0, food = 0, conveyance = 0, other = 0;

                        if (gross > 0)
                        {
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
                            else
                            {
                                // Handle edge case where gross is small
                                basic = gross * 0.6m;
                                houseRent = gross * 0.4m;
                                medical = 0;
                                food = 0;
                                conveyance = 0;
                            }

                            basic = Math.Round(basic, 2);
                            houseRent = Math.Round(houseRent, 2);
                        }

                        int? shiftId = !string.IsNullOrEmpty(shiftName) && shifts.TryGetValue(shiftName, out var sVal)
                            ? sVal.Id
                            : null;
                        int? groupId = !string.IsNullOrEmpty(groupName) && groups.TryGetValue(groupName, out var gVal)
                            ? gVal.Id
                            : null;
                        int? floorId = !string.IsNullOrEmpty(floorName) && floors.TryGetValue(floorName, out var fVal)
                            ? fVal.Id
                            : null;

                        // Check Create vs Update
                        var empKey = $"{providedEmpId}|{companyName}";
                        Employee employee;
                        bool isUpdate = false;

                        if (!string.IsNullOrEmpty(providedEmpId) &&
                            employeeMap.TryGetValue(empKey, out var existingEmp))
                        {
                            employee = existingEmp;
                            isUpdate = true;
                        }
                        else
                        {
                            employee = new Employee();
                            isUpdate = false;
                        }

                        // Check Proximity Conflict
                        if (!string.IsNullOrEmpty(proximity))
                        {
                            var proxKey = $"{proximity}|{companyName}";
                            if (proximityMap.TryGetValue(proxKey, out var proxOwner) && proxOwner != employee)
                            {
                                result.Warnings.Add(new ImportWarningDto
                                {
                                    RowNumber = row, Field = "Proximity",
                                    Message =
                                        $"Proximity (Card ID) '{proximity}' already assigned to employee '{proxOwner.EmployeeId}'. Ignoring Proximity."
                                });
                                result.WarningCount++;
                                proximity = null; // Do not assign this proximity
                            }
                        }

                        // Assign Properties
                        employee.EmployeeId = string.IsNullOrEmpty(providedEmpId) ? "TEMP" : providedEmpId;
                        employee.Proximity = proximity;
                        employee.FullNameEn = fullNameEn;
                        employee.FullNameBn = fullNameBn;

                        // Resolve Company ID from company name
                        int? companyId = null;
                        if (!string.IsNullOrEmpty(companyName) && companies.TryGetValue(companyName, out var company))
                        {
                            companyId = company.Id;
                        }
                        else if (!string.IsNullOrEmpty(companyName))
                        {
                            result.Warnings.Add(new ImportWarningDto
                            {
                                RowNumber = row, Field = "Company Name",
                                Message =
                                    $"Company '{companyName}' not found in database. Employee will be created without a company relation."
                            });
                            result.WarningCount++;
                        }

                        employee.CompanyId = companyId;
                        employee.CompanyName = companyName;
                        employee.BloodGroup = bloodGroup;
                        employee.Nid = nid;
                        employee.DateOfBirth = dob;
                        employee.Gender = gender;
                        employee.Religion = religion;
                        employee.DepartmentId = department.Id;
                        employee.SectionId = sectionId;
                        employee.DesignationId = designation.Id;
                        employee.LineId = lineId;
                        employee.ShiftId = shiftId;
                        employee.GroupId = groupId;
                        employee.FloorId = floorId;
                        employee.Status = string.IsNullOrEmpty(status) ? "Active" : status;
                        employee.JoinDate = joinDate;
                        employee.IsOtEnabled = otStatusVal.Equals("Yes", StringComparison.OrdinalIgnoreCase) ||
                                               otStatusVal.Equals("True", StringComparison.OrdinalIgnoreCase) ||
                                               otStatusVal.Equals("1");
                        employee.Email = email;
                        employee.PhoneNumber = phone;
                        employee.PresentAddress = preAddrEn;
                        employee.PresentAddressBn = preAddrBn;
                        employee.PresentDivisionId = preDivId;
                        employee.PresentDistrictId = preDistId;
                        employee.PresentThanaId = preThanaId;
                        employee.PresentPostOfficeId = prePoId;
                        employee.PresentPostalCode = prePcFinal;
                        employee.PermanentAddress = perAddrEn;
                        employee.PermanentAddressBn = perAddrBn;
                        employee.PermanentDivisionId = perDivId;
                        employee.PermanentDistrictId = perDistId;
                        employee.PermanentThanaId = perThanaId;
                        employee.PermanentPostOfficeId = perPoId;
                        employee.PermanentPostalCode = perPcFinal;
                        employee.FatherNameEn = fNameEn;
                        employee.FatherNameBn = fNameBn;
                        employee.MotherNameEn = mNameEn;
                        employee.MotherNameBn = mNameBn;
                        employee.MaritalStatus = marital;
                        employee.SpouseNameEn = sNameEn;
                        employee.SpouseNameBn = sNameBn;
                        employee.SpouseOccupation = sOcc;
                        employee.SpouseContact = sCont;
                        employee.GrossSalary = gross;
                        employee.BasicSalary = basic;
                        employee.HouseRent = houseRent;
                        employee.MedicalAllowance = medical;
                        employee.FoodAllowance = food;
                        employee.Conveyance = conveyance;
                        employee.OtherAllowance = other;
                        employee.BankName = bankName;
                        employee.BankBranchName = bankBranch;
                        employee.BankAccountNo = accNo;
                        employee.BankRoutingNo = routeNo;
                        employee.BankAccountType = accType;
                        employee.EmergencyContactName = emName;
                        employee.EmergencyContactRelation = emRel;
                        employee.EmergencyContactPhone = emPhone;
                        employee.EmergencyContactAddress = emAddr;

                        // Default Active/Created (if new)
                        if (!isUpdate)
                        {
                            employee.IsActive = true;
                            employee.CreatedAt = DateTime.UtcNow;
                            _context.Employees.Add(employee);

                            // Update local maps
                            if (!string.IsNullOrEmpty(providedEmpId)) employeeMap[empKey] = employee;
                            if (!string.IsNullOrEmpty(proximity)) proximityMap[$"{proximity}|{companyName}"] = employee;

                            result.CreatedCount++;
                        }
                        else
                        {
                            employee.UpdatedAt = DateTime.UtcNow;
                            // Proximity map update if changed? Complex to track, but acceptable to skip for this file scope.
                            result.UpdatedCount++;
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

                if (result.SuccessCount > 0)
                {
                    var addedEmployees = _context.ChangeTracker.Entries<Employee>()
                        .Where(e => e.State == EntityState.Added)
                        .Select(e => e.Entity)
                        .ToList();

                    if (addedEmployees.Any(e => e.EmployeeId == "TEMP"))
                    {
                        var maxIdStr = await _context.Employees.Where(e => e.EmployeeId.StartsWith("EMP"))
                            .Select(e => e.EmployeeId)
                            .ToListAsync();

                        int nextNumericId = 0;
                        if (maxIdStr.Count > 0)
                        {
                            nextNumericId = maxIdStr
                                .Select(s => s.Length > 3 && int.TryParse(s.Substring(3), out var i) ? i : 0)
                                .Max();
                        }

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
                result.Errors.Add(new ImportErrorDto { RowNumber = 0, Field = "File", Message = ex.Message });
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

            var webRootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads", "employees", type);

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
}
