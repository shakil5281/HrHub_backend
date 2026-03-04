using System.Collections.Generic;
using System.Threading.Tasks;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;

namespace ERPBackend.Core.Interfaces
{
    public interface IStoreService
    {
        // Category Management
        Task<List<ItemCategoryDto>> GetCategoriesAsync();
        Task<ItemCategoryDto> AddCategoryAsync(ItemCategoryDto category);
        Task<bool> UpdateCategoryAsync(ItemCategoryDto category);
        Task<bool> DeleteCategoryAsync(int id);

        // Unit Management
        Task<List<StoreUnitDto>> GetUnitsAsync();
        Task<StoreUnitDto> AddUnitAsync(StoreUnitDto unit);

        // Store Item Management
        Task<List<StoreItemDto>> GetItemsAsync();
        Task<StoreItemDto> GetItemByIdAsync(int id);
        Task<StoreItemDto> AddItemAsync(StoreItemDto item);
        Task<bool> UpdateItemAsync(StoreItemDto item);
        Task<bool> DeleteItemAsync(int id);

        // Buyer Management
        Task<List<BuyerDto>> GetBuyersAsync();
        Task<BuyerDto> AddBuyerAsync(BuyerDto buyer);

        // Order Management
        Task<List<StoreOrderDto>> GetOrdersAsync();
        Task<StoreOrderDto> GetOrderByIdAsync(int id);
        Task<StoreOrderDto> CreateOrderAsync(StoreOrderDto order);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);

        // Booking Management
        Task<List<StoreBookingDto>> GetBookingsAsync(string? bookingType = null);
        Task<StoreBookingDto> CreateBookingAsync(StoreBookingDto booking);
        Task<bool> UpdateBookingStatusAsync(int bookingId, BookingStatus status);

        // Stock Transactions (Stock In / Stock Out)
        Task<StockTransactionDto> StockInAsync(StockTransactionDto stockIn);
        Task<StockTransactionDto> StockOutAsync(StockTransactionDto stockOut);
        Task<List<StockTransactionDto>> GetStockTransactionsAsync();

        // Dashboard & Analytics
        Task<StockDashboardSummaryDto> GetDashboardSummaryAsync();
        Task<List<StoreItemDto>> GetLowStockItemsAsync();
        Task<List<StoreBookingDto>> GetShortageReportAsync();
    }
}
