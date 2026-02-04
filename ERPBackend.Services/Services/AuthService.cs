using ERPBackend.Core.DTOs;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens; // System.IdentityModel.Tokens.Jwt is needed
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return new AuthResponseDto { Success = false, Message = "User already exists!" };

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                FullName = model.FullName
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return new AuthResponseDto
                    { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

            // Assign Role
            if (!string.IsNullOrEmpty(model.Role))
            {
                if (await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }
                else
                {
                    // Optionally create role if it doesn't exist? Or fail? 
                    // For now, let's create it to be friendly or just add to default.
                    // Sticking to safety: Only add if exists.
                    return new AuthResponseDto
                        { Success = true, Message = "User created successfully, but Role did not exist." };
                }
            }

            return new AuthResponseDto { Success = true, Message = "User created successfully!" };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto model, string ipAddress)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return new AuthResponseDto { Success = false, Message = "Invalid credentials" };

            var userRoles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));

                var role = await _roleManager.FindByNameAsync(userRole);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (var roleClaim in roleClaims)
                    {
                        authClaims.Add(roleClaim);
                    }
                }
            }

            var token = GetToken(authClaims);
            var refreshToken = GenerateRefreshToken();

            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime =
                DateTime.UtcNow.AddDays(refreshTokenValidityInDays == 0 ? 7 : refreshTokenValidityInDays);
            user.LastLoginIp = ipAddress;

            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                Success = true,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = userRoles
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(TokenRequestDto model)
        {
            var principal = GetPrincipalFromExpiredToken(model.AccessToken);
            if (principal == null)
                return new AuthResponseDto { Success = false, Message = "Invalid access token or refresh token" };

            string? username = principal.Identity?.Name;

            if (username == null)
                return new AuthResponseDto { Success = false, Message = "Invalid access token" };

            var user = await _userManager.FindByNameAsync(username);

            if (user == null || user.RefreshToken != model.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return new AuthResponseDto { Success = false, Message = "Invalid access token or refresh token" };

            var newAccessToken = GetToken(principal.Claims.ToList());
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                RefreshToken = newRefreshToken,
                Success = true
            };
        }

        public async Task<bool> RevokeTokenAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return false;

            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<AuthResponseDto> CreateRoleAsync(string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
                return new AuthResponseDto { Success = false, Message = "Role already exists" };

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
                return new AuthResponseDto { Success = true, Message = "Role created successfully" };

            return new AuthResponseDto { Success = false, Message = "Failed to create role" };
        }

        public async Task<IEnumerable<RoleDetailsDto>> GetRolesAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var roleDetails = new List<RoleDetailsDto>();
            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                roleDetails.Add(new RoleDetailsDto
                {
                    Id = role.Id,
                    Name = role.Name!,
                    UserCount = usersInRole.Count
                });
            }

            return roleDetails;
        }

        public async Task<RoleDetailsDto?> GetRoleByIdAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return null;

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            return new RoleDetailsDto
            {
                Id = role.Id,
                Name = role.Name!,
                UserCount = usersInRole.Count
            };
        }

        public async Task<AuthResponseDto> UpdateRoleAsync(string roleId, string newName)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return new AuthResponseDto { Success = false, Message = "Role not found" };

            if (await _roleManager.RoleExistsAsync(newName))
                return new AuthResponseDto { Success = false, Message = "Role name already exists" };

            role.Name = newName;
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
                return new AuthResponseDto { Success = true, Message = "Role updated successfully" };

            return new AuthResponseDto { Success = false, Message = "Failed to update role" };
        }

        public async Task<AuthResponseDto> DeleteRoleAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return new AuthResponseDto { Success = false, Message = "Role not found" };

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
                return new AuthResponseDto { Success = true, Message = "Role deleted successfully" };

            return new AuthResponseDto { Success = false, Message = "Failed to delete role" };
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userDtos.Add(new UserDto
                {
                    Id = u.Id,
                    Username = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    FullName = u.FullName ?? string.Empty,
                    IsActive = u.IsActive,
                    Roles = roles
                });
            }

            return userDtos;
        }

        public async Task<AuthResponseDto> AssignRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new AuthResponseDto { Success = false, Message = "User not found" };

            if (!await _roleManager.RoleExistsAsync(roleName))
                return new AuthResponseDto { Success = false, Message = "Role not found" };

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
                return new AuthResponseDto { Success = true, Message = "Role assigned successfully" };

            return new AuthResponseDto { Success = false, Message = "Failed to assign role" };
        }

        public async Task<AuthResponseDto> RemoveRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new AuthResponseDto { Success = false, Message = "User not found" };

            if (!await _roleManager.RoleExistsAsync(roleName))
                return new AuthResponseDto { Success = false, Message = "Role not found" };

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded)
                return new AuthResponseDto { Success = true, Message = "Role removed successfully" };

            return new AuthResponseDto { Success = false, Message = "Failed to remove role" };
        }

        public async Task<AuthResponseDto> UpdateUserAsync(string userId, UserDto model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new AuthResponseDto { Success = false, Message = "User not found" };

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.IsActive = model.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return new AuthResponseDto { Success = true, Message = "User updated successfully" };

            return new AuthResponseDto { Success = false, Message = "Failed to update user" };
        }

        public async Task<AuthResponseDto> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new AuthResponseDto { Success = false, Message = "User not found" };

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                return new AuthResponseDto { Success = true, Message = "User deleted successfully" };

            return new AuthResponseDto { Success = false, Message = "Failed to delete user" };
        }

        public async Task<List<string>> GetAllPermissionsAsync()
        {
            return await Task.FromResult(ERPBackend.Core.Constants.Permissions.GetAllPermissions());
        }

        public async Task<PermissionDto> GetRolePermissionsAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return new PermissionDto { RoleName = roleName };

            var claims = await _roleManager.GetClaimsAsync(role);
            var permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

            return new PermissionDto
            {
                RoleName = roleName,
                Permissions = permissions
            };
        }

        public async Task<AuthResponseDto> UpdateRolePermissionsAsync(UpdatePermissionDto model)
        {
            var role = await _roleManager.FindByNameAsync(model.RoleName);
            if (role == null) return new AuthResponseDto { Success = false, Message = "Role not found" };

            var claims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in claims.Where(c => c.Type == "Permission"))
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            foreach (var permission in model.Permissions)
            {
                await _roleManager.AddClaimAsync(role, new Claim("Permission", permission));
            }

            return new AuthResponseDto { Success = true, Message = "Permissions updated successfully" };
        }

        public async Task<AuthResponseDto> UpdateProfileAsync(string username, UserProfileUpdateDto model)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return new AuthResponseDto { Success = false, Message = "User not found" };

            user.FullName = model.FullName;

            // Optional: Update Email if provided and different
            if (!string.IsNullOrEmpty(model.Email) && user.Email != model.Email)
            {
                user.Email = model.Email;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return new AuthResponseDto { Success = true, Message = "Profile updated successfully" };

            return new AuthResponseDto
            {
                Success = false,
                Message = "Failed to update profile: " + string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        public async Task<UserDto?> GetProfileAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                IsActive = user.IsActive,
                Roles = roles
            };
        }

        public async Task<AuthResponseDto> ResetPasswordAsync(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new AuthResponseDto { Success = false, Message = "User not found" };

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
                return new AuthResponseDto { Success = true, Message = "Password reset successfully" };

            return new AuthResponseDto
            {
                Success = false,
                Message = "Failed to reset password: " + string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        public async Task<AuthResponseDto> ToggleUserActiveStatusAsync(string userId, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new AuthResponseDto { Success = false, Message = "User not found" };

            user.IsActive = isActive;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return new AuthResponseDto
                {
                    Success = true,
                    Message = isActive ? "User activated successfully" : "User deactivated successfully"
                };

            return new AuthResponseDto { Success = false, Message = "Failed to update user status" };
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? ""));

            _ = int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddMinutes(tokenValidityInMinutes == 0 ? 60 : tokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, // You might want to validate audience/issuer depending on your scenario
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? "")),
                ValidateLifetime = false // Here we are validating the token content, not the expiration date alone
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal =
                tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
