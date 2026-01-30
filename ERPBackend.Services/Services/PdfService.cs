using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ERPBackend.Core.Interfaces;
using System.Reflection;

namespace ERPBackend.Services.Services;

public class PdfService : IPdfService
{
    public PdfService()
    {
        // Set QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GeneratePdfReportAsync<T>(IEnumerable<T> data, string reportTitle) where T : class
    {
        var dataList = data.ToList();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Text(reportTitle)
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            foreach (var _ in properties)
                            {
                                columns.RelativeColumn();
                            }
                        });

                        // Header row
                        table.Header(header =>
                        {
                            foreach (var property in properties)
                            {
                                header.Cell().Element(CellStyle).Text(property.Name).SemiBold();
                            }
                        });

                        // Data rows
                        foreach (var item in dataList)
                        {
                            foreach (var property in properties)
                            {
                                var value = property.GetValue(item)?.ToString() ?? "";
                                table.Cell().Element(CellStyle).Text(value);
                            }
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        });

        return await Task.FromResult(document.GeneratePdf());
    }

    public async Task<byte[]> GeneratePdfFromTemplateAsync(Dictionary<string, object> data, string templateName)
    {
        // Basic implementation - can be enhanced with actual templates
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);

                page.Header()
                    .Text(templateName)
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        foreach (var kvp in data)
                        {
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"{kvp.Key}:").SemiBold();
                                row.RelativeItem().Text(kvp.Value?.ToString() ?? "");
                            });
                        }
                    });
            });
        });

        return await Task.FromResult(document.GeneratePdf());
    }

    public async Task<byte[]> ExportToPdfAsync<T>(IEnumerable<T> data, string[] columns, string title) where T : class
    {
        var dataList = data.ToList();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => columns.Contains(p.Name))
            .ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header()
                    .Text(title)
                    .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columnsDefinition =>
                        {
                            foreach (var _ in properties)
                            {
                                columnsDefinition.RelativeColumn();
                            }
                        });

                        table.Header(header =>
                        {
                            foreach (var property in properties)
                            {
                                header.Cell().Element(CellStyle).Text(property.Name).SemiBold();
                            }
                        });

                        foreach (var item in dataList)
                        {
                            foreach (var property in properties)
                            {
                                var value = property.GetValue(item)?.ToString() ?? "";
                                table.Cell().Element(CellStyle).Text(value);
                            }
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generated on ");
                        x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                    });
            });
        });

        return await Task.FromResult(document.GeneratePdf());
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5);
    }
}
