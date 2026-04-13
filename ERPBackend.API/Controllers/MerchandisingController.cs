using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERPBackend.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MerchandisingController : ControllerBase
    {
        private readonly IMerchandisingService _merchandisingService;

        public MerchandisingController(IMerchandisingService merchandisingService)
        {
            _merchandisingService = merchandisingService;
        }

        #region Buyers & Styles
        [HttpGet("buyers/{companyId}")]
        public async Task<ActionResult<IEnumerable<Buyer>>> GetBuyers(int companyId) => Ok(await _merchandisingService.GetAllBuyersAsync(companyId));
        
        [HttpPost("buyers")]
        public async Task<ActionResult<Buyer>> CreateBuyer(Buyer buyer) => Ok(await _merchandisingService.CreateBuyerAsync(buyer));

        [HttpPut("buyers/{id}")]
        public async Task<IActionResult> UpdateBuyer(int id, Buyer buyer) { await _merchandisingService.UpdateBuyerAsync(buyer); return Ok(); }

        [HttpDelete("buyers/{id}")]
        public async Task<IActionResult> DeleteBuyer(int id) { await _merchandisingService.DeleteBuyerAsync(id); return Ok(); }

        [HttpGet("styles/buyer/{buyerId}")]
        public async Task<ActionResult<IEnumerable<Style>>> GetStyles(int buyerId) => Ok(await _merchandisingService.GetStylesByBuyerAsync(buyerId));

        [HttpPost("styles")]
        public async Task<ActionResult<Style>> CreateStyle(Style style) => Ok(await _merchandisingService.CreateStyleAsync(style));

        [HttpPut("styles/{id}")]
        public async Task<IActionResult> UpdateStyle(int id, Style style) { await _merchandisingService.UpdateStyleAsync(style); return Ok(); }

        [HttpDelete("styles/{id}")]
        public async Task<IActionResult> DeleteStyle(int id) { await _merchandisingService.DeleteStyleAsync(id); return Ok(); }

        [HttpGet("brands/{companyId}")]
        public async Task<ActionResult<IEnumerable<Brand>>> GetBrands(int companyId) => Ok(await _merchandisingService.GetAllBrandsAsync(companyId));

        [HttpGet("brands/buyer/{buyerId}")]
        public async Task<ActionResult<IEnumerable<Brand>>> GetBrandsByBuyer(int buyerId) => Ok(await _merchandisingService.GetBrandsByBuyerAsync(buyerId));

        [HttpPost("brands")]
        public async Task<ActionResult<Brand>> CreateBrand(Brand brand) => Ok(await _merchandisingService.CreateBrandAsync(brand));

        [HttpPut("brands/{id}")]
        public async Task<IActionResult> UpdateBrand(int id, Brand brand) { await _merchandisingService.UpdateBrandAsync(brand); return Ok(); }

        [HttpDelete("brands/{id}")]
        public async Task<IActionResult> DeleteBrand(int id) { await _merchandisingService.DeleteBrandAsync(id); return Ok(); }
        #endregion

        #region Buttons
        [HttpGet("button-bookings/program/{programId}")]
        public async Task<ActionResult<IEnumerable<ButtonBooking>>> GetButtonBookingsByProgram(int programId) => Ok(await _merchandisingService.GetButtonBookingsByProgramAsync(programId));

        [HttpGet("button-bookings/{companyId}")]
        public async Task<ActionResult<IEnumerable<ButtonBooking>>> GetAllButtonBookings(int companyId) => Ok(await _merchandisingService.GetAllButtonBookingsAsync(companyId));
        
        [HttpPost("button-bookings")]
        public async Task<ActionResult<ButtonBooking>> CreateButtonBooking(ButtonBooking booking) => Ok(await _merchandisingService.CreateButtonBookingAsync(booking));

        [HttpPut("button-bookings/{id}")]
        public async Task<IActionResult> UpdateButtonBooking(int id, ButtonBooking booking) { await _merchandisingService.UpdateButtonBookingAsync(booking); return Ok(); }

        [HttpDelete("button-bookings/{id}")]
        public async Task<IActionResult> DeleteButtonBooking(int id) { await _merchandisingService.DeleteButtonBookingAsync(id); return Ok(); }
        #endregion

        #region Zippers
        [HttpGet("zipper-bookings/{companyId}")]
        public async Task<ActionResult<IEnumerable<ZipperBooking>>> GetAllZipperBookings(int companyId) => Ok(await _merchandisingService.GetAllZipperBookingsAsync(companyId));

        [HttpPost("zipper-bookings")]
        public async Task<ActionResult<ZipperBooking>> CreateZipperBooking(ZipperBooking booking) => Ok(await _merchandisingService.CreateZipperBookingAsync(booking));
        #endregion

        #region Labels (Main & Care)
        [HttpGet("main-label-bookings/{companyId}")]
        public async Task<ActionResult<IEnumerable<MainLabelBooking>>> GetAllMainLabelBookings(int companyId) => Ok(await _merchandisingService.GetAllMainLabelBookingsAsync(companyId));

        [HttpPost("main-label-bookings")]
        public async Task<ActionResult<MainLabelBooking>> CreateMainLabelBooking(MainLabelBooking booking) => Ok(await _merchandisingService.CreateMainLabelBookingAsync(booking));

        [HttpGet("care-label-bookings/{companyId}")]
        public async Task<ActionResult<IEnumerable<CareLabelBooking>>> GetAllCareLabelBookings(int companyId) => Ok(await _merchandisingService.GetAllCareLabelBookingsAsync(companyId));

        [HttpPost("care-label-bookings")]
        public async Task<ActionResult<CareLabelBooking>> CreateCareLabelBooking(CareLabelBooking booking) => Ok(await _merchandisingService.CreateCareLabelBookingAsync(booking));
        #endregion

        #region Poly & Packing
        [HttpGet("poly-bookings/{companyId}")]
        public async Task<ActionResult<IEnumerable<PolyBooking>>> GetAllPolyBookings(int companyId) => Ok(await _merchandisingService.GetAllPolyBookingsAsync(companyId));

        [HttpPost("poly-bookings")]
        public async Task<ActionResult<PolyBooking>> CreatePolyBooking(PolyBooking booking) => Ok(await _merchandisingService.CreatePolyBookingAsync(booking));
        #endregion

        #region Threads
        [HttpGet("thread-bookings/{companyId}")]
        public async Task<ActionResult<IEnumerable<ThreadBooking>>> GetAllThreadBookings(int companyId) => Ok(await _merchandisingService.GetAllThreadBookingsAsync(companyId));

        [HttpPost("thread-bookings")]
        public async Task<ActionResult<ThreadBooking>> CreateThreadBooking(ThreadBooking booking) => Ok(await _merchandisingService.CreateThreadBookingAsync(booking));
        #endregion
    }
}
