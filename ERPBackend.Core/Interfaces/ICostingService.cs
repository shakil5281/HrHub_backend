using System.Collections.Generic;
using System.Threading.Tasks;
using ERPBackend.Core.Models;

namespace ERPBackend.Core.Interfaces
{
    public interface ICostingService
    {
        Task<Costing?> GetByStyleIdAsync(int styleId);
        Task<Costing> CreateOrUpdateAsync(Costing costing);
        Task<decimal> CalculateFobPriceAsync(Costing costing);
    }
}
