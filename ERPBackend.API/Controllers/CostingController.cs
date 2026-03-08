using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPBackend.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CostingController : ControllerBase
    {
        private readonly ICostingService _costingService;

        public CostingController(ICostingService costingService)
        {
            _costingService = costingService;
        }

        [HttpGet("style/{styleId}")]
        public async Task<IActionResult> GetByStyle(int styleId)
            => Ok(await _costingService.GetByStyleIdAsync(styleId));

        [HttpPost]
        public async Task<IActionResult> Save(Costing costing)
            => Ok(await _costingService.CreateOrUpdateAsync(costing));
    }
}
