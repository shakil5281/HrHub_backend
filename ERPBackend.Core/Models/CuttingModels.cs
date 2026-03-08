using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPBackend.Core.Models
{
    public class CuttingPlan
    {
        public int Id { get; set; }
        
        [Required]
        public string PlanNumber { get; set; } = string.Empty;
        
        public string StyleName { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        
        public DateTime TargetDate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal PlannedQuantity { get; set; }
        
        public string Priority { get; set; } = string.Empty; // High, Normal, Low
        public string Status { get; set; } = string.Empty; // Pending, Processing, Completed
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // FabricBooking moved to MerchandisingModels.cs and enhanced

    public class MarkerLayout
    {
        public int Id { get; set; }
        
        [Required]
        public string MarkerId { get; set; } = string.Empty;
        
        public string StyleName { get; set; } = string.Empty;
        public string Width { get; set; } = string.Empty;
        public string Length { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Efficiency { get; set; }
        
        public string CADFilePath { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Approved, Printing, Designing
    }

    public class CuttingBatch
    {
        public int Id { get; set; }
        
        [Required]
        public string BatchNumber { get; set; } = string.Empty;
        
        public int? PlanId { get; set; }
        public int? MarkerId { get; set; }
        
        public DateTime CuttingDate { get; set; }
        public string CutterName { get; set; } = string.Empty;
        public string TableNumber { get; set; } = string.Empty;
        
        public List<CuttingBatchItem> Items { get; set; } = new List<CuttingBatchItem>();
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalQuantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalWastage { get; set; } // In grams or kg
    }

    public class CuttingBatchItem
    {
        public int Id { get; set; }
        public int CuttingBatchId { get; set; }
        
        public string Size { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Wastage { get; set; }
    }

    public class Bundle
    {
        public int Id { get; set; }
        
        [Required]
        public string BundleTag { get; set; } = string.Empty;
        
        public int CuttingBatchId { get; set; }
        public string StyleName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int PieceCount { get; set; }
        
        public string SerialRange { get; set; } = string.Empty;
        public string Weight { get; set; } = string.Empty;
        
        public string CurrentLocation { get; set; } = string.Empty; // Cutting, Transit, Sewing
        public string Status { get; set; } = string.Empty; // Ready, Sent, Received
    }

    public class WastageRecord
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        
        public string Category { get; set; } = string.Empty; // End-bits, Marker-gaps, Rejects
        public string Reason { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}
