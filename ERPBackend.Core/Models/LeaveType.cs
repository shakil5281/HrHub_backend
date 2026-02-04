using System.ComponentModel.DataAnnotations;

namespace ERPBackend.Core.Models
{
    public class LeaveType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty; // CL, SL, AL, EL, ML

        public int YearlyLimit { get; set; }

        public bool IsCarryForward { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
