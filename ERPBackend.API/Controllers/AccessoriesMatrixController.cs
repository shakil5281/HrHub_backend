using Microsoft.AspNetCore.Mvc;
using ERPBackend.Services.Services;
using ERPBackend.Core.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace ERPBackend.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccessoriesMatrixController : ControllerBase
    {
        private readonly IAccessoryMatrixService _service;

        public AccessoriesMatrixController(IAccessoryMatrixService service)
        {
            _service = service;
        }

        [HttpGet("{orderId}/{accessoryType}")]
        public async Task<IActionResult> GetRequirements(int orderId, string accessoryType)
        {
            var requirements = await _service.GetRequirementsAsync(orderId, accessoryType);
            return Ok(requirements);
        }

        [HttpPost("{orderId}/{accessoryType}")]
        public async Task<IActionResult> SaveRequirements(int orderId, string accessoryType, [FromBody] List<AccessoryRequirementDto> requirements)
        {
            if (requirements == null) return BadRequest("Requirements list cannot be null.");
            
            var result = await _service.SaveRequirementsAsync(orderId, accessoryType, requirements);
            if (result) return Ok(new { message = "Requirements saved successfully." });
            return StatusCode(500, "An error occurred while saving requirements.");
        }

        [HttpGet("order-summary/{orderId}")]
        public async Task<IActionResult> GetOrderSummary(int orderId)
        {
            var summary = await _service.GetOrderSummaryAsync(orderId);
            if (summary == null) return NotFound();
            return Ok(summary);
        }
    }
}
