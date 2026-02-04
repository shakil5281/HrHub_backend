using ERPBackend.Core.DTOs;

namespace ERPBackend.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto model);
        Task<AuthResponseDto> LoginAsync(LoginDto model, string ipAddress);
        Task<AuthResponseDto> RefreshTokenAsync(TokenRequestDto model);
        Task<bool> RevokeTokenAsync(string username);
        Task<AuthResponseDto> CreateRoleAsync(string roleName);
        Task<IEnumerable<RoleDetailsDto>> GetRolesAsync();
        Task<RoleDetailsDto?> GetRoleByIdAsync(string roleId);
        Task<AuthResponseDto> UpdateRoleAsync(string roleId, string newName);
        Task<AuthResponseDto> DeleteRoleAsync(string roleId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<AuthResponseDto> AssignRoleAsync(string userId, string roleName);
        Task<AuthResponseDto> RemoveRoleAsync(string userId, string roleName);
        Task<AuthResponseDto> UpdateUserAsync(string userId, UserDto model);
        Task<AuthResponseDto> DeleteUserAsync(string userId);
        Task<List<string>> GetAllPermissionsAsync();
        Task<PermissionDto> GetRolePermissionsAsync(string roleName);
        Task<AuthResponseDto> UpdateRolePermissionsAsync(UpdatePermissionDto model);
        Task<AuthResponseDto> UpdateProfileAsync(string username, UserProfileUpdateDto model);
        Task<UserDto?> GetProfileAsync(string username);
        Task<AuthResponseDto> ResetPasswordAsync(string userId, string newPassword);
        Task<AuthResponseDto> ToggleUserActiveStatusAsync(string userId, bool isActive);
    }
}
