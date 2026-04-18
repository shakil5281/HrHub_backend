using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Enums;

namespace ERPBackend.Core.Models
{
    public class ProgramOrder
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        
        [Required, StringLength(100)]
        public string ProgramNumber { get; set; } = string.Empty;
        
        [Required, StringLength(200)]
        public string BuyerName { get; set; } = string.Empty;
        
        public int? BuyerId { get; set; }
        public virtual Buyer? Buyer { get; set; }
        
        [StringLength(200)]
        public string CustomerName { get; set; } = string.Empty;
        
        public string FabricDescription { get; set; } = string.Empty;
        
        public string ProgramName { get; set; } = string.Empty; // Example: FW 2025 PRODUCTION
        
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public string FactoryName { get; set; } = string.Empty;
        public string FactoryAddress { get; set; } = string.Empty;
        
        // Navigation Properties for Articles
        public virtual ICollection<ProgramArticle> Articles { get; set; } = new List<ProgramArticle>();
        
        // Navigation Properties for Accessories (Relational by ProgramId)
        public virtual ICollection<ButtonBooking> Buttons { get; set; } = new List<ButtonBooking>();
        public virtual ICollection<ZipperBooking> Zippers { get; set; } = new List<ZipperBooking>();
        public virtual ICollection<SnapButtonBooking> SnapButtons { get; set; } = new List<SnapButtonBooking>();
        public virtual ICollection<MainLabelBooking> MainLabels { get; set; } = new List<MainLabelBooking>();
        public virtual ICollection<CareLabelBooking> CareLabels { get; set; } = new List<CareLabelBooking>();
        public virtual ICollection<ThreadBooking> Threads { get; set; } = new List<ThreadBooking>();
        public virtual ICollection<PolyBooking> PolyBookings { get; set; } = new List<PolyBooking>();
        public virtual ICollection<FabricBooking> FabricBookings { get; set; } = new List<FabricBooking>();
    }

    public class ProgramArticle
    {
        [Key]
        public int Id { get; set; }
        public int ProgramOrderId { get; set; }
        
        public int? StyleId { get; set; }
        public string OldArticleNo { get; set; } = string.Empty;
        public string NewArticleNo { get; set; } = string.Empty;
        
        public PackType PackType { get; set; }
        
        [Required]
        public string ItemName { get; set; } = string.Empty;
        
        public int TotalQty { get; set; }
        
        public virtual ProgramOrder? ProgramOrder { get; set; }
        public virtual Style? Style { get; set; }
        public virtual ICollection<ProgramColor> Colors { get; set; } = new List<ProgramColor>();
    }

    public class ProgramColor
    {
        [Key]
        public int Id { get; set; }
        public int ProgramArticleId { get; set; }
        
        public int? ColorId { get; set; }
        [Required]
        public string ColorName { get; set; } = string.Empty;
        
        public virtual ProgramArticle? ProgramArticle { get; set; }
        public virtual FabricColorPantone? Color { get; set; }
        public virtual ICollection<ProgramSizeBreakdown> SizeBreakdowns { get; set; } = new List<ProgramSizeBreakdown>();
    }

    public class ProgramSizeBreakdown
    {
        [Key]
        public int Id { get; set; }
        public int ProgramColorId { get; set; }

        public int SizeM { get; set; }
        public int SizeL { get; set; }
        public int SizeXL { get; set; }
        public int SizeXXL { get; set; }
        public int SizeXXXL { get; set; }
        public int Size3XL { get; set; }
        public int Size4XL { get; set; }
        public int Size5XL { get; set; }
        public int Size6XL { get; set; }

        public int RowTotal { get; set; }
        
        public string BuyerPackingNumber { get; set; } = string.Empty;
        
        public virtual ProgramColor? ProgramColor { get; set; }

        [NotMapped]
        public string? ButtonColor { get; set; }
        [NotMapped]
        public int? ButtonColorId { get; set; }
        [NotMapped]
        public decimal? ButtonQty { get; set; }
        [NotMapped]
        public string? ButtonType { get; set; }
        [NotMapped]
        public string? ButtonSize { get; set; }
        [NotMapped]
        public string? Unit { get; set; }
        [NotMapped]
        public string? Status { get; set; }

        public virtual ICollection<ProgramAccessoryRequirement> AccessoryRequirements { get; set; } = new List<ProgramAccessoryRequirement>();
    }

    public class ProgramAccessoryRequirement
    {
        [Key]
        public int Id { get; set; }
        public int ProgramOrderId { get; set; }
        public int ProgramSizeBreakdownId { get; set; }
        
        [MaxLength(50)]
        public string AccessoryType { get; set; } = string.Empty; // "Zipper", "Thread", "Poly"
        
        public int? MasterColorId { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal? RequiredQuantity { get; set; }
        public string? Specification { get; set; }

        [ForeignKey("ProgramSizeBreakdownId")]
        public virtual ProgramSizeBreakdown? ProgramSizeBreakdown { get; set; }

        [ForeignKey("MasterColorId")]
        public virtual FabricColorPantone? MasterColor { get; set; }
    }
}
