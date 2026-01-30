namespace ERPBackend.Core.Interfaces;

public interface IDataAnalysisService
{
    Task<Dictionary<string, object>> GetEntityStatisticsAsync(string entityType);
    Task<IEnumerable<Dictionary<string, object>>> ExecuteCustomQueryAsync(string query);
    Task<Dictionary<string, object>> GetAggregateDataAsync(string entityType, string[] groupByFields, string[] aggregateFields);
    Task<byte[]> GenerateAnalysisReportAsync(string entityType, Dictionary<string, object> parameters);
}
