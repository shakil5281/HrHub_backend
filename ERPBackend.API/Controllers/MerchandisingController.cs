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
        #endregion
    }
}
