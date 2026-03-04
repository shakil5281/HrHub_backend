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
        public string PlanNumber { get; set; }
        
        public string StyleName { get; set; }
        public string OrderNumber { get; set; }
        
        public DateTime TargetDate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal PlannedQuantity { get; set; }
        
        public string Priority { get; set; } // High, Normal, Low
        public string Status { get; set; } // Pending, Processing, Completed
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class FabricBooking
    {
        public int Id { get; set; }
        
        [Required]
        public string OrderReference { get; set; }
        
        public string FabricType { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal RequiredQuantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal IssuedQuantity { get; set; }
        
        public string Unit { get; set; } // Yds, Mtrs, Kg
        public string Status { get; set; }
    }

    public class MarkerLayout
    {
        public int Id { get; set; }
        
        [Required]
        public string MarkerId { get; set; }
        
        public string StyleName { get; set; }
        public string Width { get; set; }
        public string Length { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Efficiency { get; set; }
        
        public string CADFilePath { get; set; }
        public string Status { get; set; } // Approved, Printing, Designing
    }

    public class CuttingBatch
    {
        public int Id { get; set; }
        
        [Required]
        public string BatchNumber { get; set; }
        
        public int? PlanId { get; set; }
        public int? MarkerId { get; set; }
        
        public DateTime CuttingDate { get; set; }
        public string CutterName { get; set; }
        public string TableNumber { get; set; }
        
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
        
        public string Size { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Wastage { get; set; }
    }

    public class Bundle
    {
        public int Id { get; set; }
        
        [Required]
        public string BundleTag { get; set; }
        
        public int CuttingBatchId { get; set; }
        public string StyleName { get; set; }
        public string Size { get; set; }
        public int PieceCount { get; set; }
        
        public string SerialRange { get; set; }
        public string Weight { get; set; }
        
        public string CurrentLocation { get; set; } // Cutting, Transit, Sewing
        public string Status { get; set; } // Ready, Sent, Received
    }

    public class WastageRecord
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        
        public string Category { get; set; } // End-bits, Marker-gaps, Rejects
        public string Reason { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public string Unit { get; set; }
    }
}
