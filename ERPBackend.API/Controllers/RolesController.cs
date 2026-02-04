using ERPBackend.Core.DTOs;
using ERPBackend.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class RolesController : ControllerBase
    {
        private readonly IAuthService _authService;

        public RolesController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _authService.GetRolesAsync();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto model)
        {
            var result = await _authService.CreateRoleAsync(model.RoleName);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var role = await _authService.GetRoleByIdAsync(id);
            if (role == null) return NotFound("Role not found");
            return Ok(role);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleDto model)
        {
            var result = await _authService.UpdateRoleAsync(id, model.RoleName);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var result = await _authService.DeleteRoleAsync(id);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }
    }
}

using ERPBackend.Core.DTOs;
using ERPBackend.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,Admin")] 
    public class RolesController : ControllerBase
    {
        private readonly IAuthService _authService;

        public RolesController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _authService.GetRolesAsync();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto model)
        {
            var result = await _authService.CreateRoleAsync(model.RoleName);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
