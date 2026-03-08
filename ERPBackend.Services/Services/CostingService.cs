using System.Threading.Tasks;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.Services.Services
{
    public class CostingService : ICostingService
    {
        private readonly MerchandisingDbContext _context;

        public CostingService(MerchandisingDbContext context)
        {
            _context = context;
        }

        public async Task<Costing?> GetByStyleIdAsync(int styleId)
        {
            return await _context.Costings
                .FirstOrDefaultAsync(c => c.StyleId == styleId);
        }

        public async Task<Costing> CreateOrUpdateAsync(Costing costing)
        {
            costing.FOBPrice = costing.FabricCost + costing.TrimCost + costing.CMCost + 
                               costing.WashCost + costing.PrintCost + costing.EmbroideryCost + 
                               costing.PackingCost + costing.OverheadCost + costing.ProfitMargin;

            var existing = await _context.Costings.FindAsync(costing.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(costing);
            }
            else
            {
                _context.Costings.Add(costing);
            }

            await _context.SaveChangesAsync();
            return costing;
        }

        public Task<decimal> CalculateFobPriceAsync(Costing costing)
        {
            return Task.FromResult(costing.FabricCost + costing.TrimCost + costing.CMCost + 
                                   costing.WashCost + costing.PrintCost + costing.EmbroideryCost + 
                                   costing.PackingCost + costing.OverheadCost + costing.ProfitMargin);
        }
    }
}
