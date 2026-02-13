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
    public class ProductionTargetController : ControllerBase
    {
        private readonly ProductionDbContext _context;

        public ProductionTargetController(ProductionDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductionTargetDto>>> GetTargets(DateTime? date)
        {
            var query = _context.ProductionTargets
                .Include(t => t.Assignment)
                    .ThenInclude(a => a!.Production)
                .Include(t => t.Assignment)
                    .ThenInclude(a => a!.Line)
                .AsQueryable();

            if (date.HasValue)
            {
                var targetDate = date.Value.Date;
                query = query.Where(t => t.TargetDate.Date == targetDate);
            }

            return await query
                .Select(t => new ProductionTargetDto
                {
                    Id = t.Id,
                    AssignmentId = t.AssignmentId,
                    StyleNo = t.Assignment!.Production!.StyleNo,
                    LineName = t.Assignment!.Line!.LineName,
                    Buyer = t.Assignment!.Production!.Buyer,
                    TargetDate = t.TargetDate,
                    DailyTarget = t.DailyTarget,
                    HourlyTarget = t.HourlyTarget,
                    Remarks = t.Remarks
                })
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<ProductionTargetDto>> SaveTarget(CreateProductionTargetDto dto)
        {
            var targetDate = dto.TargetDate.Date;
            var target = await _context.ProductionTargets
                .FirstOrDefaultAsync(t => t.AssignmentId == dto.AssignmentId && t.TargetDate.Date == targetDate);

            if (target == null)
            {
                target = new ProductionTarget
                {
                    AssignmentId = dto.AssignmentId,
                    TargetDate = targetDate,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ProductionTargets.Add(target);
            }

            target.DailyTarget = dto.DailyTarget;
            target.HourlyTarget = dto.HourlyTarget;
            target.Remarks = dto.Remarks;
            target.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Return with populated data
            var result = await _context.ProductionTargets
                .Include(t => t.Assignment)
                    .ThenInclude(a => a!.Production)
                .Include(t => t.Assignment)
                    .ThenInclude(a => a!.Line)
                .FirstAsync(t => t.Id == target.Id);

            return Ok(new ProductionTargetDto
            {
                Id = result.Id,
                AssignmentId = result.AssignmentId,
                StyleNo = result.Assignment!.Production!.StyleNo,
                LineName = result.Assignment!.Line!.LineName,
                Buyer = result.Assignment!.Production!.Buyer,
                TargetDate = result.TargetDate,
                DailyTarget = result.DailyTarget,
                HourlyTarget = result.HourlyTarget,
                Remarks = result.Remarks
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTarget(int id)
        {
            var target = await _context.ProductionTargets.FindAsync(id);
            if (target == null) return NotFound();

            _context.ProductionTargets.Remove(target);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
