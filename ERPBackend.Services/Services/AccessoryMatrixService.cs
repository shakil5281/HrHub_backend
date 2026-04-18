using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.Services.Services
{
    public interface IAccessoryMatrixService
    {
        Task<List<AccessoryRequirementDto>> GetRequirementsAsync(int orderId, string accessoryType);
        Task<bool> SaveRequirementsAsync(int orderId, string accessoryType, List<AccessoryRequirementDto> requirements);
        Task<AccessoryOrderSummaryDto?> GetOrderSummaryAsync(int orderId);
    }

    public class AccessoryMatrixService : IAccessoryMatrixService
    {
        private readonly MerchandisingDbContext _context;

        public AccessoryMatrixService(MerchandisingDbContext context)
        {
            _context = context;
        }

        public async Task<List<AccessoryRequirementDto>> GetRequirementsAsync(int orderId, string accessoryType)
        {
            return await _context.ProgramAccessoryRequirements
                .Where(r => r.ProgramOrderId == orderId && r.AccessoryType == accessoryType)
                .Select(r => new AccessoryRequirementDto
                {
                    Id = r.Id,
                    ProgramSizeBreakdownId = r.ProgramSizeBreakdownId,
                    AccessoryType = r.AccessoryType,
                    MasterColorId = r.MasterColorId,
                    MasterColorName = r.MasterColor != null ? r.MasterColor.ColorName : null,
                    RequiredQuantity = r.RequiredQuantity,
                    Specification = r.Specification
                })
                .ToListAsync();
        }

        public async Task<bool> SaveRequirementsAsync(int orderId, string accessoryType, List<AccessoryRequirementDto> requirements)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Remove existing for this order and type to rebuild
                    var existing = await _context.ProgramAccessoryRequirements
                        .Where(r => r.ProgramOrderId == orderId && r.AccessoryType == accessoryType)
                        .ToListAsync();
                    
                    _context.ProgramAccessoryRequirements.RemoveRange(existing);

                    var newEntities = requirements.Select(r => new ProgramAccessoryRequirement
                    {
                        ProgramOrderId = orderId,
                        ProgramSizeBreakdownId = r.ProgramSizeBreakdownId,
                        AccessoryType = accessoryType,
                        MasterColorId = r.MasterColorId,
                        RequiredQuantity = r.RequiredQuantity,
                        Specification = r.Specification
                    });

                    await _context.ProgramAccessoryRequirements.AddRangeAsync(newEntities);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            });
        }

        public async Task<AccessoryOrderSummaryDto?> GetOrderSummaryAsync(int orderId)
        {
            var order = await _context.ProgramOrders.FindAsync(orderId);
            if (order == null) return null;

            var summary = new AccessoryOrderSummaryDto
            {
                OrderId = orderId,
                ProgramNumber = order.ProgramNumber ?? string.Empty
            };

            var genericRequirements = await _context.ProgramAccessoryRequirements
                .Where(r => r.ProgramOrderId == orderId)
                .GroupBy(r => r.AccessoryType)
                .Select(g => new AccessoryTypeSummaryDto
                {
                    AccessoryType = g.Key,
                    TotalRequiredQuantity = g.Sum(r => r.RequiredQuantity ?? 0),
                    MappedColors = g.Count(r => r.MasterColorId.HasValue),
                    TotalSizeBreakdowns = g.Count()
                })
                .ToListAsync();

            summary.Accessories.AddRange(genericRequirements);

            // Special handling for legacy Button bookings if they exist
            var buttonBookings = await _context.ButtonBookings
                .Where(b => b.ProgramOrderId == orderId)
                .ToListAsync();

            if (buttonBookings.Any())
            {
                summary.Accessories.Add(new AccessoryTypeSummaryDto
                {
                    AccessoryType = "Button",
                    TotalRequiredQuantity = buttonBookings.Sum(b => b.RequiredQuantity),
                    TotalSizeBreakdowns = buttonBookings.Count,
                    MappedColors = buttonBookings.Count(b => b.ButtonColorId.HasValue)
                });
            }

            return summary;
        }
    }
}
