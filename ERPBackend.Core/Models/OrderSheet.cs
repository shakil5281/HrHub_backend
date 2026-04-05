using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPBackend.Core.Enums;

namespace ERPBackend.Core.Models
{
    public class OrderSheet
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        
        [Required, StringLength(100)]
        public string ProgramNumber { get; set; } = string.Empty;
        
        // public int BuyerId { get; set; }
        [Required, StringLength(200)]
        public string BuyerName { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string CustomerName { get; set; } = string.Empty;
        
        public string FabricDescription { get; set; } = string.Empty;
        
        public string ProgramName { get; set; } = string.Empty; // Example: FW 2025 PRODUCTION
        
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // Header specific fields
        public string FactoryName { get; set; } = string.Empty;
        public string FactoryAddress { get; set; } = string.Empty;
        
        // public virtual Buyer? Buyer { get; set; }
        public virtual ICollection<OrderSheetItem> Items { get; set; } = new List<OrderSheetItem>();
    }

    public class OrderSheetItem
    {
        [Key]
        public int Id { get; set; }
        public int OrderSheetId { get; set; }
        
        public int? StyleId { get; set; }
        public string OldArticleNo { get; set; } = string.Empty;
        public string NewArticleNo { get; set; } = string.Empty;
        
        public PackType PackType { get; set; }
        
        [Required]
        public string ItemName { get; set; } = string.Empty; // Example: L/S POLO
        
        public int TotalQty { get; set; }
        
        public virtual OrderSheet? OrderSheet { get; set; }
        public virtual Style? Style { get; set; }
        public virtual ICollection<OrderSheetColor> Colors { get; set; } = new List<OrderSheetColor>();
    }

    public class OrderSheetColor
    {
        [Key]
        public int Id { get; set; }
        public int OrderSheetItemId { get; set; }
        
        public int? ColorId { get; set; }
        [Required]
        public string ColorName { get; set; } = string.Empty;
        
        public virtual OrderSheetItem? OrderSheetItem { get; set; }
        public virtual FabricColorPantone? Color { get; set; }
        public virtual ICollection<OrderSheetSizeBreakdown> SizeBreakdowns { get; set; } = new List<OrderSheetSizeBreakdown>();
    }

    public class OrderSheetSizeBreakdown
    {
        [Key]
        public int Id { get; set; }
        public int OrderSheetColorId { get; set; }

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

        public virtual OrderSheetColor? OrderSheetColor { get; set; }
    }
}
