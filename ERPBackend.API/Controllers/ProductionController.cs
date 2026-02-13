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
    public class ProductionController : ControllerBase
    {
        private readonly ProductionDbContext _context;

        public ProductionController(ProductionDbContext context)
        {
            _context = context;
        }

        // GET: api/production
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductionDto>>> GetProductions()
        {
            var productions = await _context.Productions
                .Include(p => p.Colors)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ProductionDto
                {
                    Id = p.Id,
                    ProgramCode = p.ProgramCode,
                    Buyer = p.Buyer,
                    OrderQty = p.OrderQty,
                    StyleNo = p.StyleNo,
                    Item = p.Item,
                    UnitPrice = p.UnitPrice,
                    Status = p.Status,
                    Colors = p.Colors.Select(c => new ProductionColorDto
                    {
                        Id = c.Id,
                        ColorName = c.ColorName,
                        Quantity = c.Quantity
                    }).ToList()
                })
                .ToListAsync();

            return Ok(productions);
        }

        // GET: api/production/report
        [HttpGet("report")]
        public async Task<ActionResult<ProductionReportDto>> GetProductionReport()
        {
            var productions = await _context.Productions.ToListAsync();

            var report = new ProductionReportDto
            {
                TotalOrderQty = productions.Sum(p => p.OrderQty),
                TotalComplete = productions.Where(p => p.Status == "Complete").Sum(p => p.OrderQty),
                TotalRunning = productions.Where(p => p.Status == "Running" || p.Status == "Processing").Sum(p => p.OrderQty),
                TotalPending = productions.Where(p => p.Status == "Pending").Sum(p => p.OrderQty),
                TotalClose = productions.Where(p => p.Status == "Close").Sum(p => p.OrderQty)
            };

            return Ok(report);
        }

        // GET: api/production/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductionDto>> GetProduction(int id)
        {
            var production = await _context.Productions
                .Include(p => p.Colors)
                .Where(p => p.Id == id)
                .Select(p => new ProductionDto
                {
                    Id = p.Id,
                    ProgramCode = p.ProgramCode,
                    Buyer = p.Buyer,
                    OrderQty = p.OrderQty,
                    StyleNo = p.StyleNo,
                    Item = p.Item,
                    UnitPrice = p.UnitPrice,
                    Status = p.Status,
                    Colors = p.Colors.Select(c => new ProductionColorDto
                    {
                        Id = c.Id,
                        ColorName = c.ColorName,
                        Quantity = c.Quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (production == null)
                return NotFound();

            return Ok(production);
        }

        // POST: api/production
        [HttpPost]
        public async Task<ActionResult<ProductionDto>> CreateProduction([FromBody] CreateProductionDto dto)
        {
            // Check for unique StyleNo
            if (await _context.Productions.AnyAsync(p => p.StyleNo == dto.StyleNo))
            {
                return BadRequest(new { message = $"Style No '{dto.StyleNo}' already exists." });
            }

            var production = new Production
            {
                ProgramCode = dto.ProgramCode,
                Buyer = dto.Buyer,
                OrderQty = dto.OrderQty,
                StyleNo = dto.StyleNo,
                Item = dto.Item,
                UnitPrice = dto.UnitPrice,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow,
                Colors = dto.Colors.Select(c => new ProductionColor
                {
                    ColorName = c.ColorName,
                    Quantity = c.Quantity
                }).ToList()
            };

            _context.Productions.Add(production);
            await _context.SaveChangesAsync();

            var result = new ProductionDto
            {
                Id = production.Id,
                ProgramCode = production.ProgramCode,
                Buyer = production.Buyer,
                OrderQty = production.OrderQty,
                StyleNo = production.StyleNo,
                Item = production.Item,
                UnitPrice = production.UnitPrice,
                Status = production.Status,
                Colors = production.Colors.Select(c => new ProductionColorDto
                {
                    Id = c.Id,
                    ColorName = c.ColorName,
                    Quantity = c.Quantity
                }).ToList()
            };

            return CreatedAtAction(nameof(GetProduction), new { id = production.Id }, result);
        }

        // PUT: api/production/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduction(int id, [FromBody] CreateProductionDto dto)
        {
            var production = await _context.Productions.Include(p => p.Colors).FirstOrDefaultAsync(p => p.Id == id);
            if (production == null)
                return NotFound();

            // Check for unique StyleNo if it changed
            if (production.StyleNo != dto.StyleNo && await _context.Productions.AnyAsync(p => p.StyleNo == dto.StyleNo))
            {
                return BadRequest(new { message = $"Style No '{dto.StyleNo}' already exists." });
            }

            production.ProgramCode = dto.ProgramCode;
            production.Buyer = dto.Buyer;
            production.OrderQty = dto.OrderQty;
            production.StyleNo = dto.StyleNo;
            production.Item = dto.Item;
            production.UnitPrice = dto.UnitPrice;
            production.Status = dto.Status;
            production.UpdatedAt = DateTime.UtcNow;

            // Simple way to update colors: remove all and re-add
            _context.ProductionColors.RemoveRange(production.Colors);
            production.Colors = dto.Colors.Select(c => new ProductionColor
            {
                ColorName = c.ColorName,
                Quantity = c.Quantity,
                ProductionId = id
            }).ToList();

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/production/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduction(int id)
        {
            var production = await _context.Productions.FindAsync(id);
            if (production == null)
                return NotFound();

            _context.Productions.Remove(production);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/production/clear
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearAllProductions()
        {
            // First remove all colors because of FKey (though Cascade is on, it's safer)
            var colors = await _context.ProductionColors.ToListAsync();
            _context.ProductionColors.RemoveRange(colors);

            var productions = await _context.Productions.ToListAsync();
            _context.Productions.RemoveRange(productions);

            await _context.SaveChangesAsync();
            return Ok(new { message = "All production data cleared successfully." });
        }
    }
}
