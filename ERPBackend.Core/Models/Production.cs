using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPBackend.Core.Models
{
    public class Production
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ProgramCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Buyer { get; set; } = string.Empty;

        public int OrderQty { get; set; }

        [Required]
        [StringLength(50)]
        public string StyleNo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Item { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Running, Pending, Complete, Close

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<ProductionColor> Colors { get; set; } = new List<ProductionColor>();
    }

    public class ProductionColor
    {
        [Key]
        public int Id { get; set; }

        public int ProductionId { get; set; }

        [Required]
        [StringLength(50)]
        public string ColorName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        [ForeignKey("ProductionId")]
        public Production? Production { get; set; }
    }
}
