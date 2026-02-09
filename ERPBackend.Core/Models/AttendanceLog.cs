using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Entities;

namespace ERPBackend.Core.Models
{
    public class AttendanceLog
    {
        [Key] public int Id { get; set; }

        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))] public virtual Employee? Employee { get; set; }

        public int? CompanyId { get; set; }
        [ForeignKey(nameof(CompanyId))] public virtual Company? Company { get; set; }

        public DateTime LogTime { get; set; }

        [StringLength(50)] public string? DeviceId { get; set; }

        [StringLength(50)] public string? VerificationMode { get; set; } // e.g., Fingerprint, Card, Password

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
