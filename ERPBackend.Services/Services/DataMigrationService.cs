using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;

namespace ERPBackend.Services.Services;

public class DataMigrationService : IDataMigrationService
{
    private readonly IUnitOfWork _unitOfWork;

    public DataMigrationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> MigrateDataAsync<T>(IEnumerable<T> sourceData, string targetEntity) where T : class
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            var repository = _unitOfWork.Repository<T>();
            await repository.AddRangeAsync(sourceData);
            
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> BulkInsertAsync<T>(IEnumerable<T> entities) where T : class
    {
        try
        {
            var repository = _unitOfWork.Repository<T>();
            await repository.AddRangeAsync(entities);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> BulkUpdateAsync<T>(IEnumerable<T> entities) where T : class
    {
        try
        {
            var repository = _unitOfWork.Repository<T>();
            await repository.UpdateRangeAsync(entities);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> BulkDeleteAsync<T>(IEnumerable<T> entities) where T : class
    {
        try
        {
            var repository = _unitOfWork.Repository<T>();
            await repository.DeleteRangeAsync(entities);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Dictionary<string, object>> GetMigrationStatusAsync(int logId)
    {
        var repository = _unitOfWork.Repository<DataImportLog>();
        var log = await repository.GetByIdAsync(logId);
        
        if (log == null)
        {
            return new Dictionary<string, object>
            {
                { "Status", "NotFound" }
            };
        }

        return new Dictionary<string, object>
        {
            { "Status", log.Status },
            { "TotalRecords", log.TotalRecords },
            { "SuccessfulRecords", log.SuccessfulRecords },
            { "FailedRecords", log.FailedRecords },
            { "StartTime", log.ImportStartTime },
            { "EndTime", log.ImportEndTime ?? DateTime.UtcNow },
            { "ErrorDetails", log.ErrorDetails ?? "" }
        };
    }
}
