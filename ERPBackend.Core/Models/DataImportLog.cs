namespace ERPBackend.Core.Models;

public class DataImportLog : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // Excel, CSV, PDF
    public string EntityType { get; set; } = string.Empty; // Employee, Department, etc.
    public int TotalRecords { get; set; }
    public int SuccessfulRecords { get; set; }
    public int FailedRecords { get; set; }
    public string? ErrorDetails { get; set; }
    public DateTime ImportStartTime { get; set; }
    public DateTime? ImportEndTime { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Failed
    public string? ImportedBy { get; set; }
}
