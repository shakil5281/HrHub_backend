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

        [HttpPost("{userId}/assign-role")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleDto model)
        {
            if (userId != model.UserId) return BadRequest("User ID mismatch");

            var result = await _authService.AssignRoleAsync(userId, model.RoleName);
            if (!result.Success) return BadRequest(result);
            
            return Ok(result);
        }
    }
}
