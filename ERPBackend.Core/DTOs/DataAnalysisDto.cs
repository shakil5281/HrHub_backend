namespace ERPBackend.Core.DTOs;

public class DataAnalysisRequest
{
    public string EntityType { get; set; } = string.Empty;
    public string AnalysisType { get; set; } = string.Empty; // Statistics, Aggregation, Custom
    public string[]? GroupByFields { get; set; }
    public string[]? AggregateFields { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public string? CustomQuery { get; set; }
}

public class DataAnalysisResult
{
    public string AnalysisType { get; set; } = string.Empty;
    public Dictionary<string, object> Results { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan Duration { get; set; }
}
