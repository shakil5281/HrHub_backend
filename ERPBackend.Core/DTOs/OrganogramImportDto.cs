namespace ERPBackend.Core.DTOs
{
    public class OrganogramImportResultDto
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

    public class ImportErrorDto
    {
        public int RowNumber { get; set; }
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class ImportWarningDto
    {
        public int RowNumber { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class OrganogramRowDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string DepartmentNameEn { get; set; } = string.Empty;
        public string DepartmentNameBn { get; set; } = string.Empty;
        public string SectionNameEn { get; set; } = string.Empty;
        public string SectionNameBn { get; set; } = string.Empty;
        public string DesignationNameEn { get; set; } = string.Empty;
        public string DesignationNameBn { get; set; } = string.Empty;
        public decimal NightBill { get; set; }
        public decimal HolidayBill { get; set; }
        public decimal AttendanceBonus { get; set; }
        public string LineNameEn { get; set; } = string.Empty;
        public string LineNameBn { get; set; } = string.Empty;
    }
}
