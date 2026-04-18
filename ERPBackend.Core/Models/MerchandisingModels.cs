using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Enums;

namespace ERPBackend.Core.Models
{
    // MASTER DATA REMAINS CLEAN
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
        public int LeadTime { get; set; }
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

    // ACCESORIES - FULLY MODULAR TABLES LINKED BY PROGRAMORDERID
    public abstract class BaseProgramAccessory
    {
        [Key]
        public int Id { get; set; }
        public int ProgramOrderId { get; set; }
        public int? ProgramSizeBreakdownId { get; set; } // Link to child
        
        // Metadata for reconciliation (Denormalized for performance/UI)
        public string? ItemName { get; set; }
        public string? ArticleNo { get; set; }
        public string? GarmentColor { get; set; }

        public string Unit { get; set; } = "Pcs";
        public string Status { get; set; } = "Pending";
        public string Supplier { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal RequiredQuantity { get; set; }

        [ForeignKey("ProgramOrderId")]
        public virtual ProgramOrder? ProgramOrder { get; set; }
        
        [ForeignKey("ProgramSizeBreakdownId")]
        public virtual ProgramSizeBreakdown? ProgramSizeBreakdown { get; set; }
    }

    public class FabricBooking : BaseProgramAccessory {
        public string FabricType { get; set; } = string.Empty;
        public FabricBooking() { Unit = "Yds"; }
    }

    public class ButtonBooking : BaseProgramAccessory {
        public string ButtonType { get; set; } = string.Empty;
        public string ButtonSize { get; set; } = string.Empty;
        public string ButtonColor { get; set; } = string.Empty;
        public int? ButtonColorId { get; set; }
        
        [ForeignKey("ButtonColorId")]
        public virtual FabricColorPantone? ButtonColorMaster { get; set; }
    }

    public class ZipperBooking : BaseProgramAccessory {
        public string ZipperType { get; set; } = string.Empty;
        public string ZipperSize { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Length { get; set; } = string.Empty;
    }

    public class MainLabelBooking : BaseProgramAccessory {
        public string Material { get; set; } = string.Empty;
        public string PrintDetails { get; set; } = string.Empty;
    }

    public class CareLabelBooking : BaseProgramAccessory {
        public string Material { get; set; } = string.Empty;
        public string PrintDetails { get; set; } = string.Empty;
    }

    public class PolyBooking : BaseProgramAccessory {
        public string PolyType { get; set; } = string.Empty; // Single, Blister, etc.
        public string Size { get; set; } = string.Empty;
        public string PrintDetails { get; set; } = string.Empty;
    }

    public class ThreadBooking : BaseProgramAccessory {
        public string ThreadType { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public ThreadBooking() { Unit = "Cones"; }
    }

    public class SnapButtonBooking : BaseProgramAccessory {
        public string SnapType { get; set; } = string.Empty;
        public string SnapSize { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}
