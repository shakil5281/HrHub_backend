using ERPBackend.Core.Enums;

namespace ERPBackend.Core.DTOs
{
    public class OrderSheetDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public string ProgramNumber { get; set; } = string.Empty;
        // public int BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string FabricDescription { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string FactoryName { get; set; } = string.Empty;
        public string FactoryAddress { get; set; } = string.Empty;
        
        public List<OrderSheetItemDto> Items { get; set; } = new();
    }

    public class OrderSheetItemDto
    {
        public int Id { get; set; }
        public int StyleId { get; set; }
        public string OldArticleNo { get; set; } = string.Empty;
        public string NewArticleNo { get; set; } = string.Empty;
        public PackType PackType { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int TotalQty { get; set; }
        
        public List<OrderSheetColorDto> Colors { get; set; } = new();
    }

    public class OrderSheetColorDto
    {
        public int Id { get; set; }
        public int ColorId { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public List<OrderSheetSizeBreakdownDto> SizeBreakdowns { get; set; } = new();
    }

    public class OrderSheetSizeBreakdownDto
    {
        public int Id { get; set; }
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
    }

    public class OrderSummaryDto
    {
        public int TotalOrderQuantity { get; set; }
        public int TotalColors { get; set; }
        public Dictionary<string, int> TotalPerSize { get; set; } = new();
    }

    public class GlobalOrderSummaryDto
    {
        public int TotalPieces { get; set; }
        public int TotalPrograms { get; set; }
        public int TotalBuyers { get; set; }
        public List<BuyerDistributionDto> BuyerDistribution { get; set; } = new();
        public List<SizeDistributionDto> SizeDistribution { get; set; } = new();
        public List<ProgramSummaryDto> RecentPrograms { get; set; } = new();
    }

    public class BuyerDistributionDto
    {
        public string BuyerName { get; set; } = string.Empty;
        public int TotalQty { get; set; }
        public double Percentage { get; set; }
    }

    public class SizeDistributionDto
    {
        public string SizeName { get; set; } = string.Empty;
        public int TotalQty { get; set; }
    }

    public class ProgramSummaryDto
    {
        public int Id { get; set; }
        public string ProgramNumber { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public int TotalQty { get; set; }
        public DateTime OrderDate { get; set; }
    }

    public class OrderSheetImportDto
    {
        public string ProgramNumber { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string FabricDescription { get; set; } = string.Empty;
        public string OldArticleNo { get; set; } = string.Empty;
        public string NewArticleNo { get; set; } = string.Empty;
        public string PackType { get; set; } = string.Empty; // "PackA", "PackB", "PackAB"
        public string ItemName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int SizeM { get; set; }
        public int SizeL { get; set; }
        public int SizeXL { get; set; }
        public int SizeXXL { get; set; }
        public int SizeXXXL { get; set; }
        public int Size3XL { get; set; }
        public int Size4XL { get; set; }
        public int Size5XL { get; set; }
        public int Size6XL { get; set; }
        public string BuyerPackingNumber { get; set; } = string.Empty;
        
        // Validation helpers
        public bool IsValid { get; set; } = true;
        public string ErrorMessage { get; set; } = string.Empty;
        public int RowIndex { get; set; }
    }
}
