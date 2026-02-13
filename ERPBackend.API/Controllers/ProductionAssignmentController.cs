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
    public class ProductionAssignmentController : ControllerBase
    {
        private readonly ProductionDbContext _context;

        public ProductionAssignmentController(ProductionDbContext context)
        {
            _context = context;
        }

        // --- Assignments ---

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductionAssignmentDto>>> GetAssignments()
        {
            return await _context.ProductionAssignments
                .Include(a => a.Production)
                .Include(a => a.Line)
                .Select(a => new ProductionAssignmentDto
                {
                    Id = a.Id,
                    ProductionId = a.ProductionId,
                    StyleNo = a.Production != null ? a.Production.StyleNo : "",
                    Buyer = a.Production != null ? a.Production.Buyer : "",
                    LineId = a.LineId,
                    LineName = a.Line != null ? a.Line.LineName : "",
                    TotalTarget = a.TotalTarget,
                    AssignDate = a.AssignDate,
                    Status = a.Status
                })
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<ProductionAssignmentDto>> CreateAssignment(CreateProductionAssignmentDto dto)
        {
            var assignment = new ProductionAssignment
            {
                ProductionId = dto.ProductionId,
                LineId = dto.LineId,
                TotalTarget = dto.TotalTarget,
                Status = dto.Status,
                AssignDate = DateTime.UtcNow
            };

            _context.ProductionAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Refresh to get related data
            var result = await _context.ProductionAssignments
                .Include(a => a.Production)
                .Include(a => a.Line)
                .FirstAsync(a => a.Id == assignment.Id);

            return Ok(new ProductionAssignmentDto
            {
                Id = result.Id,
                ProductionId = result.ProductionId,
                StyleNo = result.Production?.StyleNo ?? "",
                Buyer = result.Production?.Buyer ?? "",
                LineId = result.LineId,
                LineName = result.Line?.LineName ?? "",
                TotalTarget = result.TotalTarget,
                AssignDate = result.AssignDate,
                Status = result.Status
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var assignment = await _context.ProductionAssignments.FindAsync(id);
            if (assignment == null) return NotFound();

            _context.ProductionAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Daily/Hourly Production ---

        [HttpGet("daily-record")]
        public async Task<ActionResult<DailyProductionRecordDto>> GetDailyRecord(int assignmentId, DateTime date)
        {
            var targetDate = date.Date;
            var record = await _context.DailyProductionRecords
                .FirstOrDefaultAsync(r => r.AssignmentId == assignmentId && r.Date.Date == targetDate);

            if (record == null)
            {
                // Return default values if record doesn't exist yet
                return Ok(new DailyProductionRecordDto
                {
                    AssignmentId = assignmentId,
                    Date = targetDate,
                    DailyTarget = 0,
                    HourlyTarget = 0
                });
            }

            return Ok(new DailyProductionRecordDto
            {
                Id = record.Id,
                AssignmentId = record.AssignmentId,
                Date = record.Date,
                DailyTarget = record.DailyTarget,
                HourlyTarget = record.HourlyTarget,
                H1 = record.H1,
                H2 = record.H2,
                H3 = record.H3,
                H4 = record.H4,
                H5 = record.H5,
                H6 = record.H6,
                H7 = record.H7,
                H8 = record.H8,
                H9 = record.H9,
                H10 = record.H10,
                H11 = record.H11,
                H12 = record.H12,
                TotalCompleted = record.TotalCompleted
            });
        }

        [HttpPost("daily-record")]
        public async Task<ActionResult<DailyProductionRecordDto>> SaveDailyRecord(SaveDailyProductionDto dto)
        {
            var targetDate = dto.Date.Date;
            var record = await _context.DailyProductionRecords
                .FirstOrDefaultAsync(r => r.AssignmentId == dto.AssignmentId && r.Date.Date == targetDate);

            if (record == null)
            {
                record = new DailyProductionRecord
                {
                    AssignmentId = dto.AssignmentId,
                    Date = targetDate
                };
                _context.DailyProductionRecords.Add(record);
            }

            record.DailyTarget = dto.DailyTarget;
            record.HourlyTarget = dto.HourlyTarget;
            record.H1 = dto.H1;
            record.H2 = dto.H2;
            record.H3 = dto.H3;
            record.H4 = dto.H4;
            record.H5 = dto.H5;
            record.H6 = dto.H6;
            record.H7 = dto.H7;
            record.H8 = dto.H8;
            record.H9 = dto.H9;
            record.H10 = dto.H10;
            record.H11 = dto.H11;
            record.H12 = dto.H12;

            await _context.SaveChangesAsync();

            return Ok(new DailyProductionRecordDto
            {
                Id = record.Id,
                AssignmentId = record.AssignmentId,
                Date = record.Date,
                DailyTarget = record.DailyTarget,
                HourlyTarget = record.HourlyTarget,
                H1 = record.H1,
                H2 = record.H2,
                H3 = record.H3,
                H4 = record.H4,
                H5 = record.H5,
                H6 = record.H6,
                H7 = record.H7,
                H8 = record.H8,
                H9 = record.H9,
                H10 = record.H10,
                H11 = record.H11,
                H12 = record.H12,
                TotalCompleted = record.TotalCompleted
            });
        }

        // --- Reporting ---

        [HttpGet("report/daily")]
        public async Task<ActionResult> GetDailyReport(DateTime date)
        {
            var targetDate = date.Date;
            var records = await _context.DailyProductionRecords
                .Include(r => r.Assignment)
                    .ThenInclude(a => a!.Production)
                .Include(r => r.Assignment)
                    .ThenInclude(a => a!.Line)
                .Where(r => r.Date.Date == targetDate)
                .Select(r => new
                {
                    LineName = r.Assignment!.Line!.LineName,
                    StyleNo = r.Assignment!.Production!.StyleNo,
                    Buyer = r.Assignment!.Production!.Buyer,
                    DailyTarget = r.DailyTarget,
                    HourlyTarget = r.HourlyTarget,
                    Completed = r.H1 + r.H2 + r.H3 + r.H4 + r.H5 + r.H6 + r.H7 + r.H8 + r.H9 + r.H10 + r.H11 + r.H12,
                    Achievement = r.DailyTarget > 0 
                        ? (double)(r.H1 + r.H2 + r.H3 + r.H4 + r.H5 + r.H6 + r.H7 + r.H8 + r.H9 + r.H10 + r.H11 + r.H12) / r.DailyTarget * 100 
                        : 0
                })
                .ToListAsync();

            return Ok(records);
        }
    }
}
