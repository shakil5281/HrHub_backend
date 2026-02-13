using System;
using System.Collections.Generic;

namespace ERPBackend.Core.DTOs
{
    public class ProductionDto
    {
        public int Id { get; set; }
        public string ProgramCode { get; set; } = string.Empty;
        public string Buyer { get; set; } = string.Empty;
        public int OrderQty { get; set; }
        public string StyleNo { get; set; } = string.Empty;
        public string Item { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<ProductionColorDto> Colors { get; set; } = new List<ProductionColorDto>();
    }

    public class ProductionColorDto
    {
        public int Id { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class CreateProductionDto
    {
        public string ProgramCode { get; set; } = string.Empty;
        public string Buyer { get; set; } = string.Empty;
        public int OrderQty { get; set; }
        public string StyleNo { get; set; } = string.Empty;
        public string Item { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public string Status { get; set; } = "Pending";
        public List<CreateProductionColorDto> Colors { get; set; } = new List<CreateProductionColorDto>();
    }

    public class CreateProductionColorDto
    {
        public string ColorName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class ProductionReportDto
    {
        public int TotalOrderQty { get; set; }
        public int TotalComplete { get; set; }
        public int TotalRunning { get; set; }
        public int TotalPending { get; set; }
        public int TotalClose { get; set; }
    }
}
