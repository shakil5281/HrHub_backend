using System;
using System.ComponentModel.DataAnnotations;

namespace ERPBackend.Core.Models
{
    public class Holiday
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "Public"; // Public, Company, Religious

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public int? CompanyId { get; set; }
        public virtual Company? Company { get; set; }
    }
}
