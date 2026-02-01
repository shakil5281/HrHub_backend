using ERPBackend.Core.DTOs;
using ERPBackend.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class PermissionsController : ControllerBase
    {
        private readonly IAuthService _authService;

        public PermissionsController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _authService.GetAllPermissionsAsync();
            return Ok(permissions);
        }

        [HttpGet("{roleName}")]
        public async Task<IActionResult> GetRolePermissions(string roleName)
        {
            var result = await _authService.GetRolePermissionsAsync(roleName);
            return Ok(result);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateRolePermissions([FromBody] UpdatePermissionDto model)
        {
            var result = await _authService.UpdateRolePermissionsAsync(model);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
