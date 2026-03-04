using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.Services.Services
{
    public class CuttingService : ICuttingService
    {
        private readonly CashbookDbContext _context;

        public CuttingService(CashbookDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CuttingPlan>> GetPlansAsync()
        {
            return await _context.CuttingPlans.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<CuttingPlan> CreatePlanAsync(CuttingPlan plan)
        {
            _context.CuttingPlans.Add(plan);
            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<IEnumerable<FabricBooking>> GetFabricBookingsAsync()
        {
            return await _context.FabricBookings.ToListAsync();
        }

        public async Task<FabricBooking> CreateFabricBookingAsync(FabricBooking booking)
        {
            _context.FabricBookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<IEnumerable<MarkerLayout>> GetMarkersAsync()
        {
            return await _context.MarkerLayouts.ToListAsync();
        }

        public async Task<MarkerLayout> CreateMarkerAsync(MarkerLayout marker)
        {
            _context.MarkerLayouts.Add(marker);
            await _context.SaveChangesAsync();
            return marker;
        }

        public async Task<IEnumerable<CuttingBatch>> GetBatchesAsync()
        {
            return await _context.CuttingBatches.Include(b => b.Items).OrderByDescending(b => b.CuttingDate).ToListAsync();
        }

        public async Task<CuttingBatch> CreateBatchAsync(CuttingBatch batch)
        {
            _context.CuttingBatches.Add(batch);
            
            // Log wastage record if any
            if (batch.TotalWastage > 0)
            {
                _context.WastageRecords.Add(new WastageRecord
                {
                    Date = batch.CuttingDate,
                    Category = "Production Waste",
                    Reason = $"Batch {batch.BatchNumber} production",
                    Amount = batch.TotalWastage,
                    Unit = "Grams"
                });
            }

            await _context.SaveChangesAsync();
            return batch;
        }

        public async Task<IEnumerable<Bundle>> GetBundlesAsync()
        {
            return await _context.Bundles.OrderByDescending(b => b.Id).ToListAsync();
        }

        public async Task<Bundle> CreateBundleAsync(Bundle bundle)
        {
            _context.Bundles.Add(bundle);
            await _context.SaveChangesAsync();
            return bundle;
        }

        public async Task<bool> UpdateBundleStatusAsync(int id, string status, string location)
        {
            var bundle = await _context.Bundles.FindAsync(id);
            if (bundle == null) return false;

            bundle.Status = status;
            bundle.CurrentLocation = location;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<WastageRecord>> GetWastageRecordsAsync()
        {
            return await _context.WastageRecords.OrderByDescending(w => w.Date).ToListAsync();
        }

        public async Task<WastageRecord> CreateWastageRecordAsync(WastageRecord record)
        {
            _context.WastageRecords.Add(record);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task<object> GetCuttingSummaryAsync()
        {
            var today = DateTime.Today;
            return new
            {
                TotalCutToday = await _context.CuttingBatches.Where(b => b.CuttingDate.Date == today).SumAsync(b => b.TotalQuantity),
                TotalBundlesToday = await _context.Bundles.CountAsync(b => b.Status == "Ready"),
                TodayWastage = await _context.WastageRecords.Where(w => w.Date.Date == today).SumAsync(w => w.Amount),
                ActivePlansCount = await _context.CuttingPlans.CountAsync(p => p.Status != "Completed"),
                RecentBatches = await _context.CuttingBatches.OrderByDescending(b => b.Id).Take(5).ToListAsync()
            };
        }
    }
}
