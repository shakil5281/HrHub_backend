namespace ERPBackend.Core.DTOs
{
    public class ProgramOrderDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public string ProgramNumber { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public int? BuyerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string FabricDescription { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string FactoryName { get; set; } = string.Empty;
        public string FactoryAddress { get; set; } = string.Empty;
        public List<ProgramArticleDto> Articles { get; set; } = new List<ProgramArticleDto>();
    }

    public class ProgramArticleDto
    {
        public int Id { get; set; }
        public int? StyleId { get; set; }
        public string OldArticleNo { get; set; } = string.Empty;
        public string NewArticleNo { get; set; } = string.Empty;
        public int PackType { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int TotalQty { get; set; }
        public string StyleNumber { get; set; } = string.Empty;
        public List<ProgramColorDto> Colors { get; set; } = new List<ProgramColorDto>();
    }

    public class ProgramColorDto
    {
        public int Id { get; set; }
        public int? ColorId { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public List<ProgramSizeBreakdownDto> SizeBreakdowns { get; set; } = new List<ProgramSizeBreakdownDto>();
    }

    public class ProgramSizeBreakdownDto
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
        public string? ButtonColor { get; set; }
        public decimal? ButtonQty { get; set; }
    }

    public class ProgramSummaryDto
    {
        public int TotalOrderQuantity { get; set; }
        public int TotalColors { get; set; }
        public Dictionary<string, int> TotalPerSize { get; set; } = new Dictionary<string, int>();
    }

    public class GlobalProgramSummaryDto
    {
        public int TotalPrograms { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class OrderImportRowDto
    {
        public string? ProgramNumber { get; set; }
        public string? ProgramName { get; set; } // Mapped from Season
        public string? BuyerName { get; set; }
        public string? CustomerName { get; set; }
        public string? ItemName { get; set; }
        public string? NewArticleNo { get; set; } // Mapped from Article No
        public string? StyleNo { get; set; }
        public string? Fabric { get; set; }
        public string? Color { get; set; } // Mapped from Color Name
        public int SizeM { get; set; }
        public int SizeL { get; set; }
        public int SizeXL { get; set; }
        public int SizeXXL { get; set; }
        public int SizeXXXL { get; set; }
        public int Size3XL { get; set; }
        public int Size4XL { get; set; }
        public int Size5XL { get; set; }
        public int Size6XL { get; set; }
        public string? PackRef { get; set; }
        public bool IsValid { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }

    public class MultiSheetOrderImportDto
    {
        public List<dynamic>? Styles { get; set; }
        public List<dynamic>? Colors { get; set; }
        public List<OrderImportRowDto>? Orders { get; set; }
    }
}
