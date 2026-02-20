namespace ERPBackend.Core.DTOs
{
    public class EmployeeImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int CreatedCount { get; set; }
        public int UpdatedCount { get; set; }
        public List<ImportErrorDto> Errors { get; set; } = new List<ImportErrorDto>();
        public List<ImportWarningDto> Warnings { get; set; } = new List<ImportWarningDto>();
    }
}
