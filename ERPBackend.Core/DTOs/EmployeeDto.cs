namespace ERPBackend.Core.DTOs
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string FullNameEn { get; set; } = string.Empty;
        public string? FullNameBn { get; set; }
        public string? NID { get; set; }
        public string? Proximity { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Religion { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int? SectionId { get; set; }
        public string? SectionName { get; set; }
        public int DesignationId { get; set; }
        public string? DesignationName { get; set; }
        public int? LineId { get; set; }
        public string? LineName { get; set; }
        public int? ShiftId { get; set; }
        public string? ShiftName { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public int? FloorId { get; set; }
        public string? FloorName { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime JoinDate { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? SignatureImageUrl { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PresentAddress { get; set; }
        public string? PresentAddressBn { get; set; }
        public int? PresentDivisionId { get; set; }
        public int? PresentDistrictId { get; set; }
        public int? PresentThanaId { get; set; }
        public int? PresentPostOfficeId { get; set; }
        public string? PresentPostalCode { get; set; }

        public string? PermanentAddress { get; set; }
        public string? PermanentAddressBn { get; set; }
        public int? PermanentDivisionId { get; set; }
        public int? PermanentDistrictId { get; set; }
        public int? PermanentThanaId { get; set; }
        public int? PermanentPostOfficeId { get; set; }
        public string? PermanentPostalCode { get; set; }

        // Family Information
        public string? FatherNameEn { get; set; }
        public string? FatherNameBn { get; set; }
        public string? MotherNameEn { get; set; }
        public string? MotherNameBn { get; set; }
        public string? MaritalStatus { get; set; }
        public string? SpouseNameEn { get; set; }
        public string? SpouseNameBn { get; set; }
        public string? SpouseOccupation { get; set; }
        public string? SpouseContact { get; set; }

        // Salary Information
        public decimal? BasicSalary { get; set; }
        public decimal? HouseRent { get; set; }
        public decimal? MedicalAllowance { get; set; }
        public decimal? Conveyance { get; set; }
        public decimal? FoodAllowance { get; set; }
        public decimal? OtherAllowance { get; set; }
        public decimal? GrossSalary { get; set; }

        // Account Information
        public string? BankName { get; set; }
        public string? BankBranchName { get; set; }
        public string? BankAccountNo { get; set; }
        public string? BankRoutingNo { get; set; }
        public string? BankAccountType { get; set; }

        // Emergency Contact Info
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactRelation { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactAddress { get; set; }

        public bool IsActive { get; set; }
        public bool IsOTEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateEmployeeDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string FullNameEn { get; set; } = string.Empty;
        public string? FullNameBn { get; set; }
        public string? NID { get; set; }
        public string? Proximity { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Religion { get; set; }
        public int DepartmentId { get; set; }
        public int? SectionId { get; set; }
        public int DesignationId { get; set; }
        public int? LineId { get; set; }
        public int? ShiftId { get; set; }
        public int? GroupId { get; set; }
        public int? FloorId { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime JoinDate { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PresentAddress { get; set; }
        public string? PresentAddressBn { get; set; }
        public int? PresentDivisionId { get; set; }
        public int? PresentDistrictId { get; set; }
        public int? PresentThanaId { get; set; }
        public int? PresentPostOfficeId { get; set; }
        public string? PresentPostalCode { get; set; }

        public string? PermanentAddress { get; set; }
        public string? PermanentAddressBn { get; set; }
        public int? PermanentDivisionId { get; set; }
        public int? PermanentDistrictId { get; set; }
        public int? PermanentThanaId { get; set; }
        public int? PermanentPostOfficeId { get; set; }
        public string? PermanentPostalCode { get; set; }

        // Family Information
        public string? FatherNameEn { get; set; }
        public string? FatherNameBn { get; set; }
        public string? MotherNameEn { get; set; }
        public string? MotherNameBn { get; set; }
        public string? MaritalStatus { get; set; }
        public string? SpouseNameEn { get; set; }
        public string? SpouseNameBn { get; set; }
        public string? SpouseOccupation { get; set; }
        public string? SpouseContact { get; set; }

        // Salary Information
        public decimal? BasicSalary { get; set; }
        public decimal? HouseRent { get; set; }
        public decimal? MedicalAllowance { get; set; }
        public decimal? Conveyance { get; set; }
        public decimal? FoodAllowance { get; set; }
        public decimal? OtherAllowance { get; set; }
        public decimal? GrossSalary { get; set; }

        // Account Information
        public string? BankName { get; set; }
        public string? BankBranchName { get; set; }
        public string? BankAccountNo { get; set; }
        public string? BankRoutingNo { get; set; }
        public string? BankAccountType { get; set; }

        // Emergency Contact Info
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactRelation { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactAddress { get; set; }
        public bool IsOTEnabled { get; set; }
    }

    public class UpdateEmployeeDto
    {
        public string FullNameEn { get; set; } = string.Empty;
        public string? FullNameBn { get; set; }
        public string? NID { get; set; }
        public string? Proximity { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Religion { get; set; }
        public int DepartmentId { get; set; }
        public int? SectionId { get; set; }
        public int DesignationId { get; set; }
        public int? LineId { get; set; }
        public int? ShiftId { get; set; }
        public int? GroupId { get; set; }
        public int? FloorId { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime JoinDate { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PresentAddress { get; set; }
        public string? PresentAddressBn { get; set; }
        public int? PresentDivisionId { get; set; }
        public int? PresentDistrictId { get; set; }
        public int? PresentThanaId { get; set; }
        public int? PresentPostOfficeId { get; set; }
        public string? PresentPostalCode { get; set; }

        public string? PermanentAddress { get; set; }
        public string? PermanentAddressBn { get; set; }
        public int? PermanentDivisionId { get; set; }
        public int? PermanentDistrictId { get; set; }
        public int? PermanentThanaId { get; set; }
        public int? PermanentPostOfficeId { get; set; }
        public string? PermanentPostalCode { get; set; }

        // Family Information
        public string? FatherNameEn { get; set; }
        public string? FatherNameBn { get; set; }
        public string? MotherNameEn { get; set; }
        public string? MotherNameBn { get; set; }
        public string? MaritalStatus { get; set; }
        public string? SpouseNameEn { get; set; }
        public string? SpouseNameBn { get; set; }
        public string? SpouseOccupation { get; set; }
        public string? SpouseContact { get; set; }

        // Salary Information
        public decimal? BasicSalary { get; set; }
        public decimal? HouseRent { get; set; }
        public decimal? MedicalAllowance { get; set; }
        public decimal? Conveyance { get; set; }
        public decimal? FoodAllowance { get; set; }
        public decimal? OtherAllowance { get; set; }
        public decimal? GrossSalary { get; set; }

        // Account Information
        public string? BankName { get; set; }
        public string? BankBranchName { get; set; }
        public string? BankAccountNo { get; set; }
        public string? BankRoutingNo { get; set; }
        public string? BankAccountType { get; set; }

        // Emergency Contact Info
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactRelation { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactAddress { get; set; }

        public bool IsActive { get; set; }
        public bool IsOTEnabled { get; set; }
    }
}
