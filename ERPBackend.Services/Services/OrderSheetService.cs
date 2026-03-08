using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Core.DTOs;
using ERPBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ERPBackend.Core.Enums;

namespace ERPBackend.Services.Services
{
    public class OrderSheetService : IOrderSheetService
    {
        private readonly MerchandisingDbContext _context;

        public OrderSheetService(MerchandisingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderSheet>> GetAllAsync(int companyId)
        {
            return await _context.OrderSheets
                .Where(o => o.CompanyId == companyId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<OrderSheet?> GetByIdAsync(int id)
        {
            return await _context.OrderSheets
                .Include(o => o.Items)
                    .ThenInclude(i => i.Colors)
                        .ThenInclude(c => c.SizeBreakdowns)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<OrderSheetDto?> GetDtoByIdAsync(int id)
        {
            var o = await _context.OrderSheets
                .Include(o => o.Items)
                    .ThenInclude(i => i.Colors)
                        .ThenInclude(c => c.SizeBreakdowns)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (o == null) return null;

            return new OrderSheetDto
            {
                Id = o.Id,
                CompanyId = o.CompanyId,
                BranchId = o.BranchId,
                ProgramNumber = o.ProgramNumber,
                BuyerName = o.BuyerName,
                CustomerName = o.CustomerName,
                FabricDescription = o.FabricDescription,
                ProgramName = o.ProgramName,
                OrderDate = o.OrderDate,
                FactoryName = o.FactoryName,
                FactoryAddress = o.FactoryAddress,
                Items = o.Items.Select(i => new OrderSheetItemDto
                {
                    Id = i.Id,
                    OldArticleNo = i.OldArticleNo,
                    NewArticleNo = i.NewArticleNo,
                    PackType = i.PackType,
                    ItemName = i.ItemName,
                    TotalQty = i.TotalQty,
                    Colors = i.Colors.Select(c => new OrderSheetColorDto
                    {
                        Id = c.Id,
                        ColorName = c.ColorName,
                        SizeBreakdowns = c.SizeBreakdowns.Select(sb => new OrderSheetSizeBreakdownDto
                        {
                            Id = sb.Id,
                            SizeM = sb.SizeM,
                            SizeL = sb.SizeL,
                            SizeXL = sb.SizeXL,
                            SizeXXL = sb.SizeXXL,
                            SizeXXXL = sb.SizeXXXL,
                            Size3XL = sb.Size3XL,
                            Size4XL = sb.Size4XL,
                            Size5XL = sb.Size5XL,
                            Size6XL = sb.Size6XL,
                            RowTotal = sb.RowTotal,
                            BuyerPackingNumber = sb.BuyerPackingNumber
                        }).ToList()
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<OrderSheet> CreateAsync(OrderSheet orderSheet)
        {
            // Auto-calculate Row Totals for all size breakdowns
            foreach (var item in orderSheet.Items)
            {
                foreach (var color in item.Colors)
                {
                    foreach (var sb in color.SizeBreakdowns)
                    {
                        sb.RowTotal = CalculateRowTotal(sb);
                    }
                }
                item.TotalQty = item.Colors.Sum(c => c.SizeBreakdowns.Sum(sb => sb.RowTotal));
            }

            _context.OrderSheets.Add(orderSheet);
            await _context.SaveChangesAsync();
            return orderSheet;
        }

        public async Task UpdateAsync(OrderSheet orderSheet)
        {
            var existingOrder = await _context.OrderSheets
                .Include(o => o.Items)
                    .ThenInclude(i => i.Colors)
                        .ThenInclude(c => c.SizeBreakdowns)
                .FirstOrDefaultAsync(o => o.Id == orderSheet.Id);

            if (existingOrder == null) throw new Exception("Order Sheet not found");

            // Update header fields
            _context.Entry(existingOrder).CurrentValues.SetValues(orderSheet);

            // Re-calculate totals and sync items
            // For a "Sheet update" we often replace the dynamic grid data
            _context.OrderSheetItems.RemoveRange(existingOrder.Items);
            
            foreach (var item in orderSheet.Items)
            {
                item.Id = 0; // Reset for insertion
                foreach (var color in item.Colors)
                {
                    color.Id = 0;
                    foreach (var sb in color.SizeBreakdowns)
                    {
                        sb.Id = 0;
                        sb.RowTotal = CalculateRowTotal(sb);
                    }
                }
                item.TotalQty = item.Colors.Sum(c => c.SizeBreakdowns.Sum(sb => sb.RowTotal));
                existingOrder.Items.Add(item);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.OrderSheets.FindAsync(id);
            if (order != null)
            {
                _context.OrderSheets.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        private int CalculateRowTotal(OrderSheetSizeBreakdown sb)
        {
            return sb.SizeM + sb.SizeL + sb.SizeXL + sb.SizeXXL + sb.SizeXXXL + 
                   sb.Size3XL + sb.Size4XL + sb.Size5XL + sb.Size6XL;
        }

        public int CalculatePackQuantity(int packA, int packB, bool isCombined)
        {
            return isCombined ? packA + packB : packA;
        }

        public OrderSummaryDto GetOrderSummary(int orderSheetId)
        {
            var orderSheet = _context.OrderSheets
                .Include(o => o.Items)
                    .ThenInclude(i => i.Colors)
                        .ThenInclude(c => c.SizeBreakdowns)
                .FirstOrDefault(o => o.Id == orderSheetId);

            if (orderSheet == null) return new OrderSummaryDto();

            var summary = new OrderSummaryDto();
            summary.TotalOrderQuantity = orderSheet.Items.Sum(i => i.TotalQty);
            summary.TotalColors = orderSheet.Items.SelectMany(i => i.Colors).Select(c => c.ColorName).Distinct().Count();
            
            // Per size totals
            var allSized = orderSheet.Items.SelectMany(i => i.Colors).SelectMany(c => c.SizeBreakdowns);
            summary.TotalPerSize.Add("M", allSized.Sum(s => s.SizeM));
            summary.TotalPerSize.Add("L", allSized.Sum(s => s.SizeL));
            summary.TotalPerSize.Add("XL", allSized.Sum(s => s.SizeXL));
            summary.TotalPerSize.Add("XXL", allSized.Sum(s => s.SizeXXL));
            summary.TotalPerSize.Add("XXXL", allSized.Sum(s => s.SizeXXXL));
            summary.TotalPerSize.Add("3XL", allSized.Sum(s => s.Size3XL));
            summary.TotalPerSize.Add("4XL", allSized.Sum(s => s.Size4XL));
            summary.TotalPerSize.Add("5XL", allSized.Sum(s => s.Size5XL));
            summary.TotalPerSize.Add("6XL", allSized.Sum(s => s.Size6XL));

            return summary;
        }

        public async Task<GlobalOrderSummaryDto> GetGlobalSummaryAsync(int companyId)
        {
            var orders = await _context.OrderSheets
                .Include(o => o.Items)
                    .ThenInclude(i => i.Colors)
                        .ThenInclude(c => c.SizeBreakdowns)
                .Where(o => o.CompanyId == companyId)
                .ToListAsync();

            var summary = new GlobalOrderSummaryDto
            {
                TotalPrograms = orders.Count,
                TotalBuyers = orders.Select(o => o.BuyerName).Distinct().Count(),
                TotalPieces = orders.Sum(o => o.Items.Sum(i => i.TotalQty))
            };

            // Buyer Distribution
            summary.BuyerDistribution = orders
                .GroupBy(o => o.BuyerName)
                .Select(g => new BuyerDistributionDto
                {
                    BuyerName = g.Key,
                    TotalQty = g.Sum(o => o.Items.Sum(i => i.TotalQty)),
                    Percentage = summary.TotalPieces > 0 ? (double)g.Sum(o => o.Items.Sum(i => i.TotalQty)) / summary.TotalPieces * 100 : 0
                })
                .OrderByDescending(b => b.TotalQty)
                .ToList();

            // Aggregated Size Distribution
            var allBreakdowns = orders.SelectMany(o => o.Items).SelectMany(i => i.Colors).SelectMany(c => c.SizeBreakdowns).ToList();
            summary.SizeDistribution = new List<SizeDistributionDto>
            {
                new SizeDistributionDto { SizeName = "M", TotalQty = allBreakdowns.Sum(s => s.SizeM) },
                new SizeDistributionDto { SizeName = "L", TotalQty = allBreakdowns.Sum(s => s.SizeL) },
                new SizeDistributionDto { SizeName = "XL", TotalQty = allBreakdowns.Sum(s => s.SizeXL) },
                new SizeDistributionDto { SizeName = "XXL", TotalQty = allBreakdowns.Sum(s => s.SizeXXL) },
                new SizeDistributionDto { SizeName = "XXXL", TotalQty = allBreakdowns.Sum(s => s.SizeXXXL) },
                new SizeDistributionDto { SizeName = "3XL+", TotalQty = allBreakdowns.Sum(s => s.Size3XL + s.Size4XL + s.Size5XL + s.Size6XL) }
            };

            // Recent Programs
            summary.RecentPrograms = orders
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new ProgramSummaryDto
                {
                    Id = o.Id,
                    ProgramNumber = o.ProgramNumber,
                    BuyerName = o.BuyerName,
                    TotalQty = o.Items.Sum(i => i.TotalQty),
                    OrderDate = o.OrderDate
                })
                .ToList();

            return summary;
        }

        public async Task<List<OrderSheetImportDto>> ParseExcelAsync(Stream fileStream)
        {
            var results = new List<OrderSheetImportDto>();
            using var package = new OfficeOpenXml.ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++) // Header is at row 1
            {
                var dto = new OrderSheetImportDto
                {
                    RowIndex = row,
                    ProgramNumber = worksheet.Cells[row, 1].Text,
                    BuyerName = worksheet.Cells[row, 2].Text,
                    CustomerName = worksheet.Cells[row, 3].Text,
                    OrderDate = DateTime.TryParse(worksheet.Cells[row, 4].Text, out DateTime dt) ? dt : DateTime.UtcNow,
                    OldArticleNo = worksheet.Cells[row, 5].Text,
                    NewArticleNo = worksheet.Cells[row, 6].Text,
                    PackType = worksheet.Cells[row, 7].Text,
                    ItemName = worksheet.Cells[row, 8].Text,
                    FabricDescription = worksheet.Cells[row, 9].Text,
                    Color = worksheet.Cells[row, 10].Text,
                    SizeM = int.TryParse(worksheet.Cells[row, 11].Text, out int m) ? m : 0,
                    SizeL = int.TryParse(worksheet.Cells[row, 12].Text, out int l) ? l : 0,
                    SizeXL = int.TryParse(worksheet.Cells[row, 13].Text, out int xl) ? xl : 0,
                    SizeXXL = int.TryParse(worksheet.Cells[row, 14].Text, out int xxl) ? xxl : 0,
                    SizeXXXL = int.TryParse(worksheet.Cells[row, 15].Text, out int xxxl) ? xxxl : 0,
                    Size3XL = int.TryParse(worksheet.Cells[row, 16].Text, out int s3) ? s3 : 0,
                    Size4XL = int.TryParse(worksheet.Cells[row, 17].Text, out int s4) ? s4 : 0,
                    Size5XL = int.TryParse(worksheet.Cells[row, 18].Text, out int s5) ? s5 : 0,
                    Size6XL = int.TryParse(worksheet.Cells[row, 19].Text, out int s6) ? s6 : 0,
                    BuyerPackingNumber = worksheet.Cells[row, 20].Text
                };

                // Simple validation
                if (string.IsNullOrWhiteSpace(dto.ProgramNumber)) { dto.IsValid = false; dto.ErrorMessage += "ProgramNumber required; "; }
                if (string.IsNullOrWhiteSpace(dto.Color)) { dto.IsValid = false; dto.ErrorMessage += "Color required; "; }

                results.Add(dto);
            }

            return results;
        }

        public async Task<int> ImportOrderSheetsAsync(List<OrderSheetImportDto> importData, int companyId, int branchId)
        {
            // Efficiency: Group by program (OrderSheet) -> Group by item (OrderSheetItem)
            var programs = importData.GroupBy(d => d.ProgramNumber);
            int count = 0;

            foreach (var programGroup in programs)
            {
                var first = programGroup.First();
                var orderSheet = new OrderSheet
                {
                    ProgramNumber = first.ProgramNumber,
                    BuyerName = first.BuyerName,
                    CustomerName = first.CustomerName,
                    OrderDate = first.OrderDate,
                    FabricDescription = first.FabricDescription,
                    CompanyId = companyId,
                    BranchId = branchId,
                    Items = new List<OrderSheetItem>()
                };

                // Group by Article / Item combination
                var itemGroups = programGroup.GroupBy(d => new { d.OldArticleNo, d.NewArticleNo, d.ItemName });
                foreach (var itemGroup in itemGroups)
                {
                    var firstItem = itemGroup.First();
                    var packType = firstItem.PackType?.ToLower() switch
                    {
                        "packa" => PackType.PackA,
                        "packb" => PackType.PackB,
                        _ => PackType.PackAB
                    };

                    var orderItem = new OrderSheetItem
                    {
                        OldArticleNo = firstItem.OldArticleNo,
                        NewArticleNo = firstItem.NewArticleNo,
                        ItemName = firstItem.ItemName,
                        PackType = packType,
                        Colors = new List<OrderSheetColor>()
                    };

                    // Group by Color
                    var colorGroups = itemGroup.GroupBy(d => d.Color);
                    foreach (var colorGroup in colorGroups)
                    {
                        var orderColor = new OrderSheetColor
                        {
                            ColorName = colorGroup.Key,
                            SizeBreakdowns = new List<OrderSheetSizeBreakdown>()
                        };

                        foreach (var row in colorGroup)
                        {
                            var sb = new OrderSheetSizeBreakdown
                            {
                                SizeM = row.SizeM,
                                SizeL = row.SizeL,
                                SizeXL = row.SizeXL,
                                SizeXXL = row.SizeXXL,
                                SizeXXXL = row.SizeXXXL,
                                Size3XL = row.Size3XL,
                                Size4XL = row.Size4XL,
                                Size5XL = row.Size5XL,
                                Size6XL = row.Size6XL,
                                BuyerPackingNumber = row.BuyerPackingNumber,
                                RowTotal = row.SizeM + row.SizeL + row.SizeXL + row.SizeXXL + row.SizeXXXL + row.Size3XL + row.Size4XL + row.Size5XL + row.Size6XL
                            };
                            orderColor.SizeBreakdowns.Add(sb);
                        }
                        orderItem.Colors.Add(orderColor);
                    }
                    orderItem.TotalQty = orderItem.Colors.Sum(c => c.SizeBreakdowns.Sum(s => s.RowTotal));
                    orderSheet.Items.Add(orderItem);
                }

                _context.OrderSheets.Add(orderSheet);
                count++;
            }

            await _context.SaveChangesAsync();
            return count;
        }

        public async Task<byte[]> DownloadTemplateAsync()
        {
            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("OrderSheet_Import_Template");

            // Define Columns
            string[] headers = {
                "ProgramNumber", "BuyerName", "CustomerName", "OrderDate", 
                "OldArticleNo", "NewArticleNo", "PackType", "ItemName", "FabricDescription",
                "Color", "Size_M", "Size_L", "Size_XL", "Size_XXL", "Size_XXXL", 
                "Size_3XL", "Size_4XL", "Size_5XL", "Size_6XL", "BuyerPackingNumber"
            };

            // Formatting
            var headerRange = worksheet.Cells[1, 1, 1, headers.Length];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(31, 73, 125));
            headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White);
            headerRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }

            // Demo Data - Multi Article Sample
            // Article 1
            var article1 = new[] {
                new { Color = "NAVY", M = 120, L = 150, XL = 150, XXL = 80, XXXL = 40 },
                new { Color = "BLACK", M = 100, L = 120, XL = 120, XXL = 60, XXXL = 30 }
            };

            int row = 2;
            foreach(var c in article1) {
                worksheet.Cells[row, 1].Value = "DEMO-001";
                worksheet.Cells[row, 2].Value = "ANTONY SRL";
                worksheet.Cells[row, 3].Value = "RIFLE";
                worksheet.Cells[row, 4].Value = DateTime.Now.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 5].Value = "A-100";
                worksheet.Cells[row, 6].Value = "N-101";
                worksheet.Cells[row, 7].Value = "PackA";
                worksheet.Cells[row, 8].Value = "L/S POLO";
                worksheet.Cells[row, 9].Value = "100% COTTON 180GSM";
                worksheet.Cells[row, 10].Value = c.Color;
                worksheet.Cells[row, 11].Value = c.M;
                worksheet.Cells[row, 12].Value = c.L;
                worksheet.Cells[row, 13].Value = c.XL;
                worksheet.Cells[row, 14].Value = c.XXL;
                worksheet.Cells[row, 15].Value = c.XXXL;
                worksheet.Cells[row, 20].Value = "P-01";
                row++;
            }

            // Article 2
            var article2 = new[] {
                new { Color = "WHITE", M = 80, L = 80, XL = 80, XXL = 40, XXXL = 20 },
                new { Color = "RED", M = 50, L = 50, XL = 50, XXL = 20, XXXL = 10 }
            };
            foreach(var c in article2) {
                worksheet.Cells[row, 1].Value = "DEMO-001"; // Belong to same program
                worksheet.Cells[row, 2].Value = "ANTONY SRL";
                worksheet.Cells[row, 3].Value = "RIFLE";
                worksheet.Cells[row, 4].Value = DateTime.Now.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 5].Value = "B-200";
                worksheet.Cells[row, 6].Value = "M-202";
                worksheet.Cells[row, 7].Value = "PackA";
                worksheet.Cells[row, 8].Value = "S/S TEE";
                worksheet.Cells[row, 9].Value = "JERSEY 160GSM";
                worksheet.Cells[row, 10].Value = c.Color;
                worksheet.Cells[row, 11].Value = c.M;
                worksheet.Cells[row, 12].Value = c.L;
                worksheet.Cells[row, 13].Value = c.XL;
                worksheet.Cells[row, 14].Value = c.XXL;
                worksheet.Cells[row, 15].Value = c.XXXL;
                worksheet.Cells[row, 20].Value = "P-02";
                row++;
            }

            worksheet.Cells.AutoFitColumns();
            return await Task.FromResult(package.GetAsByteArray());
        }

        public async Task<byte[]> ExportOrderSheetAsync(int id)
        {
            var order = await _context.OrderSheets
                .Include(o => o.Items)
                    .ThenInclude(i => i.Colors)
                        .ThenInclude(c => c.SizeBreakdowns)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) throw new Exception("Order Sheet not found");

            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Production_Program_Analysis");

            // Page Setup
            worksheet.PrinterSettings.Orientation = OfficeOpenXml.eOrientation.Landscape;
            worksheet.PrinterSettings.FitToPage = true;

            // Global Styles
            var allCells = worksheet.Cells["A1:Z500"];
            allCells.Style.Font.Name = "Segoe UI";
            allCells.Style.Font.Size = 9;

            // 1. Program Header Section (F8FAFC Background)
            var headerBox = worksheet.Cells["A1:L5"];
            headerBox.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerBox.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(248, 250, 252));
            headerBox.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin, System.Drawing.Color.FromArgb(226, 232, 240));

            // Labels Row 2
            void SetHeaderLabel(int row, int col, string label) {
                worksheet.Cells[row, col].Value = label;
                worksheet.Cells[row, col].Style.Font.Size = 7.5f;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(100, 116, 139));
            }
            void SetHeaderValue(int row, int col, string value) {
                worksheet.Cells[row, col].Value = value;
                worksheet.Cells[row, col].Style.Font.Size = 11;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(15, 23, 42));
            }

            SetHeaderLabel(2, 1, "PROGRAM #");
            SetHeaderLabel(2, 4, "BUYER ENTITY");
            SetHeaderLabel(2, 7, "CUSTOMER");
            SetHeaderLabel(2, 10, "ORDER DATE");
            
            SetHeaderValue(3, 1, (order.ProgramNumber ?? "---").ToUpper());
            SetHeaderValue(3, 4, (order.BuyerName ?? "---").ToUpper());
            SetHeaderValue(3, 7, (order.CustomerName ?? "RIFLE").ToUpper());
            SetHeaderValue(3, 10, order.OrderDate.ToString("dd MMM yyyy").ToUpper());

            SetHeaderLabel(4, 1, "FABRIC COMPOSITION");
            SetHeaderLabel(4, 7, "PROGRAM CYCLE");

            worksheet.Cells[5, 1].Value = order.FabricDescription ?? "---";
            worksheet.Cells[5, 1].Style.Font.Size = 10;
            worksheet.Cells[5, 1].Style.Font.Bold = true;
            worksheet.Cells[5, 1].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(71, 85, 105));

            worksheet.Cells[5, 7].Value = order.ProgramName ?? "---";
            worksheet.Cells[5, 7].Style.Font.Size = 10;
            worksheet.Cells[5, 7].Style.Font.Bold = true;

            // 2. Table Headers (Dark Slate & Grey)
            int tableStartRow = 7;
            
            // Header Level 1 (Dark BG)
            var h1 = worksheet.Cells[tableStartRow, 1, tableStartRow, 20];
            h1.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            h1.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(15, 23, 42));
            h1.Style.Font.Color.SetColor(System.Drawing.Color.White);
            h1.Style.Font.Bold = true;
            h1.Style.Font.Size = 8;
            h1.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            h1.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            worksheet.Row(tableStartRow).Height = 25;

            worksheet.Cells[tableStartRow, 1].Value = "SL";
            worksheet.Cells[tableStartRow, 2].Value = "OLD ART";
            worksheet.Cells[tableStartRow, 3].Value = "NEW ART";
            worksheet.Cells[tableStartRow, 4].Value = "LABEL";
            worksheet.Cells[tableStartRow, 5].Value = "PACK";
            worksheet.Cells[tableStartRow, 6].Value = "ITEM";
            worksheet.Cells[tableStartRow, 7].Value = "TOTAL QTY";
            worksheet.Cells[tableStartRow, 8].Value = "COLOR";
            worksheet.Cells[tableStartRow, 9, tableStartRow, 17].Merge = true;
            worksheet.Cells[tableStartRow, 9].Value = "SIZE BREAKDOWN MATRIX";
            worksheet.Cells[tableStartRow, 18].Value = "ROW QTY";
            worksheet.Cells[tableStartRow, 19].Value = "G.TOTAL";
            worksheet.Cells[tableStartRow, 20].Value = "BUYER ORDER#";

            // Header Level 2 (Sub-headers for Sizes)
            int h2Row = tableStartRow + 1;
            var h2 = worksheet.Cells[h2Row, 8, h2Row, 18];
            h2.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            h2.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(30, 41, 59));
            h2.Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(148, 163, 184));
            h2.Style.Font.Size = 7.5f;
            h2.Style.Font.Bold = true;
            h2.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Row(h2Row).Height = 18;

            string[] sizes = { "M", "L", "XL", "XXL", "XXXL", "3XL", "4XL", "5XL", "6XL" };
            for (int i = 0; i < sizes.Length; i++) {
                worksheet.Cells[h2Row, 9 + i].Value = sizes[i];
            }

            // Merge Level 1 headers vertically where needed
            int[] mergeCols = { 1, 2, 3, 4, 5, 6, 7, 19, 20 };
            foreach (int c in mergeCols) {
                worksheet.Cells[tableStartRow, c, h2Row, c].Merge = true;
            }

            // 3. Data Rows
            int currentRow = h2Row + 1;
            int itemSl = 1;

            foreach (var item in order.Items)
            {
                int itemStartRow = currentRow;
                
                // Color iteration
                foreach (var color in item.Colors)
                {
                    foreach (var sb in color.SizeBreakdowns)
                    {
                        worksheet.Row(currentRow).Height = 22;
                        
                        // Color Name
                        worksheet.Cells[currentRow, 8].Value = color.ColorName.ToUpper();
                        worksheet.Cells[currentRow, 8].Style.Font.Bold = true;
                        worksheet.Cells[currentRow, 8].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        worksheet.Cells[currentRow, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(248, 250, 252));

                        // Sizes
                        int[] vals = { sb.SizeM, sb.SizeL, sb.SizeXL, sb.SizeXXL, sb.SizeXXXL, sb.Size3XL, sb.Size4XL, sb.Size5XL, sb.Size6XL };
                        for (int v = 0; v < vals.Length; v++) {
                            worksheet.Cells[currentRow, 9 + v].Value = vals[v];
                            worksheet.Cells[currentRow, 9 + v].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        }

                        // Row Qty
                        worksheet.Cells[currentRow, 18].Value = sb.RowTotal;
                        worksheet.Cells[currentRow, 18].Style.Font.Bold = true;
                        worksheet.Cells[currentRow, 18].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(194, 65, 12));
                        worksheet.Cells[currentRow, 18].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                        // Buyer Packing / Order #
                        worksheet.Cells[currentRow, 20].Value = sb.BuyerPackingNumber;
                        worksheet.Cells[currentRow, 20].Style.Font.Size = 7.5f;
                        worksheet.Cells[currentRow, 20].Style.Font.Italic = true;
                        worksheet.Cells[currentRow, 20].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                        // Borders for internal rows
                        worksheet.Cells[currentRow, 8, currentRow, 20].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        worksheet.Cells[currentRow, 8, currentRow, 20].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.FromArgb(241, 245, 249));

                        currentRow++;
                    }
                }

                int itemEndRow = currentRow - 1;

                // Set/Merge Item Level Fields
                void SetItemCell(int col, object value, bool bold = true, System.Drawing.Color? textColor = null) {
                    var cell = worksheet.Cells[itemStartRow, col, itemEndRow, col];
                    cell.Merge = true;
                    cell.Value = value;
                    cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    cell.Style.Font.Bold = bold;
                    if (textColor.HasValue) cell.Style.Font.Color.SetColor(textColor.Value);
                }

                SetItemCell(1, itemSl++);
                SetItemCell(2, item.OldArticleNo);
                SetItemCell(3, item.NewArticleNo, true, System.Drawing.Color.FromArgb(37, 99, 235));
                SetItemCell(4, order.CustomerName ?? "RIFLE");
                SetItemCell(5, item.PackType == PackType.PackA ? "A" : item.PackType == PackType.PackB ? "B" : "A-B");
                SetItemCell(6, item.ItemName.ToUpper());
                SetItemCell(7, item.TotalQty.ToString("N0"));
                
                // G.Total Item Cell (Special Dark Styling)
                var gTotalCell = worksheet.Cells[itemStartRow, 19, itemEndRow, 19];
                gTotalCell.Merge = true;
                gTotalCell.Value = item.TotalQty;
                gTotalCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                gTotalCell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(15, 23, 42));
                gTotalCell.Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(251, 191, 36));
                gTotalCell.Style.Font.Bold = true;
                gTotalCell.Style.Font.Size = 11;
                gTotalCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                gTotalCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                // Add left border to item first column
                worksheet.Cells[itemStartRow, 1, itemEndRow, 20].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin, System.Drawing.Color.FromArgb(226, 232, 240));
            }

            // 4. Summary Row
            var summaryRange = worksheet.Cells[currentRow, 1, currentRow, 20];
            summaryRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            summaryRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(248, 250, 252));
            worksheet.Row(currentRow).Height = 30;

            worksheet.Cells[currentRow, 1, currentRow, 7].Merge = true;
            worksheet.Cells[currentRow, 1].Value = "SUMMARY AGGREGATION";
            worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 1].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(100, 116, 139));
            worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            worksheet.Cells[currentRow, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            worksheet.Cells[currentRow, 8, currentRow, 18].Merge = true;
            worksheet.Cells[currentRow, 8].Value = "GRAND PROGRAM ACCUMULATION";
            worksheet.Cells[currentRow, 8].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 8].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(100, 116, 139));
            worksheet.Cells[currentRow, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            worksheet.Cells[currentRow, 8].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // Grand Total (Bright Orange BG)
            var grandTotal = order.Items.Sum(i => i.TotalQty);
            var grandCell = worksheet.Cells[currentRow, 19];
            grandCell.Value = grandTotal.ToString("N0") + " PCS";
            grandCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            grandCell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(234, 88, 12));
            grandCell.Style.Font.Color.SetColor(System.Drawing.Color.White);
            grandCell.Style.Font.Bold = true;
            grandCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            grandCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // Final Formatting
            worksheet.Column(1).Width = 5;   // SL
            worksheet.Column(2).Width = 12;  // Old Art
            worksheet.Column(3).Width = 12;  // New Art
            worksheet.Column(4).Width = 12;  // Label
            worksheet.Column(5).Width = 8;   // Pack
            worksheet.Column(6).Width = 15;  // Item
            worksheet.Column(7).Width = 12;  // Total Qty
            worksheet.Column(8).Width = 15;  // Color
            for (int i = 9; i <= 17; i++) worksheet.Column(i).Width = 5.5; // Sizes
            worksheet.Column(18).Width = 10; // Row Qty
            worksheet.Column(19).Width = 12; // G.Total
            worksheet.Column(20).Width = 20; // Buyer Order#

            return await Task.FromResult(package.GetAsByteArray());
        }
    }
}
