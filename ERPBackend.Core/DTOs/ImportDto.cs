namespace ERPBackend.Core.DTOs;

public class ImportRequest
{
    public string EntityType { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // Excel, CSV
    public bool ValidateOnly { get; set; } = false;
    public bool SkipDuplicates { get; set; } = true;
    public bool UpdateExisting { get; set; } = false;
}

public class ImportResult
{
    public int TotalRecords { get; set; }
    public int SuccessfulRecords { get; set; }
    public int FailedRecords { get; set; }
    public int SkippedRecords { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public int ImportLogId { get; set; }
    public TimeSpan Duration { get; set; }
}
