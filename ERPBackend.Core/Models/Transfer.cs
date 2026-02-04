using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Entities;

namespace ERPBackend.Core.Models
{
    public class Transfer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;

        public int? FromDepartmentId { get; set; }
        [ForeignKey("FromDepartmentId")]
        public virtual Department? FromDepartment { get; set; }

        public int? FromDesignationId { get; set; }
        [ForeignKey("FromDesignationId")]
        public virtual Designation? FromDesignation { get; set; }

        [Required]
        public int ToDepartmentId { get; set; }
        [ForeignKey("ToDepartmentId")]
        public virtual Department ToDepartment { get; set; } = null!;

        [Required]
        public int ToDesignationId { get; set; }
        [ForeignKey("ToDesignationId")]
        public virtual Designation ToDesignation { get; set; } = null!;

        [Required]
        public DateTime TransferDate { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = null!;

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [StringLength(500)]
        public string? AdminRemark { get; set; }

        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
    }
}
