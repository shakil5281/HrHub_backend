using ERPBackend.Core.DTOs;
using System.Security.Claims;

namespace ERPBackend.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto model);
        Task<AuthResponseDto> LoginAsync(LoginDto model, string ipAddress);
        Task<AuthResponseDto> RefreshTokenAsync(TokenRequestDto model);
        Task<bool> RevokeTokenAsync(string username);
        Task<AuthResponseDto> CreateRoleAsync(string roleName);
        Task<IEnumerable<string>> GetRolesAsync();
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<AuthResponseDto> AssignRoleAsync(string userId, string roleName);
    }
}
