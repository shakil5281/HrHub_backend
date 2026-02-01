using ERPBackend.Core.DTOs;
using ERPBackend.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,Admin,HR Manager")]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;

        public UsersController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDto model)
        {
            var result = await _authService.RegisterAsync(model);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _authService.GetRolesAsync();
            return Ok(roles);
        }

        [HttpPost("roles")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto model)
        {
            var result = await _authService.CreateRoleAsync(model.RoleName);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{userId}/assign-role")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleDto model)
        {
            if (userId != model.UserId) return BadRequest("User ID mismatch");

            var result = await _authService.AssignRoleAsync(userId, model.RoleName);
            if (!result.Success) return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{userId}/remove-role")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> RemoveRole(string userId, [FromBody] AssignRoleDto model)
        {
            if (userId != model.UserId) return BadRequest("User ID mismatch");

            var result = await _authService.RemoveRoleAsync(userId, model.RoleName);
            if (!result.Success) return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{userId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UserDto model)
        {
            var result = await _authService.UpdateUserAsync(userId, model);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{userId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var result = await _authService.DeleteUserAsync(userId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{userId}/reset-password")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ResetPassword(string userId, [FromBody] ResetPasswordDto model)
        {
            var result = await _authService.ResetPasswordAsync(userId, model.NewPassword);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{userId}/toggle-status")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> ToggleUserStatus(string userId, [FromBody] UserStatusDto model)
        {
            var result = await _authService.ToggleUserActiveStatusAsync(userId, model.IsActive);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}
