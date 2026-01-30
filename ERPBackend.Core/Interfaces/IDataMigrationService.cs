namespace ERPBackend.Core.Interfaces;

public interface IDataMigrationService
{
    Task<bool> MigrateDataAsync<T>(IEnumerable<T> sourceData, string targetEntity) where T : class;
    Task<bool> BulkInsertAsync<T>(IEnumerable<T> entities) where T : class;
    Task<bool> BulkUpdateAsync<T>(IEnumerable<T> entities) where T : class;
    Task<bool> BulkDeleteAsync<T>(IEnumerable<T> entities) where T : class;
    Task<Dictionary<string, object>> GetMigrationStatusAsync(int logId);
}
