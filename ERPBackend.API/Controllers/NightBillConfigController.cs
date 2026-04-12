using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NightBillConfigController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NightBillConfigController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NightBillConfig>>> GetConfigs()
        {
            return await _context.NightBillConfigs.Include(c => c.Company).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<NightBillConfig>> CreateConfig(NightBillConfig config)
        {
            _context.NightBillConfigs.Add(config);
            await _context.SaveChangesAsync();
            return Ok(config);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConfig(int id, NightBillConfig config)
        {
            if (id != config.Id) return BadRequest();
            _context.Entry(config).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfig(int id)
        {
            var config = await _context.NightBillConfigs.FindAsync(id);
            if (config == null) return NotFound();
            _context.NightBillConfigs.Remove(config);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
