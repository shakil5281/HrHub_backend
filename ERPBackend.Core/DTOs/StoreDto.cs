using System;
using System.Collections.Generic;
using ERPBackend.Core.Models;

namespace ERPBackend.Core.DTOs
{
    public class ItemCategoryDto
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class StoreUnitDto
    {
        public int Id { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string? UnitType { get; set; }
    }

    public class StoreItemDto
    {
        public int Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public decimal OpeningStock { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal MinimumStockLevel { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class BuyerDto
    {
        public int Id { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
    }

    public class StoreOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int BuyerId { get; set; }
        public string? BuyerName { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public int OrderItemsCount { get; set; }
        public List<StoreOrderItemDto> OrderItems { get; set; } = new List<StoreOrderItemDto>();
    }

    public class StoreOrderItemDto
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Unit { get; set; }
    }

    public class StoreBookingDto
    {
        public int Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public string? UnitName { get; set; }
        public decimal BookedQuantity { get; set; }
        public decimal? IssuedQty { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string BookingType { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class StockTransactionDto
    {
        public int Id { get; set; }
        public string TransactionNumber { get; set; } = string.Empty;
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? DepartmentOrLine { get; set; }
        public string? LocationOrBin { get; set; }
        public string? SupplierName { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    public class StockDashboardSummaryDto
    {
        public decimal TotalStockValue { get; set; }
        public int ActiveSKUs { get; set; }
        public int LowStockItems { get; set; }
        public int TotalOrders { get; set; }
        public int PendingBookings { get; set; }
    }
}
