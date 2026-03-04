using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.Services.Services
{
    public class StoreService : IStoreService
    {
        private readonly StoreDbContext _context;

        public StoreService(StoreDbContext context)
        {
            _context = context;
        }

        #region Category Management
        public async Task<List<ItemCategoryDto>> GetCategoriesAsync()
        {
            return await _context.ItemCategories
                .Select(c => new ItemCategoryDto
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    IsActive = c.IsActive
                }).ToListAsync();
        }

        public async Task<ItemCategoryDto> AddCategoryAsync(ItemCategoryDto category)
        {
            var entity = new ItemCategory
            {
                CategoryName = category.CategoryName,
                Description = category.Description,
                IsActive = true
            };
            _context.ItemCategories.Add(entity);
            await _context.SaveChangesAsync();
            category.Id = entity.Id;
            return category;
        }

        public async Task<bool> UpdateCategoryAsync(ItemCategoryDto category)
        {
            var entity = await _context.ItemCategories.FindAsync(category.Id);
            if (entity == null) return false;

            entity.CategoryName = category.CategoryName;
            entity.Description = category.Description;
            entity.IsActive = category.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var entity = await _context.ItemCategories.FindAsync(id);
            if (entity == null) return false;
            _context.ItemCategories.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Unit Management
        public async Task<List<StoreUnitDto>> GetUnitsAsync()
        {
            return await _context.StoreUnits
                .Select(u => new StoreUnitDto
                {
                    Id = u.Id,
                    UnitName = u.UnitName,
                    ShortName = u.ShortName,
                    UnitType = u.UnitType
                }).ToListAsync();
        }

        public async Task<StoreUnitDto> AddUnitAsync(StoreUnitDto unit)
        {
            var entity = new StoreUnit
            {
                UnitName = unit.UnitName,
                ShortName = unit.ShortName,
                UnitType = unit.UnitType
            };
            _context.StoreUnits.Add(entity);
            await _context.SaveChangesAsync();
            unit.Id = entity.Id;
            return unit;
        }
        #endregion

        #region Item Management
        public async Task<List<StoreItemDto>> GetItemsAsync()
        {
            return await _context.StoreItems
                .Include(i => i.Category)
                .Include(i => i.Unit)
                .Select(i => new StoreItemDto
                {
                    Id = i.Id,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    CategoryId = i.CategoryId,
                    CategoryName = i.Category != null ? i.Category.CategoryName : "",
                    UnitId = i.UnitId,
                    UnitName = i.Unit != null ? i.Unit.UnitName : "",
                    OpeningStock = i.OpeningStock,
                    CurrentStock = i.CurrentStock,
                    MinimumStockLevel = i.MinimumStockLevel,
                    UnitPrice = i.UnitPrice,
                    Description = i.Description,
                    IsActive = i.IsActive
                }).ToListAsync();
        }

        public async Task<StoreItemDto> GetItemByIdAsync(int id)
        {
            var i = await _context.StoreItems
                .Include(i => i.Category)
                .Include(i => i.Unit)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (i == null) return null!;

            return new StoreItemDto
            {
                Id = i.Id,
                ItemCode = i.ItemCode,
                ItemName = i.ItemName,
                CategoryId = i.CategoryId,
                UnitId = i.UnitId,
                OpeningStock = i.OpeningStock,
                CurrentStock = i.CurrentStock,
                MinimumStockLevel = i.MinimumStockLevel,
                UnitPrice = i.UnitPrice,
                Description = i.Description,
                IsActive = i.IsActive
            };
        }

        public async Task<StoreItemDto> AddItemAsync(StoreItemDto item)
        {
            var entity = new StoreItem
            {
                ItemCode = item.ItemCode,
                ItemName = item.ItemName,
                CategoryId = item.CategoryId,
                UnitId = item.UnitId,
                OpeningStock = item.OpeningStock,
                CurrentStock = item.OpeningStock,
                MinimumStockLevel = item.MinimumStockLevel,
                UnitPrice = item.UnitPrice,
                Description = item.Description,
                IsActive = true
            };
            _context.StoreItems.Add(entity);
            await _context.SaveChangesAsync();
            item.Id = entity.Id;
            return item;
        }

        public async Task<bool> UpdateItemAsync(StoreItemDto item)
        {
            var entity = await _context.StoreItems.FindAsync(item.Id);
            if (entity == null) return false;

            entity.ItemName = item.ItemName;
            entity.CategoryId = item.CategoryId;
            entity.UnitId = item.UnitId;
            entity.MinimumStockLevel = item.MinimumStockLevel;
            entity.UnitPrice = item.UnitPrice;
            entity.Description = item.Description;
            entity.IsActive = item.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            var entity = await _context.StoreItems.FindAsync(id);
            if (entity == null) return false;
            _context.StoreItems.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Buyer Management
        public async Task<List<BuyerDto>> GetBuyersAsync()
        {
            return await _context.Buyers
                .Select(b => new BuyerDto
                {
                    Id = b.Id,
                    BuyerName = b.Name,
                    Country = b.Country,
                    ContactPerson = b.ContactPerson,
                    Email = b.Email,
                    Phone = b.Phone,
                    IsActive = b.IsActive
                }).ToListAsync();
        }

        public async Task<BuyerDto> AddBuyerAsync(BuyerDto buyer)
        {
            var entity = new Buyer
            {
                Name = buyer.BuyerName,
                Country = buyer.Country,
                ContactPerson = buyer.ContactPerson,
                Email = buyer.Email,
                Phone = buyer.Phone,
                IsActive = true
            };
            _context.Buyers.Add(entity);
            await _context.SaveChangesAsync();
            buyer.Id = entity.Id;
            return buyer;
        }
        #endregion

        #region Order Management
        public async Task<List<StoreOrderDto>> GetOrdersAsync()
        {
            return await _context.StoreOrders
                .Include(o => o.Buyer)
                .Include(o => o.Items)
                .Select(o => new StoreOrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    BuyerId = o.BuyerId,
                    BuyerName = o.Buyer != null ? o.Buyer.Name : "",
                    OrderDate = o.OrderDate,
                    Status = o.Status.ToString(),
                    Remarks = o.Remarks,
                    OrderItemsCount = o.Items.Count
                }).ToListAsync();
        }

        public async Task<StoreOrderDto> GetOrderByIdAsync(int id)
        {
            var o = await _context.StoreOrders
                .Include(o => o.Buyer)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Item)
                .FirstOrDefaultAsync(ord => ord.Id == id);

            if (o == null) return null!;

            return new StoreOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                BuyerId = o.BuyerId,
                BuyerName = o.Buyer != null ? o.Buyer.Name : "",
                OrderDate = o.OrderDate,
                Status = o.Status.ToString(),
                Remarks = o.Remarks,
                OrderItemsCount = o.Items.Count,
                OrderItems = o.Items.Select(item => new StoreOrderItemDto
                {
                    Id = item.Id,
                    ItemId = item.ItemId,
                    ItemName = item.Item != null ? item.Item.ItemName : "",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Unit = item.Unit
                }).ToList()
            };
        }

        public async Task<StoreOrderDto> CreateOrderAsync(StoreOrderDto order)
        {
            var entity = new StoreOrder
            {
                OrderNumber = order.OrderNumber,
                BuyerId = order.BuyerId,
                OrderDate = order.OrderDate,
                Status = OrderStatus.Draft,
                Remarks = order.Remarks,
                Items = (order.OrderItems ?? new List<StoreOrderItemDto>()).Select(i => new StoreOrderItem
                {
                    ItemId = i.ItemId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Unit = i.Unit
                }).ToList()
            };

            _context.StoreOrders.Add(entity);
            await _context.SaveChangesAsync();
            order.Id = entity.Id;
            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var entity = await _context.StoreOrders.FindAsync(orderId);
            if (entity == null) return false;
            entity.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Booking Management
        public async Task<List<StoreBookingDto>> GetBookingsAsync(string? bookingType = null)
        {
            var query = _context.StoreBookings.Include(b => b.Item).AsQueryable();
            if (!string.IsNullOrEmpty(bookingType))
            {
                query = query.Where(b => b.BookingType == bookingType);
            }

            return await query.Select(b => new StoreBookingDto
            {
                Id = b.Id,
                BookingNumber = b.BookingNumber,
                OrderId = b.OrderId,
                OrderNumber = b.OrderReference,
                ItemId = b.ItemId,
                ItemName = b.Item != null ? b.Item.ItemName : "",
                ItemCode = b.Item != null ? b.Item.ItemCode : "",
                UnitName = (b.Item != null && b.Item.Unit != null) ? b.Item.Unit.ShortName : "",
                BookedQuantity = b.RequiredQty,
                IssuedQty = b.IssuedQty,
                Status = b.Status.ToString(),
                BookingDate = b.BookingDate,
                BookingType = b.BookingType,
                Remarks = b.Remarks
            }).ToListAsync();
        }

        public async Task<StoreBookingDto> CreateBookingAsync(StoreBookingDto booking)
        {
            var entity = new StoreBooking
            {
                BookingNumber = booking.BookingNumber,
                OrderId = booking.OrderId,
                OrderReference = booking.OrderNumber,
                ItemId = booking.ItemId,
                RequiredQty = booking.BookedQuantity,
                IssuedQty = 0,
                Status = BookingStatus.Pending,
                BookingDate = booking.BookingDate,
                BookingType = booking.BookingType,
                Remarks = booking.Remarks
            };
            _context.StoreBookings.Add(entity);
            await _context.SaveChangesAsync();
            booking.Id = entity.Id;
            return booking;
        }

        public async Task<bool> UpdateBookingStatusAsync(int bookingId, BookingStatus status)
        {
            var entity = await _context.StoreBookings.FindAsync(bookingId);
            if (entity == null) return false;
            entity.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Stock Transactions
        public async Task<StockTransactionDto> StockInAsync(StockTransactionDto stockIn)
        {
            var transaction = new StockTransaction
            {
                TransactionNumber = stockIn.TransactionNumber,
                ItemId = stockIn.ItemId,
                Type = TransactionType.StockIn,
                Quantity = stockIn.Quantity,
                ReferenceNumber = stockIn.ReferenceNumber,
                SupplierName = stockIn.SupplierName,
                LocationOrBin = stockIn.LocationOrBin,
                TransactionDate = stockIn.TransactionDate
            };

            // Update Item Stock
            var item = await _context.StoreItems.FindAsync(stockIn.ItemId);
            if (item != null)
            {
                item.CurrentStock += stockIn.Quantity;
            }

            _context.StockTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            stockIn.Id = transaction.Id;
            return stockIn;
        }

        public async Task<StockTransactionDto> StockOutAsync(StockTransactionDto stockOut)
        {
            var transaction = new StockTransaction
            {
                TransactionNumber = stockOut.TransactionNumber,
                ItemId = stockOut.ItemId,
                Type = TransactionType.StockOut,
                Quantity = stockOut.Quantity,
                ReferenceNumber = stockOut.ReferenceNumber,
                DepartmentOrLine = stockOut.DepartmentOrLine,
                TransactionDate = stockOut.TransactionDate
            };

            // Update Item Stock
            var item = await _context.StoreItems.FindAsync(stockOut.ItemId);
            if (item != null)
            {
                item.CurrentStock -= stockOut.Quantity;
            }

            _context.StockTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            stockOut.Id = transaction.Id;
            return stockOut;
        }

        public async Task<List<StockTransactionDto>> GetStockTransactionsAsync()
        {
            return await _context.StockTransactions
                .Include(t => t.Item)
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new StockTransactionDto
                {
                    Id = t.Id,
                    TransactionNumber = t.TransactionNumber,
                    ItemId = t.ItemId,
                    ItemName = t.Item != null ? t.Item.ItemName : "",
                    Type = t.Type.ToString(),
                    Quantity = t.Quantity,
                    ReferenceNumber = t.ReferenceNumber,
                    DepartmentOrLine = t.DepartmentOrLine,
                    LocationOrBin = t.LocationOrBin,
                    SupplierName = t.SupplierName,
                    TransactionDate = t.TransactionDate
                }).ToListAsync();
        }
        #endregion

        #region Dashboard & Reports
        public async Task<StockDashboardSummaryDto> GetDashboardSummaryAsync()
        {
            return new StockDashboardSummaryDto
            {
                TotalStockValue = await _context.StoreItems.SumAsync(i => i.CurrentStock * i.UnitPrice),
                ActiveSKUs = await _context.StoreItems.CountAsync(i => i.IsActive),
                LowStockItems = await _context.StoreItems.CountAsync(i => i.CurrentStock <= i.MinimumStockLevel),
                TotalOrders = await _context.StoreOrders.CountAsync(),
                PendingBookings = await _context.StoreBookings.CountAsync(b => b.Status == BookingStatus.Pending)
            };
        }

        public async Task<List<StoreItemDto>> GetLowStockItemsAsync()
        {
            return await _context.StoreItems
                .Where(i => i.CurrentStock <= i.MinimumStockLevel)
                .Select(i => new StoreItemDto
                {
                    Id = i.Id,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    CurrentStock = i.CurrentStock,
                    MinimumStockLevel = i.MinimumStockLevel,
                    UnitName = i.Unit != null ? i.Unit.ShortName : ""
                }).ToListAsync();
        }

        public async Task<List<StoreBookingDto>> GetShortageReportAsync()
        {
            // Simple logic for shortage report: confirmed bookings where stock is insufficient
            var results = await _context.StoreBookings
                .Include(b => b.Item)
                    .ThenInclude(i => i!.Unit)
                .Where(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
                .ToListAsync();

            return results.Select(b => new StoreBookingDto
            {
                Id = b.Id,
                OrderId = b.OrderId,
                OrderNumber = b.OrderReference,
                ItemId = b.ItemId,
                ItemName = b.Item?.ItemName,
                ItemCode = b.Item?.ItemCode,
                UnitName = b.Item?.Unit?.ShortName,
                BookedQuantity = b.RequiredQty,
                IssuedQty = b.IssuedQty
            }).ToList();
        }
        #endregion
    }
}
