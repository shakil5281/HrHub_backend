using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Models;

namespace ERPBackend.Core.Models
{
    public class ManpowerRequirement
    {
        [Key] public int Id { get; set; }

        [Required] public int DepartmentId { get; set; }
        [ForeignKey(nameof(DepartmentId))] public virtual Department? Department { get; set; }

        [Required] public int DesignationId { get; set; }
        [ForeignKey(nameof(DesignationId))] public virtual Designation? Designation { get; set; }

        public int RequiredCount { get; set; }
        
        [StringLength(500)] public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
