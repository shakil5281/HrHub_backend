using Microsoft.AspNetCore.Identity;

namespace ERPBackend.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? LastLoginIp { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
