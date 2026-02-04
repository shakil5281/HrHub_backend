using System;
using System.ComponentModel.DataAnnotations;

namespace ERPBackend.Core.Models
{
    public class Company
    {
        [Key] public int Id { get; set; }

        [Required] [StringLength(200)] public string CompanyNameEn { get; set; } = string.Empty;
        [Required] [StringLength(200)] public string CompanyNameBn { get; set; } = string.Empty;

        [Required] [StringLength(500)] public string Address { get; set; } = string.Empty;

        [Required] [StringLength(20)] public string PhoneNumber { get; set; } = string.Empty;

        [Required] [StringLength(50)] public string RegistrationNo { get; set; } = string.Empty;

        [StringLength(100)] public string Industry { get; set; } = string.Empty;

        [Required] [EmailAddress] public string Email { get; set; } = string.Empty;

        public string? LogoPath { get; set; }
        public string? AuthorizeSignaturePath { get; set; }

        public string Status { get; set; } = "Active";

        public int Founded { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
