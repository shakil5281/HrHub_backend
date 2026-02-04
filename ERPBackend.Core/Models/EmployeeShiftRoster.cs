using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Entities;

namespace ERPBackend.Core.Models
{
    public class EmployeeShiftRoster
    {
        [Key] public int Id { get; set; }

        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))] public virtual Employee? Employee { get; set; }

        public DateTime Date { get; set; }

        public int ShiftId { get; set; }
        [ForeignKey(nameof(ShiftId))] public virtual Shift? Shift { get; set; }

        public bool IsOffDay { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
