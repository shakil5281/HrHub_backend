using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Entities;

namespace ERPBackend.Core.Models
{
    public class AdvanceSalary
    {
        [Key] public int Id { get; set; }

        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))] public virtual Employee? Employee { get; set; }

        public int? CompanyId { get; set; }
        [ForeignKey(nameof(CompanyId))] public virtual Company? Company { get; set; }

        [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }

        public DateTime RequestDate { get; set; }
        public DateTime? ApprovalDate { get; set; }

        public int RepaymentMonth { get; set; }
        public int RepaymentYear { get; set; }

        [StringLength(50)] public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [StringLength(50)] public string? Grade { get; set; }
        [StringLength(500)] public string? Remarks { get; set; }

        [Column(TypeName = "decimal(18,2)")] public decimal BasicSalary { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal HouseRent { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal MedicalAllowance { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal FoodAllowance { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal TransportAllowance { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal GrossSalary { get; set; }
        
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        
        [Column(TypeName = "decimal(18,2)")] public decimal AbsentDeduction { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal TotalPayableWages { get; set; }
        
        [Column(TypeName = "decimal(18,2)")] public decimal OTHours { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal OTRate { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal OTAmount { get; set; }
        
        [Column(TypeName = "decimal(18,2)")] public decimal NetPayable { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
