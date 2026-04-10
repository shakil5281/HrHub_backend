using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Enums;

namespace ERPBackend.Core.Models
{
    public class Buyer
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PaymentTerms { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
        public int LeadTime { get; set; } // In days
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Brand> Brands { get; set; } = new List<Brand>();
        public virtual Company? Company { get; set; }
    }

    public class Brand
    {
        [Key]
        public int Id { get; set; }
        public int BuyerId { get; set; }
        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public virtual Buyer? Buyer { get; set; }
    }

    public class Style
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int BuyerId { get; set; }
        public int? BrandId { get; set; }
        [Required, StringLength(100)]
        public string StyleNumber { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public string FabricType { get; set; } = string.Empty;
        public string GSM { get; set; } = string.Empty;
        public string SizeRange { get; set; } = string.Empty;

        public virtual Company? Company { get; set; }
        public virtual Buyer? Buyer { get; set; }
        public virtual Brand? Brand { get; set; }
        public virtual ICollection<TechPack> TechPacks { get; set; } = new List<TechPack>();
        public virtual ICollection<SampleRequest> SampleRequests { get; set; } = new List<SampleRequest>();
        public virtual ICollection<Costing> Costings { get; set; } = new List<Costing>();
    }

    public class TechPack
    {
        [Key]
        public int Id { get; set; }
        public int StyleId { get; set; }
        [Required]
        public string FileUrl { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0";
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        public virtual Style? Style { get; set; }
    }

    public class SampleRequest
    {
        [Key]
        public int Id { get; set; }
        public int StyleId { get; set; }
        public SampleType SampleType { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public SampleStatus Status { get; set; } = SampleStatus.Pending;
        public string BuyerFeedback { get; set; } = string.Empty;

        public virtual Style? Style { get; set; }
    }

    public class Costing
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int StyleId { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal FabricCost { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal TrimCost { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal CMCost { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal WashCost { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal PrintCost { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal EmbroideryCost { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal PackingCost { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal OverheadCost { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal ProfitMargin { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal FOBPrice { get; set; }

        public virtual Style? Style { get; set; }
    }

    public class StyleOrder
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int BuyerId { get; set; }
        public int StyleId { get; set; }
        [Required, StringLength(100)]
        public string PONumber { get; set; } = string.Empty;
        public int OrderQuantity { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime DeliveryDate { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Draft;

        public virtual Company? Company { get; set; }
        public virtual Buyer? Buyer { get; set; }
        public virtual Style? Style { get; set; }
        public virtual ICollection<OrderColor> OrderColors { get; set; } = new List<OrderColor>();
        public virtual ICollection<BOM> BOMs { get; set; } = new List<BOM>();
    }

    public class OrderColor
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        [Required]
        public string Color { get; set; } = string.Empty;

        public virtual StyleOrder? StyleOrder { get; set; }
        public virtual ICollection<OrderSizeBreakdown> SizeBreakdowns { get; set; } = new List<OrderSizeBreakdown>();
    }

    public class OrderSizeBreakdown
    {
        [Key]
        public int Id { get; set; }
        public int OrderColorId { get; set; }
        [Required]
        public string Size { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public virtual OrderColor? OrderColor { get; set; }
    }

    public class BOM
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }

        public virtual StyleOrder? StyleOrder { get; set; }
        public virtual ICollection<BOMItem> BOMItems { get; set; } = new List<BOMItem>();
    }

    public class BOMItem
    {
        [Key]
        public int Id { get; set; }
        public int BOMId { get; set; }
        [Required]
        public string ItemName { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,4)")]
        public decimal Consumption { get; set; }
        public string Unit { get; set; } = "Pcs";
        public string Supplier { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }

        public virtual BOM? BOM { get; set; }
    }

    public class FabricBooking
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string? OrderReference { get; set; }
        public string FabricType { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,4)")]
        public decimal RequiredQuantity { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal IssuedQuantity { get; set; }
        public string Unit { get; set; } = "Yds"; // Yds, Mtrs, Kg
        public string Status { get; set; } = "Pending";
        public string Supplier { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }

        public virtual StyleOrder? StyleOrder { get; set; }
    }

    public class AccessoriesBooking
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "Pcs";
        public string Status { get; set; } = "Pending";
        public string Supplier { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }

        public virtual StyleOrder? StyleOrder { get; set; }
    }

    public class MerchProductionPlan
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Factory { get; set; } = string.Empty;
        public string ProductionLine { get; set; } = string.Empty;
        public int TargetPerDay { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public virtual StyleOrder? StyleOrder { get; set; }
    }

    public class Shipment
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int OrderId { get; set; }
        public DateTime ShipmentDate { get; set; }
        public int CartonQuantity { get; set; }
        public ShipmentMethod ShippingMethod { get; set; } = ShipmentMethod.Sea;
        public string Forwarder { get; set; } = string.Empty;

        public virtual StyleOrder? StyleOrder { get; set; }
    }
}
