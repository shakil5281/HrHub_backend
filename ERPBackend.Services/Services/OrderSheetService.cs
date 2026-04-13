using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Core.DTOs;
using ERPBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using OfficeOpenXml;

namespace ERPBackend.Services.Services
{
    public class OrderSheetService : IOrderSheetService
    {
        private readonly MerchandisingDbContext _context;

        public OrderSheetService(MerchandisingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProgramOrderDto>> GetAllAsync(int companyId)
        {
            return await _context.ProgramOrders
                .Where(o => o.CompanyId == companyId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new ProgramOrderDto
                {
                    Id = o.Id,
                    CompanyId = o.CompanyId,
                    BranchId = o.BranchId,
                    ProgramNumber = o.ProgramNumber,
                    BuyerName = o.BuyerName,
                    CustomerName = o.CustomerName,
                    ProgramName = o.ProgramName,
                    OrderDate = o.OrderDate,
                    Articles = o.Articles.Select(i => new ProgramArticleDto
                    {
                        Id = i.Id,
                        ItemName = i.ItemName,
                        TotalQty = i.TotalQty
                    }).ToList()
                }).ToListAsync();
        }

        public async Task<ProgramOrder?> GetByIdAsync(int id)
        {
            return await _context.ProgramOrders
                .Include(o => o.Articles)
                    .ThenInclude(i => i.Colors)
                        .ThenInclude(c => c.SizeBreakdowns)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<ProgramOrderDto?> GetDtoByIdAsync(int id)
        {
            var o = await _context.ProgramOrders
                .Include(o => o.Articles)
                    .ThenInclude(i => i.Style)
                .Include(o => o.Articles)
                    .ThenInclude(i => i.Colors)
                        .ThenInclude(c => c.SizeBreakdowns)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (o == null) return null;

            // Fallback lookups for missing IDs (for data imported before the ID mapping fix)
            var buyers = await _context.Buyers.Where(b => b.CompanyId == o.CompanyId).ToListAsync();
            var styles = await _context.Styles.Where(s => s.CompanyId == o.CompanyId).ToListAsync();
            var colors = await _context.FabricColorPantones.Where(c => c.CompanyId == o.CompanyId).ToListAsync();

            Console.WriteLine($"[DEBUG] HEALING ORDER ID {id}. CompanyId: {o.CompanyId}");
            Console.WriteLine($"[DEBUG] Master Buyers: {string.Join(", ", buyers.Select(b => $"'{b.Name}' (ID:{b.Id})"))}");
            Console.WriteLine($"[DEBUG] Target Buyer Name: '{o.BuyerName}'");

            int? buyerId = o.BuyerId;
            if (buyerId == null && !string.IsNullOrEmpty(o.BuyerName))
            {
                var matchedBuyer = buyers.FirstOrDefault(b => b.Name.Trim().Equals(o.BuyerName.Trim(), StringComparison.OrdinalIgnoreCase));
                buyerId = matchedBuyer?.Id;
                Console.WriteLine($"[DEBUG] Matched Buyer ID: {buyerId}");
            }

            Console.WriteLine($"[DEBUG] Master Styles: {string.Join(", ", styles.Select(s => $"'{s.StyleNumber}' (ID:{s.Id})"))}");
            Console.WriteLine($"[DEBUG] Master Colors: {string.Join(", ", colors.Select(c => $"'{c.ColorName}' (ID:{c.Id})"))}");

            return new ProgramOrderDto
            {
                Id = o.Id,
                CompanyId = o.CompanyId,
                BranchId = o.BranchId,
                ProgramNumber = o.ProgramNumber,
                BuyerName = o.BuyerName,
                BuyerId = buyerId,
                CustomerName = o.CustomerName,
                FabricDescription = o.FabricDescription,
                ProgramName = o.ProgramName,
                OrderDate = o.OrderDate,
                FactoryName = o.FactoryName,
                FactoryAddress = o.FactoryAddress,
                Articles = o.Articles.Select(i => {
                    var styleId = i.StyleId;
                    if (styleId == null) {
                        var matchedStyle = styles.FirstOrDefault(s => 
                            s.StyleNumber.Trim().Equals(i.NewArticleNo?.Trim(), StringComparison.OrdinalIgnoreCase) || 
                            s.StyleNumber.Trim().Equals(i.OldArticleNo?.Trim(), StringComparison.OrdinalIgnoreCase));
                        styleId = matchedStyle?.Id;
                        Console.WriteLine($"[DEBUG] Article {i.NewArticleNo} Matched Style: {matchedStyle?.StyleNumber} (ID: {styleId})");
                    }
                    return new ProgramArticleDto
                    {
                        Id = i.Id,
                        StyleId = styleId,
                        StyleNumber = i.Style?.StyleNumber ?? styles.FirstOrDefault(s => s.Id == styleId)?.StyleNumber ?? string.Empty,
                        OldArticleNo = i.OldArticleNo,
                        NewArticleNo = i.NewArticleNo,
                        PackType = (int)i.PackType,
                        ItemName = i.ItemName,
                        TotalQty = i.TotalQty,
                        Colors = i.Colors.Select(c => {
                            var colorId = c.ColorId;
                            if (colorId == null) {
                                var matchedColor = colors.FirstOrDefault(cl => cl.ColorName.Trim().Equals(c.ColorName?.Trim(), StringComparison.OrdinalIgnoreCase));
                                colorId = matchedColor?.Id;
                                Console.WriteLine($"[DEBUG] Color {c.ColorName} Matched ID: {colorId}");
                            }
                            return new ProgramColorDto
                            {
                                Id = c.Id,
                                ColorId = colorId,
                                ColorName = c.ColorName,
                                SizeBreakdowns = c.SizeBreakdowns.Select(sb => new ProgramSizeBreakdownDto
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
                                    BuyerPackingNumber = sb.BuyerPackingNumber,
                                    ButtonColor = sb.ButtonColor,
                                    ButtonQty = sb.ButtonQty
                                }).ToList()
                            };
                        }).ToList()
                    };
                }).ToList()
            };
        }

        public async Task<ProgramOrder> CreateAsync(ProgramOrder programOrder)
        {
            foreach (var article in programOrder.Articles)
            {
                article.TotalQty = 0;
                foreach (var color in article.Colors)
                {
                    foreach (var sb in color.SizeBreakdowns)
                    {
                        sb.RowTotal = sb.SizeM + sb.SizeL + sb.SizeXL + sb.SizeXXL + sb.SizeXXXL + sb.Size3XL + sb.Size4XL + sb.Size5XL + sb.Size6XL;
                        article.TotalQty += sb.RowTotal;
                    }
                }
            }
            _context.ProgramOrders.Add(programOrder);
            await _context.SaveChangesAsync();
            return programOrder;
        }

        public async Task UpdateAsync(ProgramOrder programOrder)
        {
            var existingOrder = await _context.ProgramOrders
                .Include(o => o.Articles)
                    .ThenInclude(i => i.Colors)
                        .ThenInclude(c => c.SizeBreakdowns)
                .FirstOrDefaultAsync(o => o.Id == programOrder.Id);

            if (existingOrder == null) return;

            // Update top level
            _context.Entry(existingOrder).CurrentValues.SetValues(programOrder);

            // Clean existing children and rebuild
            if (existingOrder.Articles != null)
            {
                _context.ProgramArticles.RemoveRange(existingOrder.Articles);
            }

            existingOrder.Articles = new List<ProgramArticle>();
            
            // Clear existing button bookings for this program to rebuild from matrix
            var existingButtons = await _context.ButtonBookings.Where(b => b.ProgramOrderId == existingOrder.Id).ToListAsync();
            _context.ButtonBookings.RemoveRange(existingButtons);

            if (programOrder.Articles != null)
            {
                foreach (var article in programOrder.Articles)
                {
                    article.Id = 0;
                    article.ProgramOrderId = existingOrder.Id;
                    article.TotalQty = 0;
                    
                    foreach (var color in article.Colors)
                    {
                        color.Id = 0;
                        color.ProgramArticleId = 0;
                        
                        foreach (var sb in color.SizeBreakdowns)
                        {
                            sb.Id = 0;
                            sb.ProgramColorId = 0;
                            sb.RowTotal = sb.SizeM + sb.SizeL + sb.SizeXL + sb.SizeXXL + sb.SizeXXXL + sb.Size3XL + sb.Size4XL + sb.Size5XL + sb.Size6XL;
                            article.TotalQty += sb.RowTotal;

                            // Sync with ButtonBookings table
                            if (!string.IsNullOrEmpty(sb.ButtonColor) || (sb.ButtonQty.HasValue && sb.ButtonQty > 0))
                            {
                                var buttonBooking = new ButtonBooking
                                {
                                    ProgramOrderId = existingOrder.Id,
                                    ItemName = article.ItemName,
                                    ArticleNo = article.NewArticleNo,
                                    GarmentColor = color.ColorName,
                                    ButtonColor = sb.ButtonColor ?? string.Empty,
                                    RequiredQuantity = sb.ButtonQty ?? 0,
                                    ButtonType = "PLASTIC", // Default fallback
                                    ButtonSize = "24L",      // Default fallback
                                    Unit = "Pcs",
                                    Status = "Planned",
                                    DeliveryDate = DateTime.UtcNow.AddDays(15)
                                };
                                _context.ButtonBookings.Add(buttonBooking);
                            }
                        }
                    }
                    existingOrder.Articles.Add(article);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.ProgramOrders.FindAsync(id);
            if (order != null)
            {
                _context.ProgramOrders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public ProgramSummaryDto GetOrderSummary(int programId)
        {
            var order = _context.ProgramOrders
                .Include(o => o.Articles)
                    .ThenInclude(i => i.Colors)
                        .ThenInclude(c => c.SizeBreakdowns)
                .FirstOrDefault(o => o.Id == programId);

            if (order == null) return new ProgramSummaryDto();

            var summary = new ProgramSummaryDto
            {
                TotalOrderQuantity = order.Articles.Sum(i => i.TotalQty),
                TotalColors = order.Articles.SelectMany(i => i.Colors).Select(c => c.ColorName).Distinct().Count(),
                TotalPerSize = new Dictionary<string, int>()
            };

            // Aggregate sizes
            summary.TotalPerSize["M"] = order.Articles.SelectMany(i => i.Colors).SelectMany(c => c.SizeBreakdowns).Sum(sb => sb.SizeM);
            summary.TotalPerSize["L"] = order.Articles.SelectMany(i => i.Colors).SelectMany(c => c.SizeBreakdowns).Sum(sb => sb.SizeL);
            summary.TotalPerSize["XL"] = order.Articles.SelectMany(i => i.Colors).SelectMany(c => c.SizeBreakdowns).Sum(sb => sb.SizeXL);
            summary.TotalPerSize["XXL"] = order.Articles.SelectMany(i => i.Colors).SelectMany(c => c.SizeBreakdowns).Sum(sb => sb.SizeXXL);
            summary.TotalPerSize["XXXL"] = order.Articles.SelectMany(i => i.Colors).SelectMany(c => c.SizeBreakdowns).Sum(sb => sb.SizeXXXL);

            return summary;
        }

        public async Task<GlobalProgramSummaryDto> GetGlobalSummaryAsync(int companyId)
        {
            var programs = await _context.ProgramOrders
                .Where(o => o.CompanyId == companyId)
                .Include(o => o.Articles)
                .ToListAsync();

            return new GlobalProgramSummaryDto
            {
                TotalPrograms = programs.Count,
                TotalQuantity = programs.Sum(p => p.Articles.Sum(i => i.TotalQty)),
                TotalValue = 0 // Needs rate field in item or program
            };
        }

        // Placeholder methods for Excel to avoid massive code dump of old logic
        public async Task<byte[]> DownloadTemplateAsync()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Garment Order Sheet");

            string[] headers = {
                "Program Number", "Buyer", "Customer", "Item Name", "Article No", "Style No",
                "Season", "Fabric Description", "Color",
                "M", "L", "XL", "XXL", "XXXL", "3XL", "4XL", "5XL", "6XL", "Pack Ref"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Sample row
            worksheet.Cells[2, 1].Value = "PRG-2025-001";
            worksheet.Cells[2, 2].Value = "H&M";
            worksheet.Cells[2, 3].Value = "HM GLOBAL";
            worksheet.Cells[2, 4].Value = "BASIC T-SHIRT";
            worksheet.Cells[2, 5].Value = "ART-402";
            worksheet.Cells[2, 6].Value = "STY-99";
            worksheet.Cells[2, 7].Value = "SS25";
            worksheet.Cells[2, 8].Value = "100% COTTON JERSEY 180GSM";
            worksheet.Cells[2, 9].Value = "NAVY BLUE";
            worksheet.Cells[2, 10].Value = 100; // M
            worksheet.Cells[2, 11].Value = 200; // L
            worksheet.Cells[2, 12].Value = 150; // XL
            worksheet.Cells[2, 19].Value = "CARTON-01";

            worksheet.Cells.AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }

        public async Task<MultiSheetOrderImportDto> ParseExcelAsync(Stream fileStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];
            if (worksheet.Dimension == null) return new MultiSheetOrderImportDto { Orders = new List<OrderImportRowDto>() };
            var rowCount = worksheet.Dimension.Rows;
            var orders = new List<OrderImportRowDto>();
            
            for (int row = 2; row <= rowCount; row++)
            {
                if (string.IsNullOrEmpty(worksheet.Cells[row, 1].Text)) continue;

                var order = new OrderImportRowDto
                {
                    ProgramNumber = worksheet.Cells[row, 1].Text,
                    BuyerName = worksheet.Cells[row, 2].Text,
                    CustomerName = worksheet.Cells[row, 3].Text,
                    ItemName = worksheet.Cells[row, 4].Text,
                    NewArticleNo = worksheet.Cells[row, 5].Text,
                    StyleNo = worksheet.Cells[row, 6].Text,
                    ProgramName = worksheet.Cells[row, 7].Text,
                    Fabric = worksheet.Cells[row, 8].Text,
                    Color = worksheet.Cells[row, 9].Text,
                    SizeM = int.TryParse(worksheet.Cells[row, 10].Text, out int m) ? m : 0,
                    SizeL = int.TryParse(worksheet.Cells[row, 11].Text, out int l) ? l : 0,
                    SizeXL = int.TryParse(worksheet.Cells[row, 12].Text, out int xl) ? xl : 0,
                    SizeXXL = int.TryParse(worksheet.Cells[row, 13].Text, out int xxl) ? xxl : 0,
                    SizeXXXL = int.TryParse(worksheet.Cells[row, 14].Text, out int xxxl) ? xxxl : 0,
                    Size3XL = int.TryParse(worksheet.Cells[row, 15].Text, out int x3l) ? x3l : 0,
                    Size4XL = int.TryParse(worksheet.Cells[row, 16].Text, out int x4l) ? x4l : 0,
                    Size5XL = int.TryParse(worksheet.Cells[row, 17].Text, out int x5l) ? x5l : 0,
                    Size6XL = int.TryParse(worksheet.Cells[row, 18].Text, out int x6l) ? x6l : 0,
                    PackRef = worksheet.Cells[row, 19].Text,
                    IsValid = true
                };

                // Basic validation
                if (string.IsNullOrEmpty(order.ProgramNumber)) { order.IsValid = false; order.ErrorMessage = "Program Number is required"; }
                else if (string.IsNullOrEmpty(order.NewArticleNo)) { order.IsValid = false; order.ErrorMessage = "Article Number is required"; }

                orders.Add(order);
            }

            return new MultiSheetOrderImportDto { Orders = orders };
        }

        public async Task<int> ImportOrderSheetsAsync(MultiSheetOrderImportDto data, int cid, int bid)
        {
            if (data.Orders == null || !data.Orders.Any()) return 0;
            
            // Pre-fetch masters for lookup to speed up and ensure linking
            var buyersInDb = await _context.Buyers.Where(b => b.CompanyId == cid).ToListAsync();
            var stylesInDb = await _context.Styles.Where(s => s.CompanyId == cid).ToListAsync();
            var colorsInDb = await _context.FabricColorPantones.Where(c => c.CompanyId == cid).ToListAsync();

            var groupedOrders = data.Orders.Where(o => o.IsValid).GroupBy(o => o.ProgramNumber ?? "UNKNOWN");
            int count = 0;

            foreach (var group in groupedOrders)
            {
                var first = group.First();
                
                // Lookup or Auto-Create Buyer
                var buyerNameTrimmed = first.BuyerName?.Trim() ?? "UNKNOWN BUYER";
                var buyerId = buyersInDb.FirstOrDefault(b => b.Name.Trim().Equals(buyerNameTrimmed, StringComparison.OrdinalIgnoreCase))?.Id;
                
                if (buyerId == null)
                {
                    var newBuyer = new Buyer
                    {
                        Name = buyerNameTrimmed,
                        CompanyId = cid,
                        BranchId = bid,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Buyers.Add(newBuyer);
                    await _context.SaveChangesAsync();
                    buyersInDb.Add(newBuyer);
                    buyerId = newBuyer.Id;
                }

                var program = new ProgramOrder {
                    CompanyId = cid,
                    BranchId = bid,
                    ProgramNumber = group.Key,
                    BuyerName = buyerNameTrimmed,
                    BuyerId = buyerId,
                    CustomerName = first.CustomerName ?? "",
                    ProgramName = first.ProgramName ?? "",
                    FabricDescription = first.Fabric ?? "",
                    OrderDate = DateTime.UtcNow
                };

                // Group by Article
                var articles = group.GroupBy(o => (o.NewArticleNo ?? "Unknown Art").Trim());
                foreach (var artGrp in articles)
                {
                    var firstArt = artGrp.First();
                    var artNo = artGrp.Key;
                    
                    // Lookup or Auto-Create Style
                    var styleId = stylesInDb.FirstOrDefault(s => 
                        s.StyleNumber.Trim().Equals(artNo, StringComparison.OrdinalIgnoreCase) || 
                        s.StyleNumber.Trim().Equals(firstArt.StyleNo?.Trim(), StringComparison.OrdinalIgnoreCase))?.Id;

                    if (styleId == null)
                    {
                        var newStyle = new Style
                        {
                            StyleNumber = artNo,
                            ProductType = firstArt.ItemName ?? "Unknown Product",
                            BuyerId = buyerId ?? 0,
                            CompanyId = cid,
                            BranchId = bid,
                            Season = first.ProgramName ?? "N/A",
                            FabricType = first.Fabric ?? "N/A"
                        };
                        _context.Styles.Add(newStyle);
                        await _context.SaveChangesAsync();
                        stylesInDb.Add(newStyle);
                        styleId = newStyle.Id;
                    }

                    var article = new ProgramArticle {
                        ItemName = firstArt.ItemName ?? "Unknown Item",
                        NewArticleNo = artNo,
                        OldArticleNo = firstArt.StyleNo ?? "",
                        StyleId = styleId,
                        PackType = Core.Enums.PackType.PackA,
                        TotalQty = 0
                    };

                    foreach (var row in artGrp)
                    {
                        // Lookup or Auto-Create Color
                        var colorNameTrimmed = row.Color?.Trim() ?? "N/A";
                        var colorId = colorsInDb.FirstOrDefault(c => c.ColorName.Trim().Equals(colorNameTrimmed, StringComparison.OrdinalIgnoreCase))?.Id;

                        if (colorId == null)
                        {
                            var newColor = new FabricColorPantone
                            {
                                ColorName = colorNameTrimmed,
                                CompanyId = cid,
                                BranchId = bid,
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            };
                            _context.FabricColorPantones.Add(newColor);
                            await _context.SaveChangesAsync();
                            colorsInDb.Add(newColor);
                            colorId = newColor.Id;
                        }

                        var color = new ProgramColor {
                            ColorName = colorNameTrimmed,
                            ColorId = colorId
                        };
                        var breakdown = new ProgramSizeBreakdown {
                            SizeM = row.SizeM,
                            SizeL = row.SizeL,
                            SizeXL = row.SizeXL,
                            SizeXXL = row.SizeXXL,
                            SizeXXXL = row.SizeXXXL,
                            Size3XL = row.Size3XL,
                            Size4XL = row.Size4XL,
                            Size5XL = row.Size5XL,
                            Size6XL = row.Size6XL,
                            BuyerPackingNumber = row.PackRef ?? ""
                        };
                        breakdown.RowTotal = breakdown.SizeM + breakdown.SizeL + breakdown.SizeXL + breakdown.SizeXXL + breakdown.SizeXXXL + breakdown.Size3XL + breakdown.Size4XL + breakdown.Size5XL + breakdown.Size6XL;
                        color.SizeBreakdowns.Add(breakdown);
                        article.Colors.Add(color);
                        article.TotalQty += breakdown.RowTotal;
                    }
                    program.Articles.Add(article);
                }

                await CreateAsync(program);
                count++;
            }

            return count;
        }

        public async Task<byte[]> ExportOrderSheetAsync(int id)
        {
            var program = await _context.ProgramOrders
                .Include(o => o.Articles)
                    .ThenInclude(i => i.Colors)
                        .ThenInclude(c => c.SizeBreakdowns)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (program == null) return Array.Empty<byte>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Export");

            string[] headers = { "Item Name", "Article", "Color", "M", "L", "XL", "XXL", "XXXL", "3XL", "4XL", "5XL", "6XL", "Row Total" };
            for(int i=0; i<headers.Length; i++) ws.Cells[1, i+1].Value = headers[i];

            int row = 2;
            foreach(var art in program.Articles) {
                foreach(var col in art.Colors) {
                    foreach(var sb in col.SizeBreakdowns) {
                        ws.Cells[row, 1].Value = art.ItemName;
                        ws.Cells[row, 2].Value = art.NewArticleNo;
                        ws.Cells[row, 3].Value = col.ColorName;
                        ws.Cells[row, 4].Value = sb.SizeM;
                        ws.Cells[row, 5].Value = sb.SizeL;
                        ws.Cells[row, 6].Value = sb.SizeXL;
                        ws.Cells[row, 7].Value = sb.SizeXXL;
                        ws.Cells[row, 8].Value = sb.SizeXXXL;
                        ws.Cells[row, 9].Value = sb.Size3XL;
                        ws.Cells[row, 10].Value = sb.Size4XL;
                        ws.Cells[row, 11].Value = sb.Size5XL;
                        ws.Cells[row, 12].Value = sb.Size6XL;
                        ws.Cells[row, 13].Value = sb.RowTotal;
                        row++;
                    }
                }
            }

            ws.Cells.AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }
    }
}
