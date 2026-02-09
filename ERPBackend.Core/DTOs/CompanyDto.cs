using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ERPBackend.Core.Enums;

namespace ERPBackend.Core.DTOs
{
    public class CompanyDto
    {
        public int Id { get; set; }
        public BranchType Branch { get; set; }
        public string CompanyNameEn { get; set; } = string.Empty;
        public string CompanyNameBn { get; set; } = string.Empty;
        public string AddressEn { get; set; } = string.Empty;
        public string AddressBn { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public int Founded { get; set; }
        public string? LogoPath { get; set; }
        public string? AuthorizeSignaturePath { get; set; }
    }

    public class CreateCompanyDto
    {
        public BranchType Branch { get; set; } = BranchType.Secondary;

        [Required] [StringLength(200)] public string CompanyNameEn { get; set; } = string.Empty;
        [Required] [StringLength(200)] public string CompanyNameBn { get; set; } = string.Empty;

        [Required] [StringLength(500)] public string AddressEn { get; set; } = string.Empty;
        [Required] [StringLength(500)] public string AddressBn { get; set; } = string.Empty;

        [Required] [StringLength(20)] public string PhoneNumber { get; set; } = string.Empty;

        [Required] [StringLength(50)] public string RegistrationNo { get; set; } = string.Empty;

        [StringLength(100)] public string Industry { get; set; } = string.Empty;

        [Required] [EmailAddress] public string Email { get; set; } = string.Empty;

        public string Status { get; set; } = "Active";

        public int Founded { get; set; }

        public IFormFile? Logo { get; set; }
        public IFormFile? AuthorizeSignature { get; set; }
    }

    public class AssignCompanyDto
    {
        [Required] public string UserId { get; set; } = string.Empty;

        [Required] public List<int> CompanyIds { get; set; } = new List<int>();
    }
}
