using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Models;

namespace ERPBackend.Core.Entities
{
    public class Employee
    {
        [Key] public int Id { get; set; }

        // Link to application user (optional)
        [StringLength(450)] public string? UserId { get; set; }

        [Required] [StringLength(20)] public string EmployeeId { get; set; } = string.Empty;

        [Required] [StringLength(200)] public string FullNameEn { get; set; } = string.Empty;

        [StringLength(200)] public string? FullNameBn { get; set; }

        [StringLength(50)] public string? Nid { get; set; }

        [StringLength(50)] public string? Proximity { get; set; }

        // Navigation to application user
        [ForeignKey(nameof(UserId))] public virtual ApplicationUser? User { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(20)] public string? Gender { get; set; }

        [StringLength(50)] public string? Religion { get; set; }

        [Required] public int DepartmentId { get; set; }

        public int? SectionId { get; set; }

        [Required] public int DesignationId { get; set; }

        public int? LineId { get; set; }
        public int? ShiftId { get; set; }
        public int? GroupId { get; set; }
        public int? FloorId { get; set; }

        [Required] [StringLength(50)] public string Status { get; set; } = "Active";

        [Required] public DateTime JoinDate { get; set; }

        [StringLength(500)] public string? ProfileImageUrl { get; set; }

        [StringLength(500)] public string? SignatureImageUrl { get; set; }

        [StringLength(100)] public string? Email { get; set; }

        [StringLength(20)] public string? PhoneNumber { get; set; }

        [StringLength(500)] public string? PresentAddress { get; set; }
        [StringLength(500)] public string? PresentAddressBn { get; set; }
        public int? PresentDivisionId { get; set; }
        public int? PresentDistrictId { get; set; }
        public int? PresentThanaId { get; set; }
        public int? PresentPostOfficeId { get; set; }
        [StringLength(20)] public string? PresentPostalCode { get; set; }

        [StringLength(500)] public string? PermanentAddress { get; set; }
        [StringLength(500)] public string? PermanentAddressBn { get; set; }
        public int? PermanentDivisionId { get; set; }
        public int? PermanentDistrictId { get; set; }
        public int? PermanentThanaId { get; set; }
        public int? PermanentPostOfficeId { get; set; }
        [StringLength(20)] public string? PermanentPostalCode { get; set; }

        // Family Information
        [StringLength(200)] public string? FatherNameEn { get; set; }
        [StringLength(200)] public string? FatherNameBn { get; set; }
        [StringLength(200)] public string? MotherNameEn { get; set; }
        [StringLength(200)] public string? MotherNameBn { get; set; }
        [StringLength(50)] public string? MaritalStatus { get; set; }
        [StringLength(200)] public string? SpouseNameEn { get; set; }
        [StringLength(200)] public string? SpouseNameBn { get; set; }
        [StringLength(100)] public string? SpouseOccupation { get; set; }
        [StringLength(20)] public string? SpouseContact { get; set; }

        // Salary Information
        [Column(TypeName = "decimal(18,2)")] public decimal? BasicSalary { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? HouseRent { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? MedicalAllowance { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? Conveyance { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? FoodAllowance { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? OtherAllowance { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? GrossSalary { get; set; }

        // Account Information
        [StringLength(100)] public string? BankName { get; set; }
        [StringLength(100)] public string? BankBranchName { get; set; }
        [StringLength(50)] public string? BankAccountNo { get; set; }
        [StringLength(50)] public string? BankRoutingNo { get; set; }
        [StringLength(50)] public string? BankAccountType { get; set; }

        // Emergency Contact Info
        [StringLength(200)] public string? EmergencyContactName { get; set; }
        [StringLength(100)] public string? EmergencyContactRelation { get; set; }
        [StringLength(20)] public string? EmergencyContactPhone { get; set; }
        [StringLength(500)] public string? EmergencyContactAddress { get; set; }

        [StringLength(200)] public string? CompanyName { get; set; }
        [StringLength(20)] public string? BloodGroup { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsOtEnabled { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(DepartmentId))] public virtual Department? Department { get; set; }

        [ForeignKey(nameof(SectionId))] public virtual Section? Section { get; set; }

        [ForeignKey(nameof(DesignationId))] public virtual Designation? Designation { get; set; }

        [ForeignKey(nameof(LineId))] public virtual Line? Line { get; set; }
        [ForeignKey(nameof(ShiftId))] public virtual Shift? Shift { get; set; }
        [ForeignKey(nameof(GroupId))] public virtual Group? Group { get; set; }
        [ForeignKey(nameof(FloorId))] public virtual Floor? Floor { get; set; }
    }
}
