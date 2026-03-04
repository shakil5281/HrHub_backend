using System.Collections.Generic;
using System.Threading.Tasks;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPBackend.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public StoreController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        #region Master Setup
        [HttpGet("categories")]
        public async Task<ActionResult<List<ItemCategoryDto>>> GetCategories()
        {
            return await _storeService.GetCategoriesAsync();
        }

        [HttpPost("categories")]
        public async Task<ActionResult<ItemCategoryDto>> AddCategory(ItemCategoryDto category)
        {
            return await _storeService.AddCategoryAsync(category);
        }

        [HttpGet("units")]
        public async Task<ActionResult<List<StoreUnitDto>>> GetUnits()
        {
            return await _storeService.GetUnitsAsync();
        }

        [HttpPost("units")]
        public async Task<ActionResult<StoreUnitDto>> AddUnit(StoreUnitDto unit)
        {
            return await _storeService.AddUnitAsync(unit);
        }

        [HttpGet("items")]
        public async Task<ActionResult<List<StoreItemDto>>> GetItems()
        {
            return await _storeService.GetItemsAsync();
        }

        [HttpPost("items")]
        public async Task<ActionResult<StoreItemDto>> AddItem(StoreItemDto item)
        {
            return await _storeService.AddItemAsync(item);
        }

        [HttpGet("buyers")]
        public async Task<ActionResult<List<BuyerDto>>> GetBuyers()
        {
            return await _storeService.GetBuyersAsync();
        }

        [HttpPost("buyers")]
        public async Task<ActionResult<BuyerDto>> AddBuyer(BuyerDto buyer)
        {
            return await _storeService.AddBuyerAsync(buyer);
        }
        #endregion

        #region Orders
        [HttpGet("orders")]
        public async Task<ActionResult<List<StoreOrderDto>>> GetOrders()
        {
            return await _storeService.GetOrdersAsync();
        }

        [HttpPost("orders")]
        public async Task<ActionResult<StoreOrderDto>> CreateOrder(StoreOrderDto order)
        {
            return await _storeService.CreateOrderAsync(order);
        }
        #endregion

        #region Bookings
        [HttpGet("bookings")]
        public async Task<ActionResult<List<StoreBookingDto>>> GetBookings([FromQuery] string? type)
        {
            return await _storeService.GetBookingsAsync(type);
        }

        [HttpPost("bookings")]
        public async Task<ActionResult<StoreBookingDto>> CreateBooking(StoreBookingDto booking)
        {
            return await _storeService.CreateBookingAsync(booking);
        }
        #endregion

        #region Inventory Management
        [HttpPost("stock-in")]
        public async Task<ActionResult<StockTransactionDto>> StockIn(StockTransactionDto stockIn)
        {
            return await _storeService.StockInAsync(stockIn);
        }

        [HttpPost("stock-out")]
        public async Task<ActionResult<StockTransactionDto>> StockOut(StockTransactionDto stockOut)
        {
            return await _storeService.StockOutAsync(stockOut);
        }

        [HttpGet("transactions")]
        public async Task<ActionResult<List<StockTransactionDto>>> GetTransactions()
        {
            return await _storeService.GetStockTransactionsAsync();
        }
        #endregion

        #region Dashboard & Reports
        [HttpGet("dashboard-summary")]
        public async Task<ActionResult<StockDashboardSummaryDto>> GetDashboardSummary()
        {
            return await _storeService.GetDashboardSummaryAsync();
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<List<StoreItemDto>>> GetLowStock()
        {
            return await _storeService.GetLowStockItemsAsync();
        }

        [HttpGet("shortage-report")]
        public async Task<ActionResult<List<StoreBookingDto>>> GetShortageReport()
        {
            return await _storeService.GetShortageReportAsync();
        }
        #endregion
    }
}
