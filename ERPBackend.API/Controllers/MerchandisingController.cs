using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        #region Buyers
        [HttpGet("buyers/{companyId}")]
        public async Task<ActionResult<IEnumerable<Buyer>>> GetBuyers(int companyId)
        {
            var buyers = await _merchandisingService.GetAllBuyersAsync(companyId);
            return Ok(buyers);
        }

        [HttpGet("buyers/detail/{id}")]
        public async Task<ActionResult<Buyer>> GetBuyer(int id)
        {
            var buyer = await _merchandisingService.GetBuyerByIdAsync(id);
            if (buyer == null) return NotFound();
            return Ok(buyer);
        }

        [HttpPost("buyers")]
        public async Task<ActionResult<Buyer>> CreateBuyer(Buyer buyer)
        {
            var createdBuyer = await _merchandisingService.CreateBuyerAsync(buyer);
            return CreatedAtAction(nameof(GetBuyer), new { id = createdBuyer.Id }, createdBuyer);
        }

        [HttpPut("buyers/{id}")]
        public async Task<IActionResult> UpdateBuyer(int id, Buyer buyer)
        {
            if (id != buyer.Id) return BadRequest();
            await _merchandisingService.UpdateBuyerAsync(buyer);
            return NoContent();
        }

        [HttpDelete("buyers/{id}")]
        public async Task<IActionResult> DeleteBuyer(int id)
        {
            await _merchandisingService.DeleteBuyerAsync(id);
            return NoContent();
        }
        #endregion

        #region Brands
        [HttpGet("brands/company/{companyId}")]
        public async Task<ActionResult<IEnumerable<Brand>>> GetBrandsByCompany(int companyId)
        {
            var brands = await _merchandisingService.GetBrandsByCompanyAsync(companyId);
            return Ok(brands);
        }

        [HttpGet("brands/{buyerId}")]
        public async Task<ActionResult<IEnumerable<Brand>>> GetBrands(int buyerId)
        {
            var brands = await _merchandisingService.GetBrandsByBuyerAsync(buyerId);
            return Ok(brands);
        }

        [HttpGet("brands/detail/{id}")]
        public async Task<ActionResult<Brand>> GetBrand(int id)
        {
            var brand = await _merchandisingService.GetBrandByIdAsync(id);
            if (brand == null) return NotFound();
            return Ok(brand);
        }

        [HttpPost("brands")]
        public async Task<ActionResult<Brand>> CreateBrand(Brand brand)
        {
            var createdBrand = await _merchandisingService.CreateBrandAsync(brand);
            return CreatedAtAction(nameof(GetBrand), new { id = createdBrand.Id }, createdBrand);
        }

        [HttpPut("brands/{id}")]
        public async Task<IActionResult> UpdateBrand(int id, Brand brand)
        {
            if (id != brand.Id) return BadRequest();
            await _merchandisingService.UpdateBrandAsync(brand);
            return NoContent();
        }

        [HttpDelete("brands/{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            await _merchandisingService.DeleteBrandAsync(id);
            return NoContent();
        }
        #endregion

        #region Styles
        [HttpGet("styles/buyer/{buyerId}")]
        public async Task<ActionResult<IEnumerable<Style>>> GetStyles(int buyerId)
        {
            var styles = await _merchandisingService.GetStylesByBuyerAsync(buyerId);
            return Ok(styles);
        }

        [HttpGet("styles/detail/{id}")]
        public async Task<ActionResult<Style>> GetStyle(int id)
        {
            var style = await _merchandisingService.GetStyleByIdAsync(id);
            if (style == null) return NotFound();
            return Ok(style);
        }

        [HttpPost("styles")]
        public async Task<ActionResult<Style>> CreateStyle(Style style)
        {
            var createdStyle = await _merchandisingService.CreateStyleAsync(style);
            return CreatedAtAction(nameof(GetStyle), new { id = createdStyle.Id }, createdStyle);
        }

        [HttpPut("styles/{id}")]
        public async Task<IActionResult> UpdateStyle(int id, Style style)
        {
            if (id != style.Id) return BadRequest();
            await _merchandisingService.UpdateStyleAsync(style);
            return NoContent();
        }

        [HttpDelete("styles/{id}")]
        public async Task<IActionResult> DeleteStyle(int id)
        {
            await _merchandisingService.DeleteStyleAsync(id);
            return NoContent();
        }
        #endregion

        #region Orders
        [HttpGet("orders/{companyId}")]
        public async Task<ActionResult<IEnumerable<StyleOrder>>> GetOrders(int companyId)
        {
            var orders = await _merchandisingService.GetOrdersByCompanyAsync(companyId);
            return Ok(orders);
        }

        [HttpGet("orders/detail/{id}")]
        public async Task<ActionResult<StyleOrder>> GetOrder(int id)
        {
            var order = await _merchandisingService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost("orders")]
        public async Task<ActionResult<StyleOrder>> CreateOrder(StyleOrder order)
        {
            var createdOrder = await _merchandisingService.CreateOrderAsync(order);
            return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, createdOrder);
        }
        #endregion

        #region BOM & Booking
        [HttpGet("orders/{orderId}/bom")]
        public async Task<ActionResult<BOM>> GetBOM(int orderId)
        {
            var bom = await _merchandisingService.GetBOMByOrderIdAsync(orderId);
            if (bom == null) return NotFound();
            return Ok(bom);
        }

        [HttpPost("bom")]
        public async Task<ActionResult<BOM>> CreateBOM(BOM bom)
        {
            var createdBOM = await _merchandisingService.CreateBOMAsync(bom);
            return Ok(createdBOM);
        }

        [HttpGet("orders/{orderId}/fabric-bookings")]
        public async Task<ActionResult<IEnumerable<FabricBooking>>> GetFabricBookings(int orderId)
        {
            var bookings = await _merchandisingService.GetFabricBookingsByOrderAsync(orderId);
            return Ok(bookings);
        }

        [HttpGet("fabric-bookings/{companyId}")]
        public async Task<ActionResult<IEnumerable<FabricBooking>>> GetAllFabricBookings(int companyId)
        {
            var bookings = await _merchandisingService.GetAllFabricBookingsAsync(companyId);
            return Ok(bookings);
        }

        [HttpGet("fabric-bookings/detail/{id}")]
        public async Task<ActionResult<FabricBooking>> GetFabricBooking(int id)
        {
            var booking = await _merchandisingService.GetFabricBookingByIdAsync(id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        [HttpPost("fabric-bookings")]
        public async Task<ActionResult<FabricBooking>> CreateFabricBooking(FabricBooking booking)
        {
            var createdBooking = await _merchandisingService.CreateFabricBookingAsync(booking);
            return CreatedAtAction(nameof(GetFabricBooking), new { id = createdBooking.Id }, createdBooking);
        }

        [HttpPut("fabric-bookings/{id}")]
        public async Task<IActionResult> UpdateFabricBooking(int id, FabricBooking booking)
        {
            if (id != booking.Id) return BadRequest();
            await _merchandisingService.UpdateFabricBookingAsync(booking);
            return NoContent();
        }

        [HttpDelete("fabric-bookings/{id}")]
        public async Task<IActionResult> DeleteFabricBooking(int id)
        {
            await _merchandisingService.DeleteFabricBookingAsync(id);
            return NoContent();
        }
        #endregion

        #region Production & Shipment
        [HttpGet("orders/{orderId}/production-plans")]
        public async Task<ActionResult<IEnumerable<MerchProductionPlan>>> GetProductionPlans(int orderId)
        {
            var plans = await _merchandisingService.GetProductionPlansByOrderAsync(orderId);
            return Ok(plans);
        }

        [HttpGet("orders/{orderId}/shipment")]
        public async Task<ActionResult<Shipment>> GetShipment(int orderId)
        {
            var shipment = await _merchandisingService.GetShipmentByOrderIdAsync(orderId);
            if (shipment == null) return NotFound();
            return Ok(shipment);
        }

        [HttpPost("shipments")]
        public async Task<ActionResult<Shipment>> CreateShipment(Shipment shipment)
        {
            var createdShipment = await _merchandisingService.CreateShipmentAsync(shipment);
            return Ok(createdShipment);
        }

        [HttpGet("orders/{orderId}/accessories-bookings")]
        public async Task<ActionResult<IEnumerable<AccessoriesBooking>>> GetAccessoriesBookings(int orderId)
        {
            var bookings = await _merchandisingService.GetAccessoriesBookingsByOrderAsync(orderId);
            return Ok(bookings);
        }

        [HttpGet("accessories-bookings/{companyId}")]
        public async Task<ActionResult<IEnumerable<AccessoriesBooking>>> GetAllAccessoriesBookings(int companyId)
        {
            var bookings = await _merchandisingService.GetAllAccessoriesBookingsAsync(companyId);
            return Ok(bookings);
        }

        [HttpGet("accessories-bookings/detail/{id}")]
        public async Task<ActionResult<AccessoriesBooking>> GetAccessoriesBooking(int id)
        {
            var booking = await _merchandisingService.GetAccessoriesBookingByIdAsync(id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        [HttpPost("accessories-bookings")]
        public async Task<ActionResult<AccessoriesBooking>> CreateAccessoriesBooking(AccessoriesBooking booking)
        {
            var createdBooking = await _merchandisingService.CreateAccessoriesBookingAsync(booking);
            return CreatedAtAction(nameof(GetAccessoriesBooking), new { id = createdBooking.Id }, createdBooking);
        }

        [HttpPut("accessories-bookings/{id}")]
        public async Task<IActionResult> UpdateAccessoriesBooking(int id, AccessoriesBooking booking)
        {
            if (id != booking.Id) return BadRequest();
            await _merchandisingService.UpdateAccessoriesBookingAsync(booking);
            return NoContent();
        }

        [HttpDelete("accessories-bookings/{id}")]
        public async Task<IActionResult> DeleteAccessoriesBooking(int id)
        {
            await _merchandisingService.DeleteAccessoriesBookingAsync(id);
            return NoContent();
        }
        #endregion
        #region Tech Packs
        [HttpGet("techpacks/{companyId}")]
        public async Task<ActionResult<IEnumerable<TechPack>>> GetAllTechPacks(int companyId)
        {
            var techPacks = await _merchandisingService.GetAllTechPacksAsync(companyId);
            return Ok(techPacks);
        }

        [HttpGet("techpacks/style/{styleId}")]
        public async Task<ActionResult<IEnumerable<TechPack>>> GetTechPacksByStyle(int styleId)
        {
            var techPacks = await _merchandisingService.GetTechPacksByStyleAsync(styleId);
            return Ok(techPacks);
        }

        [HttpPost("techpacks")]
        public async Task<ActionResult<TechPack>> CreateTechPack(TechPack techPack)
        {
            var createdTechPack = await _merchandisingService.CreateTechPackAsync(techPack);
            return Ok(createdTechPack);
        }

        [HttpDelete("techpacks/{id}")]
        public async Task<IActionResult> DeleteTechPack(int id)
        {
            await _merchandisingService.DeleteTechPackAsync(id);
            return NoContent();
        }
        #endregion

        #region Button Bookings
        [HttpGet("orders/{orderId}/button-bookings")]
        public async Task<ActionResult<IEnumerable<ButtonBooking>>> GetButtonBookings(int orderId)
        {
            var bookings = await _merchandisingService.GetButtonBookingsByOrderAsync(orderId);
            return Ok(bookings);
        }

        [HttpGet("button-bookings/{companyId}")]
        public async Task<ActionResult<IEnumerable<ButtonBooking>>> GetAllButtonBookings(int companyId)
        {
            var bookings = await _merchandisingService.GetAllButtonBookingsAsync(companyId);
            return Ok(bookings);
        }

        [HttpGet("button-bookings/detail/{id}")]
        public async Task<ActionResult<ButtonBooking>> GetButtonBooking(int id)
        {
            var booking = await _merchandisingService.GetButtonBookingByIdAsync(id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        [HttpPost("button-bookings")]
        public async Task<ActionResult<ButtonBooking>> CreateButtonBooking(ButtonBooking booking)
        {
            var createdBooking = await _merchandisingService.CreateButtonBookingAsync(booking);
            return CreatedAtAction(nameof(GetButtonBooking), new { id = createdBooking.Id }, createdBooking);
        }

        [HttpPut("button-bookings/{id}")]
        public async Task<IActionResult> UpdateButtonBooking(int id, ButtonBooking booking)
        {
            if (id != booking.Id) return BadRequest();
            await _merchandisingService.UpdateButtonBookingAsync(booking);
            return NoContent();
        }

        [HttpDelete("button-bookings/{id}")]
        public async Task<IActionResult> DeleteButtonBooking(int id) { await _merchandisingService.DeleteButtonBookingAsync(id); return NoContent(); }

        // Snap Button Bookings
        [HttpGet("snap-button-bookings/{companyId}")]
        public async Task<ActionResult<IEnumerable<SnapButtonBooking>>> GetSnapButtonBookings(int companyId) => Ok(await _merchandisingService.GetAllSnapButtonBookingsAsync(companyId));
        [HttpGet("snap-button-bookings/detail/{id}")]
        public async Task<ActionResult<SnapButtonBooking>> GetSnapButtonBooking(int id) => Ok(await _merchandisingService.GetSnapButtonBookingByIdAsync(id));
        [HttpPost("snap-button-bookings")]
        public async Task<ActionResult<SnapButtonBooking>> CreateSnapButtonBooking(SnapButtonBooking booking) => Ok(await _merchandisingService.CreateSnapButtonBookingAsync(booking));
        [HttpPut("snap-button-bookings/{id}")]
        public async Task<IActionResult> UpdateSnapButtonBooking(int id, SnapButtonBooking booking) { if (id != booking.Id) return BadRequest(); await _merchandisingService.UpdateSnapButtonBookingAsync(booking); return NoContent(); }
        [HttpDelete("snap-button-bookings/{id}")]
        public async Task<IActionResult> DeleteSnapButtonBooking(int id) { await _merchandisingService.DeleteSnapButtonBookingAsync(id); return NoContent(); }

        // Zipper Bookings
        [HttpGet("zipper-bookings/{companyId}")]
        public async Task<ActionResult<IEnumerable<ZipperBooking>>> GetZipperBookings(int companyId) => Ok(await _merchandisingService.GetAllZipperBookingsAsync(companyId));
        [HttpGet("zipper-bookings/detail/{id}")]
        public async Task<ActionResult<ZipperBooking>> GetZipperBooking(int id) => Ok(await _merchandisingService.GetZipperBookingByIdAsync(id));
        [HttpPost("zipper-bookings")]
        public async Task<ActionResult<ZipperBooking>> CreateZipperBooking(ZipperBooking booking) => Ok(await _merchandisingService.CreateZipperBookingAsync(booking));
        [HttpPut("zipper-bookings/{id}")]
        public async Task<IActionResult> UpdateZipperBooking(int id, ZipperBooking booking) { if (id != booking.Id) return BadRequest(); await _merchandisingService.UpdateZipperBookingAsync(booking); return NoContent(); }
        [HttpDelete("zipper-bookings/{id}")]
        public async Task<IActionResult> DeleteZipperBooking(int id) { await _merchandisingService.DeleteZipperBookingAsync(id); return NoContent(); }

        // Label Bookings
        [HttpGet("label-bookings/{companyId}")]
        public async Task<ActionResult<IEnumerable<LabelBooking>>> GetLabelBookings(int companyId) => Ok(await _merchandisingService.GetAllLabelBookingsAsync(companyId));
        [HttpGet("label-bookings/detail/{id}")]
        public async Task<ActionResult<LabelBooking>> GetLabelBooking(int id) => Ok(await _merchandisingService.GetLabelBookingByIdAsync(id));
        [HttpPost("label-bookings")]
        public async Task<ActionResult<LabelBooking>> CreateLabelBooking(LabelBooking booking) => Ok(await _merchandisingService.CreateLabelBookingAsync(booking));
        [HttpPut("label-bookings/{id}")]
        public async Task<IActionResult> UpdateLabelBooking(int id, LabelBooking booking) { if (id != booking.Id) return BadRequest(); await _merchandisingService.UpdateLabelBookingAsync(booking); return NoContent(); }
        [HttpDelete("label-bookings/{id}")]
        public async Task<IActionResult> DeleteLabelBooking(int id) { await _merchandisingService.DeleteLabelBookingAsync(id); return NoContent(); }

        // Trim Bookings
        [HttpGet("trim-bookings/{companyId}")]
        public async Task<ActionResult<IEnumerable<TrimBooking>>> GetTrimBookings(int companyId) => Ok(await _merchandisingService.GetAllTrimBookingsAsync(companyId));
        [HttpGet("trim-bookings/detail/{id}")]
        public async Task<ActionResult<TrimBooking>> GetTrimBooking(int id) => Ok(await _merchandisingService.GetTrimBookingByIdAsync(id));
        [HttpPost("trim-bookings")]
        public async Task<ActionResult<TrimBooking>> CreateTrimBooking(TrimBooking booking) => Ok(await _merchandisingService.CreateTrimBookingAsync(booking));
        [HttpPut("trim-bookings/{id}")]
        public async Task<IActionResult> UpdateTrimBooking(int id, TrimBooking booking) { if (id != booking.Id) return BadRequest(); await _merchandisingService.UpdateTrimBookingAsync(booking); return NoContent(); }
        [HttpDelete("trim-bookings/{id}")]
        public async Task<IActionResult> DeleteTrimBooking(int id) { await _merchandisingService.DeleteTrimBookingAsync(id); return NoContent(); }

        // Thread Bookings
        [HttpGet("thread-bookings/{companyId}")]
        public async Task<ActionResult<IEnumerable<ThreadBooking>>> GetThreadBookings(int companyId) => Ok(await _merchandisingService.GetAllThreadBookingsAsync(companyId));
        [HttpGet("thread-bookings/detail/{id}")]
        public async Task<ActionResult<ThreadBooking>> GetThreadBooking(int id) => Ok(await _merchandisingService.GetThreadBookingByIdAsync(id));
        [HttpPost("thread-bookings")]
        public async Task<ActionResult<ThreadBooking>> CreateThreadBooking(ThreadBooking booking) => Ok(await _merchandisingService.CreateThreadBookingAsync(booking));
        [HttpPut("thread-bookings/{id}")]
        public async Task<IActionResult> UpdateThreadBooking(int id, ThreadBooking booking) { if (id != booking.Id) return BadRequest(); await _merchandisingService.UpdateThreadBookingAsync(booking); return NoContent(); }
        [HttpDelete("thread-bookings/{id}")]
        public async Task<IActionResult> DeleteThreadBooking(int id) { await _merchandisingService.DeleteThreadBookingAsync(id); return NoContent(); }

        // Packing Bookings
        [HttpGet("packing-bookings/{companyId}")]
        public async Task<ActionResult<IEnumerable<PackingBooking>>> GetPackingBookings(int companyId) => Ok(await _merchandisingService.GetAllPackingBookingsAsync(companyId));
        [HttpGet("packing-bookings/detail/{id}")]
        public async Task<ActionResult<PackingBooking>> GetPackingBooking(int id) => Ok(await _merchandisingService.GetPackingBookingByIdAsync(id));
        [HttpPost("packing-bookings")]
        public async Task<ActionResult<PackingBooking>> CreatePackingBooking(PackingBooking booking) => Ok(await _merchandisingService.CreatePackingBookingAsync(booking));
        [HttpPut("packing-bookings/{id}")]
        public async Task<IActionResult> UpdatePackingBooking(int id, PackingBooking booking) { if (id != booking.Id) return BadRequest(); await _merchandisingService.UpdatePackingBookingAsync(booking); return NoContent(); }
        [HttpDelete("packing-bookings/{id}")]
        public async Task<IActionResult> DeletePackingBooking(int id) { await _merchandisingService.DeletePackingBookingAsync(id); return NoContent(); }
        #endregion
    }
}
