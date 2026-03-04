using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ERPBackend.Core.Models
{
    public enum OrderStatus
    {
        Draft,
        InProgress,
        Completed,
        Cancelled
    }

    public enum TransactionType
    {
        StockIn,
        StockOut,
        OpeningBalance,
        Adjustment
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Issued,
        Cancelled
    }

    public class StoreOrder
    {
        [Key]
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int BuyerId { get; set; }
        [ForeignKey("BuyerId")]
        public Buyer? Buyer { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Draft;
        public string? Remarks { get; set; }
        public List<StoreOrderItem> Items { get; set; } = new List<StoreOrderItem>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class StoreOrderItem
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public StoreOrder? Order { get; set; }
        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        public StoreItem? Item { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Unit { get; set; }
    }

    public class StoreBooking
    {
        [Key]
        public int Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string OrderReference { get; set; } = string.Empty;
        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        public StoreItem? Item { get; set; }
        public decimal RequiredQty { get; set; }
        public decimal IssuedQty { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public DateTime BookingDate { get; set; }
        public string BookingType { get; set; } = "Accessories"; // Accessories, Elastic, Zipper, Poly, Others
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class StockTransaction
    {
        [Key]
        public int Id { get; set; }
        public string TransactionNumber { get; set; } = string.Empty;
        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        public StoreItem? Item { get; set; }
        public TransactionType Type { get; set; }
        public decimal Quantity { get; set; }
        public string? ReferenceNumber { get; set; } // e.g., GRN, Issue No, Challan No
        public string? DepartmentOrLine { get; set; }
        public string? LocationOrBin { get; set; }
        public string? SupplierName { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
