using OfficeOpenXml;
using ERPBackend.Core.Interfaces;
using System.Reflection;

namespace ERPBackend.Services.Services;

public class ExcelService : IExcelService
{
    public ExcelService()
    {
        // Set EPPlus license context
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName) where T : class
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add(sheetName);

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        // Add headers
        for (int i = 0; i < properties.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = properties[i].Name;
            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
        }

        // Add data
        var dataList = data.ToList();
        for (int row = 0; row < dataList.Count; row++)
        {
            for (int col = 0; col < properties.Length; col++)
            {
                var value = properties[col].GetValue(dataList[row]);
                worksheet.Cells[row + 2, col + 1].Value = value;
            }
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        return await Task.FromResult(package.GetAsByteArray());
    }

    public async Task<IEnumerable<T>> ImportFromExcelAsync<T>(Stream fileStream, string sheetName) where T : class, new()
    {
        var result = new List<T>();
        
        using var package = new ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets[sheetName] ?? package.Workbook.Worksheets.First();

        if (worksheet.Dimension == null)
            return result;

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToList();

        // Read headers
        var headers = new Dictionary<int, PropertyInfo>();
        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
        {
            var headerValue = worksheet.Cells[1, col].Value?.ToString();
            var property = properties.FirstOrDefault(p => 
                p.Name.Equals(headerValue, StringComparison.OrdinalIgnoreCase));
            
            if (property != null)
            {
                headers[col] = property;
            }
        }

        // Read data rows
        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
        {
            var item = new T();
            foreach (var header in headers)
            {
                var cellValue = worksheet.Cells[row, header.Key].Value;
                if (cellValue != null)
                {
                    try
                    {
                        var targetType = header.Value.PropertyType;
                        var underlyingType = Nullable.GetUnderlyingType(targetType);
                        var actualType = underlyingType ?? targetType;

                        var convertedValue = Convert.ChangeType(cellValue, actualType);
                        header.Value.SetValue(item, convertedValue);
                    }
                    catch
                    {
                        // Handle conversion errors
                    }
                }
            }
            result.Add(item);
        }

        return await Task.FromResult(result);
    }

    public async Task<byte[]> ExportToExcelWithTemplateAsync<T>(IEnumerable<T> data, string templatePath) where T : class
    {
        // For now, use standard export. Template support can be enhanced later
        return await ExportToExcelAsync(data, "Data");
    }

    public async Task<Dictionary<string, List<Dictionary<string, object>>>> ImportMultipleSheetsAsync(Stream fileStream)
    {
        var result = new Dictionary<string, List<Dictionary<string, object>>>();
        
        using var package = new ExcelPackage(fileStream);
        
        foreach (var worksheet in package.Workbook.Worksheets)
        {
            var sheetData = new List<Dictionary<string, object>>();
            
            if (worksheet.Dimension == null)
                continue;

            // Read headers
            var headers = new List<string>();
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                headers.Add(worksheet.Cells[1, col].Value?.ToString() ?? $"Column{col}");
            }

            // Read data rows
            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                var rowData = new Dictionary<string, object>();
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var value = worksheet.Cells[row, col].Value;
                    rowData[headers[col - 1]] = value ?? string.Empty;
                }
                sheetData.Add(rowData);
            }

            result[worksheet.Name] = sheetData;
        }

        return await Task.FromResult(result);
    }
}
