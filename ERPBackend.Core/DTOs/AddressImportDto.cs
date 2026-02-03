namespace ERPBackend.Core.DTOs
{
    public class AddressImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int UpdatedCount { get; set; }
        public int CreatedCount { get; set; }
        public List<ImportErrorDto> Errors { get; set; } = new();
        public List<ImportWarningDto> Warnings { get; set; } = new();
    }

    public class AddressRowDto
    {
        public string CountryNameEn { get; set; } = string.Empty;
        public string CountryNameBn { get; set; } = string.Empty;
        public string DivisionNameEn { get; set; } = string.Empty;
        public string DivisionNameBn { get; set; } = string.Empty;
        public string DistrictNameEn { get; set; } = string.Empty;
        public string DistrictNameBn { get; set; } = string.Empty;
        public string ThanaNameEn { get; set; } = string.Empty;
        public string ThanaNameBn { get; set; } = string.Empty;
        public string PostOfficeNameEn { get; set; } = string.Empty;
        public string PostOfficeNameBn { get; set; } = string.Empty;
        public string PostCode { get; set; } = string.Empty;
    }
}
