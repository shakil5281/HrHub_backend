using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPBackend.Core.Models
{
    public class StoreItem
    {
        [Key]
        public int Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public ItemCategory? Category { get; set; }
        public int UnitId { get; set; }
        [ForeignKey("UnitId")]
        public StoreUnit? Unit { get; set; }
        public decimal OpeningStock { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal MinimumStockLevel { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ItemCategory
    {
        [Key]
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class StoreUnit
    {
        [Key]
        public int Id { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string? UnitType { get; set; } // e.g., Weight, Length, Count
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Buyer
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
