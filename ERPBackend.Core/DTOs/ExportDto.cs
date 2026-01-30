namespace ERPBackend.Core.DTOs;

public class ExportRequest
{
    public string EntityType { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // Excel, PDF, CSV
    public string[]? Columns { get; set; }
    public Dictionary<string, object>? Filters { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
    public int? MaxRecords { get; set; }
    public string? TemplateName { get; set; }
}

public class ExportResult
{
    public string FileName { get; set; } = string.Empty;
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public long FileSizeBytes { get; set; }
    public int ExportLogId { get; set; }
    public TimeSpan Duration { get; set; }
}
