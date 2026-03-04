using System.Collections.Generic;
using System.Threading.Tasks;
using ERPBackend.Core.Models;

namespace ERPBackend.Core.Interfaces
{
    public interface ICuttingService
    {
        // Plans
        Task<IEnumerable<CuttingPlan>> GetPlansAsync();
        Task<CuttingPlan> CreatePlanAsync(CuttingPlan plan);
        
        // Fabric Booking
        Task<IEnumerable<FabricBooking>> GetFabricBookingsAsync();
        Task<FabricBooking> CreateFabricBookingAsync(FabricBooking booking);
        
        // Marker
        Task<IEnumerable<MarkerLayout>> GetMarkersAsync();
        Task<MarkerLayout> CreateMarkerAsync(MarkerLayout marker);
        
        // Batch / Production
        Task<IEnumerable<CuttingBatch>> GetBatchesAsync();
        Task<CuttingBatch> CreateBatchAsync(CuttingBatch batch);
        
        // Bundles
        Task<IEnumerable<Bundle>> GetBundlesAsync();
        Task<Bundle> CreateBundleAsync(Bundle bundle);
        Task<bool> UpdateBundleStatusAsync(int id, string status, string location);
        
        // Wastage
        Task<IEnumerable<WastageRecord>> GetWastageRecordsAsync();
        Task<WastageRecord> CreateWastageRecordAsync(WastageRecord record);
        
        // Summary
        Task<object> GetCuttingSummaryAsync();
    }
}
