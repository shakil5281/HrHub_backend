using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace ERPBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuttingController : ControllerBase
    {
        private readonly ICuttingService _cuttingService;

        public CuttingController(ICuttingService cuttingService)
        {
            _cuttingService = cuttingService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            return Ok(await _cuttingService.GetCuttingSummaryAsync());
        }

        // Plans
        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans()
        {
            return Ok(await _cuttingService.GetPlansAsync());
        }

        [HttpPost("plans")]
        public async Task<IActionResult> CreatePlan([FromBody] CuttingPlan plan)
        {
            return Ok(await _cuttingService.CreatePlanAsync(plan));
        }

        // Fabric Bookings
        [HttpGet("fabric-bookings")]
        public async Task<IActionResult> GetFabricBookings()
        {
            return Ok(await _cuttingService.GetFabricBookingsAsync());
        }

        [HttpPost("fabric-bookings")]
        public async Task<IActionResult> CreateFabricBooking([FromBody] FabricBooking booking)
        {
            return Ok(await _cuttingService.CreateFabricBookingAsync(booking));
        }

        // Markers
        [HttpGet("markers")]
        public async Task<IActionResult> GetMarkers()
        {
            return Ok(await _cuttingService.GetMarkersAsync());
        }

        [HttpPost("markers")]
        public async Task<IActionResult> CreateMarker([FromBody] MarkerLayout marker)
        {
            return Ok(await _cuttingService.CreateMarkerAsync(marker));
        }

        // Batches
        [HttpGet("batches")]
        public async Task<IActionResult> GetBatches()
        {
            return Ok(await _cuttingService.GetBatchesAsync());
        }

        [HttpPost("batches")]
        public async Task<IActionResult> CreateBatch([FromBody] CuttingBatch batch)
        {
            return Ok(await _cuttingService.CreateBatchAsync(batch));
        }

        // Bundles
        [HttpGet("bundles")]
        public async Task<IActionResult> GetBundles()
        {
            return Ok(await _cuttingService.GetBundlesAsync());
        }

        [HttpPost("bundles")]
        public async Task<IActionResult> CreateBundle([FromBody] Bundle bundle)
        {
            return Ok(await _cuttingService.CreateBundleAsync(bundle));
        }

        [HttpPatch("bundles/{id}/status")]
        public async Task<IActionResult> UpdateBundleStatus(int id, [FromQuery] string status, [FromQuery] string location)
        {
            var result = await _cuttingService.UpdateBundleStatusAsync(id, status, location);
            if (!result) return NotFound();
            return Ok();
        }

        // Wastage
        [HttpGet("wastage")]
        public async Task<IActionResult> GetWastage()
        {
            return Ok(await _cuttingService.GetWastageRecordsAsync());
        }

        [HttpPost("wastage")]
        public async Task<IActionResult> CreateWastage([FromBody] WastageRecord record)
        {
            return Ok(await _cuttingService.CreateWastageRecordAsync(record));
        }
    }
}
