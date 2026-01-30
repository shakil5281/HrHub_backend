namespace ERPBackend.Core.Interfaces;

public interface IPdfService
{
    Task<byte[]> GeneratePdfReportAsync<T>(IEnumerable<T> data, string reportTitle) where T : class;
    Task<byte[]> GeneratePdfFromTemplateAsync(Dictionary<string, object> data, string templateName);
    Task<byte[]> ExportToPdfAsync<T>(IEnumerable<T> data, string[] columns, string title) where T : class;
}
