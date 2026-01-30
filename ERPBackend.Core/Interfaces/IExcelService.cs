namespace ERPBackend.Core.Interfaces;

public interface IExcelService
{
    Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName) where T : class;
    Task<IEnumerable<T>> ImportFromExcelAsync<T>(Stream fileStream, string sheetName) where T : class, new();
    Task<byte[]> ExportToExcelWithTemplateAsync<T>(IEnumerable<T> data, string templatePath) where T : class;
    Task<Dictionary<string, List<Dictionary<string, object>>>> ImportMultipleSheetsAsync(Stream fileStream);
}
