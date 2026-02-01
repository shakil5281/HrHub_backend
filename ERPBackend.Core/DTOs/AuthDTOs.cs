using System.ComponentModel.DataAnnotations;

namespace ERPBackend.Core.DTOs
{
    public class RegisterDto
    {
        [Required] public string Username { get; set; } = string.Empty;

        [Required] [EmailAddress] public string Email { get; set; } = string.Empty;

        [Required] public string Password { get; set; } = string.Empty;

        public string? FullName { get; set; }
        public string? Role { get; set; } // Optional: Default to a basic role if null
    }

    public class LoginDto
    {
        [Required] public string Username { get; set; } = string.Empty;

        [Required] public string Password { get; set; } = string.Empty;
    }

    public class TokenRequestDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CreateRoleDto
    {
        [Required] public string RoleName { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsActive { get; set; }
    }

    public class AssignRoleDto
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }

    public class RoleDetailsDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }

    public class PermissionDto
    {
        public string RoleName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }

    public class UpdatePermissionDto
    {
        public string RoleName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }

    public class UserProfileUpdateDto
    {
        public string FullName { get; set; } = string.Empty;
        [EmailAddress] public string Email { get; set; } = string.Empty;
    }

    public class UpdateRoleDto
    {
        [Required] public string RoleName { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        [Required] public string NewPassword { get; set; } = string.Empty;
    }

    public class UserStatusDto
    {
        public bool IsActive { get; set; }
    }
}
