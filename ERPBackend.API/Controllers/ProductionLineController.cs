using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductionLineController : ControllerBase
    {
        private readonly ProductionDbContext _context;

        public ProductionLineController(ProductionDbContext context)
        {
            _context = context;
        }

        // GET: api/productionline
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductionLineDto>>> GetLines()
        {
            return await _context.ProductionLines
                .OrderBy(l => l.SL)
                .Select(l => new ProductionLineDto
                {
                    Id = l.Id,
                    SL = l.SL,
                    LineName = l.LineName,
                    Status = l.Status
                })
                .ToListAsync();
        }

        // GET: api/productionline/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductionLineDto>> GetLine(int id)
        {
            var line = await _context.ProductionLines.FindAsync(id);

            if (line == null)
            {
                return NotFound();
            }

            return new ProductionLineDto
            {
                Id = line.Id,
                SL = line.SL,
                LineName = line.LineName,
                Status = line.Status
            };
        }

        // POST: api/productionline
        [HttpPost]
        public async Task<ActionResult<ProductionLineDto>> CreateLine(CreateProductionLineDto dto)
        {
            var line = new ProductionLine
            {
                SL = dto.SL,
                LineName = dto.LineName,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductionLines.Add(line);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLine), new { id = line.Id }, new ProductionLineDto
            {
                Id = line.Id,
                SL = line.SL,
                LineName = line.LineName,
                Status = line.Status
            });
        }

        // PUT: api/productionline/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLine(int id, CreateProductionLineDto dto)
        {
            var line = await _context.ProductionLines.FindAsync(id);

            if (line == null)
            {
                return NotFound();
            }

            line.SL = dto.SL;
            line.LineName = dto.LineName;
            line.Status = dto.Status;
            line.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/productionline/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLine(int id)
        {
            var line = await _context.ProductionLines.FindAsync(id);
            if (line == null)
            {
                return NotFound();
            }

            _context.ProductionLines.Remove(line);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
