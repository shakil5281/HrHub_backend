namespace ERPBackend.Core.Models;

public class DataExportLog : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // Excel, PDF, CSV
    public string EntityType { get; set; } = string.Empty; // Employee, Department, etc.
    public int TotalRecords { get; set; }
    public string? FilterCriteria { get; set; }
    public DateTime ExportStartTime { get; set; }
    public DateTime? ExportEndTime { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Failed
    public string? ExportedBy { get; set; }
    public string? FilePath { get; set; }
    public long? FileSizeBytes { get; set; }
}
