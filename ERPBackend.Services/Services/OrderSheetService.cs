using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using OfficeOpenXml;
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

        public async Task<IEnumerable<OrderSheetDto>> GetAllAsync(int companyId)
        {
            return await _context.OrderSheets
                .Include(o => o.Items)
                .Where(o => o.CompanyId == companyId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderSheetDto
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
                        StyleId = i.StyleId ?? 0,
                        OldArticleNo = i.OldArticleNo,
                        NewArticleNo = i.NewArticleNo,
                        PackType = i.PackType,
                        ItemName = i.ItemName,
                        TotalQty = i.TotalQty
                    }).ToList()
                })
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

            var buyer = await _context.Buyers.FirstOrDefaultAsync(b => b.Name == o.BuyerName);

            return new OrderSheetDto
            {
                Id = o.Id,
                CompanyId = o.CompanyId,
                BranchId = o.BranchId,
                ProgramNumber = o.ProgramNumber,
                BuyerId = buyer?.Id ?? 0,
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
                    StyleId = i.StyleId ?? 0,
                    OldArticleNo = i.OldArticleNo,
                    NewArticleNo = i.NewArticleNo,
                    PackType = i.PackType,
                    ItemName = i.ItemName,
                    TotalQty = i.TotalQty,
                    Colors = i.Colors.Select(c => new OrderSheetColorDto
                    {
                        Id = c.Id,
                        ColorId = c.ColorId ?? 0,
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
            orderSheet.ProgramNumber = (orderSheet.ProgramNumber ?? "").Trim().ToUpper();
            
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
            orderSheet.ProgramNumber = (orderSheet.ProgramNumber ?? "").Trim().ToUpper();

            var existingOrder = await _context.OrderSheets
                .Include(o => o.Items)
                    .ThenInclude(i => i.Colors)
                        .ThenInclude(c => c.SizeBreakdowns)
                .FirstOrDefaultAsync(o => o.Id == orderSheet.Id);

            if (existingOrder == null) throw new Exception("Order Sheet not found");

            // Update header fields (copies only scalar properties)
            _context.Entry(existingOrder).CurrentValues.SetValues(orderSheet);

            // Re-calculate totals and sync items
            // Remove existing items and their entire trees
            if (existingOrder.Items != null && existingOrder.Items.Any())
            {
                _context.OrderSheetItems.RemoveRange(existingOrder.Items);
            }

            // Sync collection: It's safer to clear the existing list before adding new ones
            // especially when the context is still tracking the parent.
            existingOrder.Items ??= new List<OrderSheetItem>();
            
            if (orderSheet.Items != null)
            {
                foreach (var item in orderSheet.Items)
                {
                    // Reset all IDs to 0 to ensure insertion of new records
                    item.Id = 0;
                    item.OrderSheetId = existingOrder.Id;
                    
                    if (item.Colors != null)
                    {
                        foreach (var color in item.Colors)
                        {
                            color.Id = 0;
                            color.OrderSheetItemId = 0;

                            if (color.SizeBreakdowns != null)
                            {
                                foreach (var sb in color.SizeBreakdowns)
                                {
                                    sb.Id = 0;
                                    sb.OrderSheetColorId = 0;
                                    sb.RowTotal = CalculateRowTotal(sb);
                                }
                            }
                        }
                        item.TotalQty = item.Colors.Sum(c => c.SizeBreakdowns.Sum(sb => sb.RowTotal));
                    }
                    
                    existingOrder.Items.Add(item);
                }
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

        public async Task<MultiSheetOrderImportDto> ParseExcelAsync(Stream fileStream)
        {
            var result = new MultiSheetOrderImportDto();
            using var package = new OfficeOpenXml.ExcelPackage(fileStream);

            // 1. Process Styles Sheet
            var styleSheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Equals("Styles", StringComparison.OrdinalIgnoreCase)) ?? package.Workbook.Worksheets[0];
            var styleRows = styleSheet.Dimension?.Rows ?? 0;
            for (int row = 2; row <= styleRows; row++)
            {
                var styleNo = styleSheet.Cells[row, 1].Value?.ToString();
                if (string.IsNullOrWhiteSpace(styleNo)) continue;

                result.Styles.Add(new StyleImportDto
                {
                    StyleNumber = styleNo,
                    BuyerName = styleSheet.Cells[row, 2].Value?.ToString() ?? "",
                    ProductType = styleSheet.Cells[row, 3].Value?.ToString() ?? "",
                    Season = styleSheet.Cells[row, 4].Value?.ToString() ?? "",
                    FabricType = styleSheet.Cells[row, 5].Value?.ToString() ?? "",
                    GSM = styleSheet.Cells[row, 6].Value?.ToString() ?? "",
                    SizeRange = styleSheet.Cells[row, 7].Value?.ToString() ?? "",
                    RowIndex = row
                });
            }

            // 2. Process Colors Sheet
            var colorSheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Equals("Colors", StringComparison.OrdinalIgnoreCase));
            if (colorSheet != null)
            {
                var colorRows = colorSheet.Dimension?.Rows ?? 0;
                for (int row = 2; row <= colorRows; row++)
                {
                    var colorName = colorSheet.Cells[row, 1].Value?.ToString();
                    if (string.IsNullOrWhiteSpace(colorName)) continue;

                    result.Colors.Add(new ColorImportDto
                    {
                        ColorName = colorName,
                        PantoneCode = colorSheet.Cells[row, 2].Value?.ToString() ?? "",
                        RowIndex = row
                    });
                }
            }

            // 3. Process Orders Sheet
            var orderSheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Equals("Orders", StringComparison.OrdinalIgnoreCase)) ?? package.Workbook.Worksheets[0];
            var orderRows = orderSheet.Dimension?.Rows ?? 0;

            for (int row = 2; row <= orderRows; row++)
            {
                var programNo = orderSheet.Cells[row, 1].Value?.ToString();
                if (string.IsNullOrWhiteSpace(programNo)) continue;

                var dto = new OrderSheetImportDto
                {
                    RowIndex = row,
                    ProgramNumber = programNo,
                    BuyerName = orderSheet.Cells[row, 2].Value?.ToString() ?? "",
                    CustomerName = orderSheet.Cells[row, 3].Value?.ToString() ?? "",
                    ProgramName = orderSheet.Cells[row, 4].Value?.ToString() ?? "",
                    OrderDate = DateTime.TryParse(orderSheet.Cells[row, 5].Value?.ToString(), out DateTime dt) ? dt : DateTime.UtcNow,
                    FactoryName = orderSheet.Cells[row, 6].Value?.ToString() ?? "",
                    OldArticleNo = orderSheet.Cells[row, 7].Value?.ToString() ?? "",
                    NewArticleNo = orderSheet.Cells[row, 8].Value?.ToString() ?? "",
                    PackType = orderSheet.Cells[row, 9].Value?.ToString() ?? "",
                    ItemName = orderSheet.Cells[row, 10].Value?.ToString() ?? "",
                    FabricDescription = orderSheet.Cells[row, 11].Value?.ToString() ?? "",
                    Color = orderSheet.Cells[row, 12].Value?.ToString() ?? "",
                    SizeM = int.TryParse(orderSheet.Cells[row, 13].Value?.ToString(), out int m) ? m : 0,
                    SizeL = int.TryParse(orderSheet.Cells[row, 14].Value?.ToString(), out int l) ? l : 0,
                    SizeXL = int.TryParse(orderSheet.Cells[row, 15].Value?.ToString(), out int xl) ? xl : 0,
                    SizeXXL = int.TryParse(orderSheet.Cells[row, 16].Value?.ToString(), out int xxl) ? xxl : 0,
                    SizeXXXL = int.TryParse(orderSheet.Cells[row, 17].Value?.ToString(), out int xxxl) ? xxxl : 0,
                    Size3XL = int.TryParse(orderSheet.Cells[row, 18].Value?.ToString(), out int s3) ? s3 : 0,
                    Size4XL = int.TryParse(orderSheet.Cells[row, 19].Value?.ToString(), out int s4) ? s4 : 0,
                    Size5XL = int.TryParse(orderSheet.Cells[row, 20].Value?.ToString(), out int s5) ? s5 : 0,
                    Size6XL = int.TryParse(orderSheet.Cells[row, 21].Value?.ToString(), out int s6) ? s6 : 0,
                    BuyerPackingNumber = orderSheet.Cells[row, 22].Value?.ToString() ?? ""
                };

                // Simple validation
                if (string.IsNullOrWhiteSpace(dto.ProgramNumber)) { dto.IsValid = false; dto.ErrorMessage += "ProgramNumber required; "; }
                if (string.IsNullOrWhiteSpace(dto.Color)) { dto.IsValid = false; dto.ErrorMessage += "Color required; "; }

                result.Orders.Add(dto);
            }

            return result;
        }

        public async Task<int> ImportOrderSheetsAsync(MultiSheetOrderImportDto importData, int companyId, int branchId)
        {
            // 1. Process Styles (Create or Overwrite)
            var allBuyers = await _context.Buyers.Where(b => b.CompanyId == companyId).ToListAsync();
            var allStyles = await _context.Styles.Where(s => s.CompanyId == companyId).ToListAsync();
            var allBrands = await _context.Brands
                .Include(b => b.Buyer)
                .Where(b => b.Buyer != null && b.Buyer.CompanyId == companyId)
                .ToListAsync();

            foreach (var sDto in importData.Styles)
            {
                var buyer = allBuyers.FirstOrDefault(b => b.Name.Equals(sDto.BuyerName, StringComparison.OrdinalIgnoreCase));
                if (buyer == null)
                {
                    buyer = new Buyer { Name = sDto.BuyerName, CompanyId = companyId, BranchId = branchId };
                    _context.Buyers.Add(buyer);
                    await _context.SaveChangesAsync();
                    allBuyers.Add(buyer);
                }

                var brand = allBrands.FirstOrDefault(b => b.BuyerId == buyer.Id);
                if (brand == null)
                {
                    brand = new Brand
                    {
                        BuyerId = buyer.Id,
                        Name = $"{buyer.Name} Default"
                    };
                    _context.Brands.Add(brand);
                    await _context.SaveChangesAsync();
                    allBrands.Add(brand);
                }

                var existingStyle = allStyles.FirstOrDefault(s => s.StyleNumber.Equals(sDto.StyleNumber, StringComparison.OrdinalIgnoreCase) && s.BuyerId == buyer.Id);
                if (existingStyle != null)
                {
                    existingStyle.ProductType = sDto.ProductType;
                    existingStyle.Season = sDto.Season;
                    existingStyle.FabricType = sDto.FabricType;
                    existingStyle.GSM = sDto.GSM;
                    existingStyle.SizeRange = sDto.SizeRange;
                    if (existingStyle.BrandId == null)
                    {
                        existingStyle.BrandId = brand.Id;
                    }
                    _context.Styles.Update(existingStyle);
                }
                else
                {
                    var newStyle = new Style
                    {
                        StyleNumber = sDto.StyleNumber,
                        BuyerId = buyer.Id,
                        ProductType = sDto.ProductType,
                        Season = sDto.Season,
                        FabricType = sDto.FabricType,
                        GSM = sDto.GSM,
                        SizeRange = sDto.SizeRange,
                        BrandId = brand.Id,
                        CompanyId = companyId,
                        BranchId = branchId
                    };
                    _context.Styles.Add(newStyle);
                    await _context.SaveChangesAsync();
                    allStyles.Add(newStyle);
                }
            }
            await _context.SaveChangesAsync();

            // 2. Process Colors (Create or Overwrite)
            var allColors = await _context.FabricColorPantones.Where(c => c.CompanyId == companyId).ToListAsync();
            foreach (var cDto in importData.Colors)
            {
                var existingColor = allColors.FirstOrDefault(c => c.ColorName.Equals(cDto.ColorName, StringComparison.OrdinalIgnoreCase));
                if (existingColor != null)
                {
                    existingColor.PantoneCode = cDto.PantoneCode;
                    _context.FabricColorPantones.Update(existingColor);
                }
                else
                {
                    var newColor = new FabricColorPantone
                    {
                        ColorName = cDto.ColorName,
                        PantoneCode = cDto.PantoneCode,
                        CompanyId = companyId,
                        BranchId = branchId
                    };
                    _context.FabricColorPantones.Add(newColor);
                    await _context.SaveChangesAsync();
                    allColors.Add(newColor);
                }
            }
            await _context.SaveChangesAsync();

            // 3. Process Orders
            foreach (var d in importData.Orders)
            {
                d.ProgramNumber = (d.ProgramNumber ?? "").Trim().ToUpper();
            }

            var programs = importData.Orders.GroupBy(d => d.ProgramNumber);
            int count = 0;

            var programNumbers = programs.Select(g => g.Key).Where(k => !string.IsNullOrEmpty(k)).ToList();
            var existingOrders = await _context.OrderSheets
                .Include(o => o.Items)
                    .ThenInclude(i => i.Colors)
                        .ThenInclude(c => c.SizeBreakdowns)
                .Where(o => o.CompanyId == companyId && programNumbers.Contains(o.ProgramNumber))
                .ToListAsync();

            foreach (var programGroup in programs)
            {
                var first = programGroup.First();
                var existingOrder = existingOrders.FirstOrDefault(o => o.ProgramNumber.Trim().Equals(programGroup.Key, StringComparison.OrdinalIgnoreCase));
                
                bool isNew = existingOrder == null;
                var orderSheet = existingOrder ?? new OrderSheet();

                orderSheet.ProgramNumber = (first.ProgramNumber ?? "").Trim().ToUpper();
                orderSheet.BuyerName = first.BuyerName;
                orderSheet.CustomerName = first.CustomerName;
                orderSheet.OrderDate = first.OrderDate;
                orderSheet.FabricDescription = first.FabricDescription;
                orderSheet.ProgramName = first.ProgramName;
                orderSheet.FactoryName = first.FactoryName;
                orderSheet.CompanyId = companyId;
                orderSheet.BranchId = branchId;

                if (!isNew)
                {
                    _context.OrderSheetItems.RemoveRange(orderSheet.Items);
                    orderSheet.Items.Clear();
                }
                else
                {
                    orderSheet.Items = new List<OrderSheetItem>();
                }

                var itemGroups = programGroup.GroupBy(d => new { d.OldArticleNo, d.NewArticleNo, d.ItemName });
                foreach (var itemGroup in itemGroups)
                {
                    var firstItem = itemGroup.First();
                    var packType = firstItem.PackType?.ToLower().Replace(" ", "") switch
                    {
                        "packa" => PackType.PackA,
                        "packb" => PackType.PackB,
                        _ => PackType.PackAB
                    };

                    // Link to newly created/updated Style
                    var style = allStyles.FirstOrDefault(s => s.StyleNumber.Equals(firstItem.NewArticleNo, StringComparison.OrdinalIgnoreCase));

                    var orderItem = new OrderSheetItem
                    {
                        OldArticleNo = firstItem.OldArticleNo,
                        NewArticleNo = firstItem.NewArticleNo,
                        ItemName = firstItem.ItemName,
                        PackType = packType,
                        StyleId = style?.Id,
                        Colors = new List<OrderSheetColor>()
                    };

                    var colorGroups = itemGroup.GroupBy(d => d.Color);
                    foreach (var colorGroup in colorGroups)
                    {
                        var colorMatch = allColors.FirstOrDefault(c => c.ColorName.Equals(colorGroup.Key, StringComparison.OrdinalIgnoreCase));

                        var orderColor = new OrderSheetColor
                        {
                            ColorName = colorGroup.Key,
                            ColorId = colorMatch?.Id,
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

                if (isNew) _context.OrderSheets.Add(orderSheet);
                count++;
            }

            await _context.SaveChangesAsync();
            return count;
        }

        public async Task<byte[]> DownloadTemplateAsync()
        {
            using var package = new OfficeOpenXml.ExcelPackage();

            // 1. Instructions Sheet
            var introWs = package.Workbook.Worksheets.Add("Instructions");
            introWs.Cells["A1"].Value = "Order Sheet Import Instructions";
            introWs.Cells["A1"].Style.Font.Size = 16;
            introWs.Cells["A1"].Style.Font.Bold = true;
            introWs.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(31, 73, 125));

            string[] instructions = {
                "1. Fill out the 'Styles' sheet first. Each 'NewArticleNo' in the 'Orders' sheet must have a corresponding entry in 'Styles'.",
                "2. Define any new colors in the 'Colors' sheet.",
                "3. Use the 'Orders' sheet to enter the actual order quantities and size breakdowns.",
                "4. 'PackType' values should be 'PackA', 'PackB', or 'PackAB'.",
                "5. Ensure 'ProgramNumber' is consistent for all rows belonging to the same manufacturing program.",
                "6. Do not rename the sheets; the importer specifically looks for 'Styles', 'Colors', and 'Orders'."
            };

            for (int i = 0; i < instructions.Length; i++)
            {
                introWs.Cells[i + 3, 1].Value = instructions[i];
                introWs.Cells[i + 3, 1].Style.Font.Size = 10;
            }
            introWs.Cells.AutoFitColumns();

            // 2. Styles Sheet
            var styleWs = package.Workbook.Worksheets.Add("Styles");
            string[] styleHeaders = { "StyleNumber", "BuyerName", "ProductType", "Season", "FabricType", "GSM", "SizeRange" };
            for (int i = 0; i < styleHeaders.Length; i++) {
                styleWs.Cells[1, i + 1].Value = styleHeaders[i];
                styleWs.Cells[1, i + 1].Style.Font.Size = 10;
            }
            var styleHeaderRange = styleWs.Cells[1, 1, 1, styleHeaders.Length];
            styleHeaderRange.Style.Font.Bold = true;
            styleHeaderRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            styleHeaderRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(22, 163, 74)); // Green
            styleHeaderRange.Style.Font.Color.SetColor(System.Drawing.Color.White);
            styleHeaderRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            
            // Demo Style Data
            styleWs.Cells[2, 1].Value = "N-101"; styleWs.Cells[2, 2].Value = "ANTONY SRL"; styleWs.Cells[2, 3].Value = "L/S POLO"; styleWs.Cells[2, 4].Value = "SUMMER 2025"; styleWs.Cells[2, 5].Value = "PIQUE JERSEY"; styleWs.Cells[2, 6].Value = "200"; styleWs.Cells[2, 7].Value = "S-XXXL";
            styleWs.Cells[3, 1].Value = "M-202"; styleWs.Cells[3, 2].Value = "ANTONY SRL"; styleWs.Cells[3, 3].Value = "V-NECK TEE"; styleWs.Cells[3, 4].Value = "WINTER 2025"; styleWs.Cells[3, 5].Value = "100% COTTON"; styleWs.Cells[3, 6].Value = "160"; styleWs.Cells[3, 7].Value = "M-4XL";

            // 3. Colors Sheet
            var colorWs = package.Workbook.Worksheets.Add("Colors");
            string[] colorHeaders = { "ColorName", "PantoneCode" };
            for (int i = 0; i < colorHeaders.Length; i++) {
                colorWs.Cells[1, i + 1].Value = colorHeaders[i];
            }
            var colorHeaderRange = colorWs.Cells[1, 1, 1, colorHeaders.Length];
            colorHeaderRange.Style.Font.Bold = true;
            colorHeaderRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            colorHeaderRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(234, 88, 12)); // Orange
            colorHeaderRange.Style.Font.Color.SetColor(System.Drawing.Color.White);
            colorHeaderRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            // Demo Colors
            colorWs.Cells[2, 1].Value = "NAVY"; colorWs.Cells[2, 2].Value = "19-4029 TCX";
            colorWs.Cells[3, 1].Value = "BLACK"; colorWs.Cells[3, 2].Value = "19-4008 TCX";
            colorWs.Cells[4, 1].Value = "WHITE"; colorWs.Cells[4, 2].Value = "11-0601 TCX";

            // 4. Orders Sheet
            var orderWs = package.Workbook.Worksheets.Add("Orders");
            string[] headers = {
                "ProgramNumber", "BuyerName", "CustomerName", "ProgramName", "OrderDate", "Factory", 
                "OldArticleNo", "NewArticleNo", "PackType", "ItemName", "FabricDescription",
                "Color", "Size_M", "Size_L", "Size_XL", "Size_XXL", "Size_XXXL", 
                "Size_3XL", "Size_4XL", "Size_5XL", "Size_6XL", "BuyerPackingNumber"
            };

            for (int i = 0; i < headers.Length; i++) {
                orderWs.Cells[1, i + 1].Value = headers[i];
            }
            var headerRange = orderWs.Cells[1, 1, 1, headers.Length];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(31, 73, 125)); // Blue
            headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White);
            headerRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            // Demo Order Data
            int row = 2;
            var demoItems = new[] { 
                new { Art = "N-101", Item = "L/S POLO", Fab = "100% COTTON 200GSM", Colors = new[] { "NAVY", "BLACK" } },
                new { Art = "M-202", Item = "V-NECK TEE", Fab = "JERSEY 160GSM", Colors = new[] { "WHITE" } }
            };

            foreach(var item in demoItems) {
                foreach(var c in item.Colors) {
                    orderWs.Cells[row, 1].Value = "PRG-2025-001";
                    orderWs.Cells[row, 2].Value = "ANTONY SRL";
                    orderWs.Cells[row, 3].Value = "RIFLE";
                    orderWs.Cells[row, 4].Value = "Summer Collection";
                    orderWs.Cells[row, 5].Value = DateTime.Now.ToString("yyyy-MM-dd");
                    orderWs.Cells[row, 6].Value = "Unit 01";
                    orderWs.Cells[row, 7].Value = "ART-" + item.Art;
                    orderWs.Cells[row, 8].Value = item.Art;
                    orderWs.Cells[row, 9].Value = "PackAB";
                    orderWs.Cells[row, 10].Value = item.Item;
                    orderWs.Cells[row, 11].Value = item.Fab;
                    orderWs.Cells[row, 12].Value = c;
                    orderWs.Cells[row, 13].Value = 120;
                    orderWs.Cells[row, 14].Value = 150;
                    orderWs.Cells[row, 15].Value = 150;
                    orderWs.Cells[row, 16].Value = 80;
                    orderWs.Cells[row, 22].Value = "PO-99" + row;
                    row++;
                }
            }

            styleWs.Cells.AutoFitColumns();
            colorWs.Cells.AutoFitColumns();
            orderWs.Cells.AutoFitColumns();
            
            // Move Instructions to the front
            package.Workbook.Worksheets.MoveToStart("Instructions");

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
            worksheet.View.ShowGridLines = false;

            // Global Styles
            var allCells = worksheet.Cells["A1:Z500"];
            allCells.Style.Font.Name = "Segoe UI";
            allCells.Style.Font.Size = 9;

            // Theme colors (match the order sheet preview: green header + orange totals)
            var primaryGreen = System.Drawing.Color.FromArgb(22, 163, 74);     // close to Tailwind green-600
            var primaryGreenDark = System.Drawing.Color.FromArgb(21, 128, 61); // close to green-700
            var softSlate = System.Drawing.Color.FromArgb(248, 250, 252);
            var borderSlate = System.Drawing.Color.FromArgb(226, 232, 240);
            var textMuted = System.Drawing.Color.FromArgb(100, 116, 139);
            var textStrong = System.Drawing.Color.FromArgb(15, 23, 42);
            var totalOrange = System.Drawing.Color.FromArgb(234, 88, 12);
            var totalOrangeText = System.Drawing.Color.FromArgb(194, 65, 12);

            // 1. Program Header Section (F8FAFC Background) - span full table width (A..T)
            var headerBox = worksheet.Cells["A1:T5"];
            headerBox.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerBox.Style.Fill.BackgroundColor.SetColor(softSlate);
            headerBox.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin, borderSlate);

            // Labels Row 2
            void SetHeaderLabel(int row, int col, string label) {
                worksheet.Cells[row, col].Value = label;
                worksheet.Cells[row, col].Style.Font.Size = 7.5f;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Font.Color.SetColor(textMuted);
            }
            void SetHeaderValue(int row, int col, string value) {
                worksheet.Cells[row, col].Value = value;
                worksheet.Cells[row, col].Style.Font.Size = 11;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Font.Color.SetColor(textStrong);
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

            // 2. Table Headers (Green like preview)
            int tableStartRow = 7;
            
            // Header Level 1
            var h1 = worksheet.Cells[tableStartRow, 1, tableStartRow, 20];
            h1.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            h1.Style.Fill.BackgroundColor.SetColor(primaryGreen);
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
            h2.Style.Fill.BackgroundColor.SetColor(primaryGreenDark);
            h2.Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(236, 253, 245)); // soft mint-white
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
                        worksheet.Cells[currentRow, 8].Style.Fill.BackgroundColor.SetColor(softSlate);

                        // Sizes
                        int[] vals = { sb.SizeM, sb.SizeL, sb.SizeXL, sb.SizeXXL, sb.SizeXXXL, sb.Size3XL, sb.Size4XL, sb.Size5XL, sb.Size6XL };
                        for (int v = 0; v < vals.Length; v++) {
                            worksheet.Cells[currentRow, 9 + v].Value = vals[v];
                            worksheet.Cells[currentRow, 9 + v].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        }

                        // Row Qty
                        worksheet.Cells[currentRow, 18].Value = sb.RowTotal;
                        worksheet.Cells[currentRow, 18].Style.Font.Bold = true;
                        worksheet.Cells[currentRow, 18].Style.Font.Color.SetColor(totalOrangeText);
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
                gTotalCell.Style.Fill.BackgroundColor.SetColor(primaryGreen);
                gTotalCell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                gTotalCell.Style.Font.Bold = true;
                gTotalCell.Style.Font.Size = 11;
                gTotalCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                gTotalCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                // Add left border to item first column
                worksheet.Cells[itemStartRow, 1, itemEndRow, 20].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin, borderSlate);
            }

            // 4. Summary Row
            var summaryRange = worksheet.Cells[currentRow, 1, currentRow, 20];
            summaryRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            summaryRange.Style.Fill.BackgroundColor.SetColor(softSlate);
            worksheet.Row(currentRow).Height = 30;

            worksheet.Cells[currentRow, 1, currentRow, 7].Merge = true;
            worksheet.Cells[currentRow, 1].Value = "SUMMARY AGGREGATION";
            worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 1].Style.Font.Color.SetColor(textMuted);
            worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            worksheet.Cells[currentRow, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            worksheet.Cells[currentRow, 8, currentRow, 18].Merge = true;
            worksheet.Cells[currentRow, 8].Value = "GRAND PROGRAM ACCUMULATION";
            worksheet.Cells[currentRow, 8].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 8].Style.Font.Color.SetColor(textMuted);
            worksheet.Cells[currentRow, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            worksheet.Cells[currentRow, 8].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // Grand Total (Bright Orange BG)
            var grandTotal = order.Items.Sum(i => i.TotalQty);
            var grandCell = worksheet.Cells[currentRow, 19];
            grandCell.Value = grandTotal.ToString("N0") + " PCS";
            grandCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            grandCell.Style.Fill.BackgroundColor.SetColor(totalOrange);
            grandCell.Style.Font.Color.SetColor(System.Drawing.Color.White);
            grandCell.Style.Font.Bold = true;
            grandCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            grandCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // Keep last column blank like preview footer area
            worksheet.Cells[currentRow, 20].Value = "";

            // 5. Signature Footer (3 columns like preview)
            currentRow += 3;
            int signatureLineRow = currentRow;
            int signatureTextRow = currentRow + 1;

            void CreateSignatureBlock(int fromCol, int toCol, string label)
            {
                worksheet.Cells[signatureLineRow, fromCol, signatureLineRow, toCol].Merge = true;
                worksheet.Cells[signatureTextRow, fromCol, signatureTextRow, toCol].Merge = true;

                var lineCell = worksheet.Cells[signatureLineRow, fromCol, signatureLineRow, toCol];
                lineCell.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Dashed;
                lineCell.Style.Border.Bottom.Color.SetColor(borderSlate);
                worksheet.Row(signatureLineRow).Height = 24;

                var textCell = worksheet.Cells[signatureTextRow, fromCol, signatureTextRow, toCol];
                textCell.Value = label;
                textCell.Style.Font.Size = 8;
                textCell.Style.Font.Bold = true;
                textCell.Style.Font.Color.SetColor(textMuted);
                textCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(signatureTextRow).Height = 16;
            }

            CreateSignatureBlock(2, 6, "MERCHANDISER SIGNATURE");
            CreateSignatureBlock(8, 13, "PRODUCTION MANAGER");
            CreateSignatureBlock(15, 19, "AUTHORIZED APPROVAL");

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

            // Border grid for the full table area
            var tableEndRow = signatureTextRow;
            var dataArea = worksheet.Cells[tableStartRow, 1, tableEndRow, 20];
            dataArea.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            dataArea.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            dataArea.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            dataArea.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            dataArea.Style.Border.Top.Color.SetColor(borderSlate);
            dataArea.Style.Border.Left.Color.SetColor(borderSlate);
            dataArea.Style.Border.Right.Color.SetColor(borderSlate);
            dataArea.Style.Border.Bottom.Color.SetColor(borderSlate);

            return await Task.FromResult(package.GetAsByteArray());
        }
    }
}
