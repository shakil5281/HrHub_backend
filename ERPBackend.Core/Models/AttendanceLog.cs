using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Entities;

namespace ERPBackend.Core.Models
{
    public class AttendanceLog
    {
        [Key] public int Id { get; set; }

        public int EmployeeCard { get; set; }
        [ForeignKey(nameof(EmployeeCard))] public virtual Employee? Employee { get; set; }

        public int? CompanyId { get; set; }
        [ForeignKey(nameof(CompanyId))] public virtual Company? Company { get; set; }

        [StringLength(50)] public string? EmployeeId { get; set; }

        public DateTime LogTime { get; set; }

        [StringLength(50)] public string? DeviceId { get; set; }

        [StringLength(50)] public string? VerificationMode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
