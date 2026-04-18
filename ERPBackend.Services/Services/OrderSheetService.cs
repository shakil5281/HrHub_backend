using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Enums;
using ERPBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;

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
                .Include(o => o.Buttons)
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
                                SizeBreakdowns = c.SizeBreakdowns.Select((sb, sbIndex) => {
                                    var btnList = o.Buttons.Where(b => 
                                        (b.ArticleNo?.Trim().Equals(i.NewArticleNo?.Trim(), StringComparison.OrdinalIgnoreCase) ?? false) && 
                                        (b.GarmentColor?.Trim().Equals(c.ColorName?.Trim(), StringComparison.OrdinalIgnoreCase) ?? false)
                                    ).ToList();
                                    var btn = sbIndex < btnList.Count ? btnList[sbIndex] : btnList.FirstOrDefault();
                                    return new ProgramSizeBreakdownDto
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
                                        ButtonColor = btn?.ButtonColor,
                                        ButtonColorId = btn?.ButtonColorId,
                                        ButtonQty = btn?.RequiredQuantity,
                                        ButtonType = btn?.ButtonType,
                                        ButtonSize = btn?.ButtonSize,
                                        Unit = btn?.Unit,
                                        Status = btn?.Status
                                    };
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

            // Update top level metadata
            existingOrder.CompanyId = programOrder.CompanyId;
            existingOrder.BranchId = programOrder.BranchId;
            existingOrder.ProgramNumber = programOrder.ProgramNumber;
            existingOrder.BuyerName = programOrder.BuyerName;
            existingOrder.BuyerId = programOrder.BuyerId;
            existingOrder.CustomerName = programOrder.CustomerName;
            existingOrder.FabricDescription = programOrder.FabricDescription;
            existingOrder.ProgramName = programOrder.ProgramName;
            existingOrder.OrderDate = programOrder.OrderDate;
            existingOrder.FactoryName = programOrder.FactoryName;
            existingOrder.FactoryAddress = programOrder.FactoryAddress;

            // Robust Healing/Sync Logic
            var incomingArticles = programOrder.Articles ?? new List<ProgramArticle>();
            var existingArticles = existingOrder.Articles.ToList();

            foreach (var incomingArt in incomingArticles)
            {
                // Match by NewArticleNo + ItemName (Natural Key)
                var existingArt = existingArticles.FirstOrDefault(a => 
                    a.NewArticleNo == incomingArt.NewArticleNo && 
                    a.ItemName == incomingArt.ItemName);

                if (existingArt == null)
                {
                    // New Article - Add it
                    incomingArt.Id = 0;
                    incomingArt.ProgramOrderId = existingOrder.Id;
                    
                    // Reset all child IDs to ensure clean insertion
                    foreach (var col in incomingArt.Colors ?? new List<ProgramColor>())
                    {
                        col.Id = 0;
                        foreach (var sb in col.SizeBreakdowns ?? new List<ProgramSizeBreakdown>())
                        {
                            sb.Id = 0;
                        }
                    }

                    _calculateArticleTotal(incomingArt);
                    existingOrder.Articles.Add(incomingArt);
                }
                else
                {
                    // Existing Article - Sync it
                    existingArt.OldArticleNo = incomingArt.OldArticleNo;
                    existingArt.StyleId = incomingArt.StyleId;
                    existingArt.PackType = incomingArt.PackType;

                    var incomingColors = incomingArt.Colors ?? new List<ProgramColor>();
                    var existingColors = existingArt.Colors.ToList();

                    foreach (var incomingCol in incomingColors)
                    {
                        var existingCol = existingColors.FirstOrDefault(c => c.ColorName == incomingCol.ColorName);

                        if (existingCol == null)
                        {
                            // New Color
                            incomingCol.Id = 0;
                            incomingCol.ProgramArticleId = existingArt.Id;
                            
                            // Reset child IDs
                            foreach (var sb in incomingCol.SizeBreakdowns ?? new List<ProgramSizeBreakdown>())
                            {
                                sb.Id = 0;
                            }

                            existingArt.Colors.Add(incomingCol);
                        }
                        else
                        {
                            // Existing Color - Sync Size Breakdowns
                            existingCol.ColorId = incomingCol.ColorId;

                            var incomingSbs = incomingCol.SizeBreakdowns ?? new List<ProgramSizeBreakdown>();
                            var existingSbs = existingCol.SizeBreakdowns.ToList();

                            foreach (var incomingSb in incomingSbs)
                            {
                                // Match by BuyerPackingNumber (Natural Key)
                                var existingSb = existingSbs.FirstOrDefault(s => s.BuyerPackingNumber == incomingSb.BuyerPackingNumber);

                                if (existingSb == null)
                                {
                                    // New Size Breakdown
                                    incomingSb.Id = 0;
                                    incomingSb.ProgramColorId = existingCol.Id;
                                    _calculateSbTotal(incomingSb);
                                    existingCol.SizeBreakdowns.Add(incomingSb);
                                }
                                else
                                {
                                    // Existing Size Breakdown - PRESERVE ID to keep relations!
                                    existingSb.SizeM = incomingSb.SizeM;
                                    existingSb.SizeL = incomingSb.SizeL;
                                    existingSb.SizeXL = incomingSb.SizeXL;
                                    existingSb.SizeXXL = incomingSb.SizeXXL;
                                    existingSb.SizeXXXL = incomingSb.SizeXXXL;
                                    existingSb.Size3XL = incomingSb.Size3XL;
                                    existingSb.Size4XL = incomingSb.Size4XL;
                                    existingSb.Size5XL = incomingSb.Size5XL;
                                    existingSb.Size6XL = incomingSb.Size6XL;
                                    existingSb.BuyerPackingNumber = incomingSb.BuyerPackingNumber;
                                    _calculateSbTotal(existingSb);

                                    // IMPORTANT: Do NOT overwrite ButtonColor/ButtonQty if incoming doesn't have them
                                    // Normally we should update them if the matrix was changed during this update
                                    if (incomingSb.ButtonColorId.HasValue || !string.IsNullOrEmpty(incomingSb.ButtonColor))
                                    {
                                        _syncButtonBooking(existingOrder.Id, existingArt, existingCol, incomingSb);
                                    }
                                }
                            }

                            // Optional: Remove size breakdowns no longer in incoming
                            var incomingPackNos = incomingSbs.Select(s => s.BuyerPackingNumber).ToList();
                            var toRemoveSbs = existingSbs.Where(s => !incomingPackNos.Contains(s.BuyerPackingNumber)).ToList();
                            foreach (var r in toRemoveSbs) _context.ProgramSizeBreakdowns.Remove(r);
                        }
                    }

                    // Optional: Remove colors no longer in incoming
                    var incomingColorNames = incomingColors.Select(c => c.ColorName).ToList();
                    var toRemoveCols = existingColors.Where(c => !incomingColorNames.Contains(c.ColorName)).ToList();
                    foreach (var r in toRemoveCols) _context.ProgramColors.Remove(r);

                    _calculateArticleTotal(existingArt);
                }
            }

            // Optional: Remove articles no longer in incoming
            var incomingArtNos = incomingArticles.Select(a => a.NewArticleNo + a.ItemName).ToList();
            var toRemoveArts = existingArticles.Where(a => !incomingArtNos.Contains(a.NewArticleNo + a.ItemName)).ToList();
            foreach (var r in toRemoveArts) _context.ProgramArticles.Remove(r);

            // PRE-RECONCILE: Remove bookings for articles/colors/breakdowns that are about to be removed
            // This prevents foreign key Restrict conflicts during the main save
            await _reconcileAccessoriesAsync(existingOrder.Id, programOrder);

            await _context.SaveChangesAsync();
        }

        private void _calculateSbTotal(ProgramSizeBreakdown sb)
        {
            sb.RowTotal = sb.SizeM + sb.SizeL + sb.SizeXL + sb.SizeXXL + sb.SizeXXXL + sb.Size3XL + sb.Size4XL + sb.Size5XL + sb.Size6XL;
        }

        private void _calculateArticleTotal(ProgramArticle article)
        {
            article.TotalQty = article.Colors.SelectMany(c => c.SizeBreakdowns).Sum(sb => sb.RowTotal);
        }

        private async Task _reconcileAccessoriesAsync(int programOrderId, ProgramOrder incomingOrder)
        {
            // Valid combinations of (ArticleNo + ColorName) from the incoming payload
            var validCombos = incomingOrder.Articles
                .SelectMany(a => a.Colors.Select(c => new { 
                    ArtNo = (a.NewArticleNo ?? "").Trim().ToLower(), 
                    Color = (c.ColorName ?? "").Trim().ToLower() 
                }))
                .Where(x => !string.IsNullOrEmpty(x.ArtNo))
                .ToList();

            // Valid IDs for SizeBreakdowns being preserved
            var validSbIds = incomingOrder.Articles
                .SelectMany(a => a.Colors)
                .SelectMany(c => c.SizeBreakdowns)
                .Where(sb => sb.Id > 0)
                .Select(sb => sb.Id)
                .ToList();

            // 1. Reconcile ButtonBookings
            var buttons = await _context.ButtonBookings.Where(b => b.ProgramOrderId == programOrderId).ToListAsync();
            var staleButtons = buttons.Where(b => 
                !validCombos.Any(v => v.ArtNo == (b.ArticleNo ?? "").Trim().ToLower() && v.Color == (b.GarmentColor ?? "").Trim().ToLower()) ||
                (b.ProgramSizeBreakdownId.HasValue && !validSbIds.Contains(b.ProgramSizeBreakdownId.Value))
            ).ToList();
            if (staleButtons.Any()) _context.ButtonBookings.RemoveRange(staleButtons);

            // 2. Reconcile ZipperBookings
            var zippers = await _context.ZipperBookings.Where(z => z.ProgramOrderId == programOrderId).ToListAsync();
            var staleZippers = zippers.Where(z => 
                !validCombos.Any(v => v.ArtNo == (z.ArticleNo ?? "").Trim().ToLower() && v.Color == (z.GarmentColor ?? "").Trim().ToLower()) ||
                (z.ProgramSizeBreakdownId.HasValue && !validSbIds.Contains(z.ProgramSizeBreakdownId.Value))
            ).ToList();
            if (staleZippers.Any()) _context.ZipperBookings.RemoveRange(staleZippers);

            // 3. Reconcile MainLabelBookings
            var mainLabels = await _context.MainLabelBookings.Where(l => l.ProgramOrderId == programOrderId).ToListAsync();
            var staleMainLabels = mainLabels.Where(l => 
                !validCombos.Any(v => v.ArtNo == (l.ArticleNo ?? "").Trim().ToLower() && v.Color == (l.GarmentColor ?? "").Trim().ToLower()) ||
                (l.ProgramSizeBreakdownId.HasValue && !validSbIds.Contains(l.ProgramSizeBreakdownId.Value))
            ).ToList();
            if (staleMainLabels.Any()) _context.MainLabelBookings.RemoveRange(staleMainLabels);

            // 4. Reconcile CareLabelBookings
            var careLabels = await _context.CareLabelBookings.Where(l => l.ProgramOrderId == programOrderId).ToListAsync();
            var staleCareLabels = careLabels.Where(l => 
                !validCombos.Any(v => v.ArtNo == (l.ArticleNo ?? "").Trim().ToLower() && v.Color == (l.GarmentColor ?? "").Trim().ToLower()) ||
                (l.ProgramSizeBreakdownId.HasValue && !validSbIds.Contains(l.ProgramSizeBreakdownId.Value))
            ).ToList();
            if (staleCareLabels.Any()) _context.CareLabelBookings.RemoveRange(staleCareLabels);

            // 5. Reconcile PolyBookings
            var polys = await _context.PolyBookings.Where(p => p.ProgramOrderId == programOrderId).ToListAsync();
            var stalePolys = polys.Where(p => 
                !validCombos.Any(v => v.ArtNo == (p.ArticleNo ?? "").Trim().ToLower() && v.Color == (p.GarmentColor ?? "").Trim().ToLower()) ||
                (p.ProgramSizeBreakdownId.HasValue && !validSbIds.Contains(p.ProgramSizeBreakdownId.Value))
            ).ToList();
            if (stalePolys.Any()) _context.PolyBookings.RemoveRange(stalePolys);

            // 6. Reconcile ThreadBookings
            var threads = await _context.ThreadBookings.Where(t => t.ProgramOrderId == programOrderId).ToListAsync();
            var staleThreads = threads.Where(t => 
                !validCombos.Any(v => v.ArtNo == (t.ArticleNo ?? "").Trim().ToLower() && v.Color == (t.GarmentColor ?? "").Trim().ToLower()) ||
                (t.ProgramSizeBreakdownId.HasValue && !validSbIds.Contains(t.ProgramSizeBreakdownId.Value))
            ).ToList();
            if (staleThreads.Any()) _context.ThreadBookings.RemoveRange(staleThreads);

            // 7. SnapButtonBookings
            var snapButtons = await _context.SnapButtonBookings.Where(s => s.ProgramOrderId == programOrderId).ToListAsync();
            var staleSnaps = snapButtons.Where(s => 
                !validCombos.Any(v => v.ArtNo == (s.ArticleNo ?? "").Trim().ToLower() && v.Color == (s.GarmentColor ?? "").Trim().ToLower()) ||
                (s.ProgramSizeBreakdownId.HasValue && !validSbIds.Contains(s.ProgramSizeBreakdownId.Value))
            ).ToList();
            if (staleSnaps.Any()) _context.SnapButtonBookings.RemoveRange(staleSnaps);

            await _context.SaveChangesAsync();
        }

        private void _syncButtonBooking(int orderId, ProgramArticle art, ProgramColor col, ProgramSizeBreakdown sb)
        {
            // Match by proper SizeBreakdown relation first, fallback to legacy matching for old data
            var existing = _context.ButtonBookings.FirstOrDefault(b => 
                (sb.Id > 0 && b.ProgramSizeBreakdownId == sb.Id) || 
                (b.ProgramSizeBreakdownId == null && b.ProgramOrderId == orderId && b.ArticleNo == art.NewArticleNo && b.GarmentColor == col.ColorName));

            if (existing != null)
            {
                existing.ArticleNo = art.NewArticleNo;
                existing.ItemName = art.ItemName;
                existing.GarmentColor = col.ColorName;
                existing.ButtonColorId = sb.ButtonColorId;
                existing.ButtonColor = sb.ButtonColor ?? string.Empty;
                existing.RequiredQuantity = sb.ButtonQty ?? 0;
                existing.ButtonType = sb.ButtonType ?? "PLASTIC";
                existing.ButtonSize = sb.ButtonSize ?? "24L";
                existing.Unit = sb.Unit ?? "Pcs";
                existing.Status = sb.Status ?? "Planned";
                
                if (sb.Id > 0) existing.ProgramSizeBreakdownId = sb.Id;
                else existing.ProgramSizeBreakdown = sb;
            }
            else
            {
                var buttonBooking = new ButtonBooking
                {
                    ProgramOrderId = orderId,
                    ItemName = art.ItemName,
                    ArticleNo = art.NewArticleNo,
                    GarmentColor = col.ColorName,
                    ButtonColorId = sb.ButtonColorId,
                    ButtonColor = sb.ButtonColor ?? string.Empty,
                    RequiredQuantity = sb.ButtonQty ?? 0,
                    ButtonType = sb.ButtonType ?? "PLASTIC",
                    ButtonSize = sb.ButtonSize ?? "24L",
                    Unit = sb.Unit ?? "Pcs",
                    Status = sb.Status ?? "Planned",
                    DeliveryDate = DateTime.UtcNow.AddDays(15)
                };
                
                if (sb.Id > 0) buttonBooking.ProgramSizeBreakdownId = sb.Id;
                else buttonBooking.ProgramSizeBreakdown = sb;
                
                _context.ButtonBookings.Add(buttonBooking);
            }
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
                    .ThenInclude(a => a.Colors)
                        .ThenInclude(c => c.SizeBreakdowns)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var summary = new GlobalProgramSummaryDto
            {
                TotalPrograms = programs.Count,
                TotalPieces = programs.Sum(p => p.Articles.Sum(a => a.TotalQty)),
                TotalBuyers = programs.Select(p => p.BuyerName).Distinct().Count()
            };

            // Calculate Buyer Distribution
            var buyerGroup = programs.GroupBy(p => p.BuyerName)
                .Select(g => new
                {
                    BuyerName = string.IsNullOrEmpty(g.Key) ? "Unknown" : g.Key,
                    TotalQty = g.Sum(p => p.Articles.Sum(a => a.TotalQty))
                })
                .OrderByDescending(g => g.TotalQty)
                .ToList();

            if (summary.TotalPieces > 0)
            {
                summary.BuyerDistribution = buyerGroup.Select(b => new BuyerDistributionDto
                {
                    BuyerName = b.BuyerName,
                    TotalQty = b.TotalQty,
                    Percentage = Math.Round((double)b.TotalQty / summary.TotalPieces * 100, 1)
                }).ToList();
            }

            // Calculate Size Distribution
            var allSizeBreakdowns = programs.SelectMany(p => p.Articles).SelectMany(a => a.Colors).SelectMany(c => c.SizeBreakdowns).ToList();
            
            summary.SizeDistribution = new List<SizeDistributionDto>
            {
                new SizeDistributionDto { SizeName = "M", TotalQty = allSizeBreakdowns.Sum(s => s.SizeM) },
                new SizeDistributionDto { SizeName = "L", TotalQty = allSizeBreakdowns.Sum(s => s.SizeL) },
                new SizeDistributionDto { SizeName = "XL", TotalQty = allSizeBreakdowns.Sum(s => s.SizeXL) },
                new SizeDistributionDto { SizeName = "XXL", TotalQty = allSizeBreakdowns.Sum(s => s.SizeXXL) },
                new SizeDistributionDto { SizeName = "XXXL", TotalQty = allSizeBreakdowns.Sum(s => s.SizeXXXL) },
                new SizeDistributionDto { SizeName = "3XL", TotalQty = allSizeBreakdowns.Sum(s => s.Size3XL) },
                new SizeDistributionDto { SizeName = "4XL", TotalQty = allSizeBreakdowns.Sum(s => s.Size4XL) },
                new SizeDistributionDto { SizeName = "5XL", TotalQty = allSizeBreakdowns.Sum(s => s.Size5XL) },
                new SizeDistributionDto { SizeName = "6XL", TotalQty = allSizeBreakdowns.Sum(s => s.Size6XL) }
            }.Where(s => s.TotalQty > 0).ToList();

            // Setup Recent Programs (top 5) mapped to DTO
            summary.RecentPrograms = programs.Take(5).Select(o => new ProgramOrderDto
            {
                Id = o.Id,
                ProgramNumber = o.ProgramNumber,
                BuyerName = o.BuyerName,
                OrderDate = o.OrderDate,
                TotalQty = o.Articles.Sum(a => a.TotalQty),
                Articles = o.Articles.Select(a => new ProgramArticleDto
                {
                     TotalQty = a.TotalQty
                }).ToList()
            }).ToList();

            return summary;
        }

        // Placeholder methods for Excel to avoid massive code dump of old logic
        public async Task<byte[]> DownloadTemplateAsync()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            
            // 1. BUYERS SHEET
            var buyerWs = package.Workbook.Worksheets.Add("Buyers");
            string[] buyerHeaders = { "Buyer Name", "Country", "Contact Person", "Email", "Currency" };
            for (int i = 0; i < buyerHeaders.Length; i++) {
                buyerWs.Cells[1, i + 1].Value = buyerHeaders[i];
                buyerWs.Cells[1, i + 1].Style.Font.Bold = true;
            }
            buyerWs.Cells[2, 1].Value = "GLOBAL BUYER LTD"; buyerWs.Cells[2, 2].Value = "USA"; buyerWs.Cells[2, 3].Value = "John Doe"; buyerWs.Cells[2, 4].Value = "john@global.com";
            buyerWs.Cells[3, 1].Value = "FAST FASHION CO"; buyerWs.Cells[3, 2].Value = "UK"; buyerWs.Cells[3, 3].Value = "Jane Smith"; buyerWs.Cells[3, 4].Value = "jane@fast.com";
            buyerWs.Cells.AutoFitColumns();

            // 2. STYLES SHEET
            var styleWs = package.Workbook.Worksheets.Add("Styles");
            string[] styleHeaders = { "Style Number", "Product Type", "Linked Buyer" };
            for (int i = 0; i < styleHeaders.Length; i++) {
                styleWs.Cells[1, i + 1].Value = styleHeaders[i];
                styleWs.Cells[1, i + 1].Style.Font.Bold = true;
            }
            styleWs.Cells[2, 1].Value = "STYLE-X1"; styleWs.Cells[2, 2].Value = "MENS CREW T-SHIRT"; styleWs.Cells[2, 3].Value = "GLOBAL BUYER LTD";
            styleWs.Cells[3, 1].Value = "STYLE-V2"; styleWs.Cells[3, 2].Value = "MENS V-NECK T-SHIRT"; styleWs.Cells[3, 3].Value = "GLOBAL BUYER LTD";
            styleWs.Cells[4, 1].Value = "SH-99"; styleWs.Cells[4, 2].Value = "WOMENS OVERSIZED HOODIE"; styleWs.Cells[4, 3].Value = "FAST FASHION CO";
            styleWs.Cells.AutoFitColumns();

            // 3. COLORS SHEET
            var colorWs = package.Workbook.Worksheets.Add("Colors");
            string[] colorHeaders = { "Color Name", "Pantone Code" };
            for (int i = 0; i < colorHeaders.Length; i++) {
                colorWs.Cells[1, i + 1].Value = colorHeaders[i];
                colorWs.Cells[1, i + 1].Style.Font.Bold = true;
            }
            colorWs.Cells[2, 1].Value = "NAVY"; colorWs.Cells[2, 2].Value = "19-4011 TCX";
            colorWs.Cells[3, 1].Value = "BLACK"; colorWs.Cells[3, 2].Value = "19-4008 TCX";
            colorWs.Cells[4, 1].Value = "WHITE"; colorWs.Cells[4, 2].Value = "11-0601 TCX";
            colorWs.Cells[5, 1].Value = "HEATHER GREY"; colorWs.Cells[5, 2].Value = "16-3802 TCX";
            colorWs.Cells.AutoFitColumns();

            // 4. MAIN ORDER SHEET
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

            // Populating demo data that relates to the master sheets above
            // Program 1
            worksheet.Cells[2, 1].Value = "DEMO-PRG-001";
            worksheet.Cells[2, 2].Value = "GLOBAL BUYER LTD";
            worksheet.Cells[2, 3].Value = "RETAIL CORP";
            worksheet.Cells[2, 4].Value = "MENS CREW T-SHIRT";
            worksheet.Cells[2, 5].Value = "ART-2024-X1";
            worksheet.Cells[2, 6].Value = "STYLE-X1";
            worksheet.Cells[2, 7].Value = "SUMMER 2024";
            worksheet.Cells[2, 8].Value = "100% COTTON JERSEY 160GSM";
            worksheet.Cells[2, 9].Value = "NAVY";
            worksheet.Cells[2, 10].Value = 500; worksheet.Cells[2, 11].Value = 1000; worksheet.Cells[2, 12].Value = 800; worksheet.Cells[2, 19].Value = "BOX-01";

            worksheet.Cells[3, 1].Value = "DEMO-PRG-001";
            worksheet.Cells[3, 2].Value = "GLOBAL BUYER LTD";
            worksheet.Cells[3, 3].Value = "RETAIL CORP";
            worksheet.Cells[3, 4].Value = "MENS CREW T-SHIRT";
            worksheet.Cells[3, 5].Value = "ART-2024-X1";
            worksheet.Cells[3, 6].Value = "STYLE-X1";
            worksheet.Cells[3, 7].Value = "SUMMER 2024";
            worksheet.Cells[3, 8].Value = "100% COTTON JERSEY 160GSM";
            worksheet.Cells[3, 9].Value = "BLACK";
            worksheet.Cells[3, 10].Value = 300; worksheet.Cells[3, 11].Value = 600; worksheet.Cells[3, 12].Value = 400; worksheet.Cells[3, 19].Value = "BOX-02";

            // Program 2
            worksheet.Cells[4, 1].Value = "DEMO-PRG-002";
            worksheet.Cells[4, 2].Value = "FAST FASHION CO";
            worksheet.Cells[4, 3].Value = "URBAN STYLE";
            worksheet.Cells[4, 4].Value = "WOMENS OVERSIZED HOODIE";
            worksheet.Cells[4, 5].Value = "HOOD-99";
            worksheet.Cells[4, 6].Value = "SH-99";
            worksheet.Cells[4, 7].Value = "WINTER 2024";
            worksheet.Cells[4, 8].Value = "BRUSHED FLEECE 320GSM";
            worksheet.Cells[4, 9].Value = "HEATHER GREY";
            worksheet.Cells[4, 10].Value = 100; worksheet.Cells[4, 11].Value = 200; worksheet.Cells[4, 19].Value = "CTN-22";

            worksheet.Cells.AutoFitColumns();
            worksheet.View.ShowGridLines = true;
            
            // Select the main sheet so it opens first
            worksheet.Select();

            return await package.GetAsByteArrayAsync();
        }

        public async Task<MultiSheetOrderImportDto> ParseExcelAsync(Stream fileStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(fileStream);
            // Specifically target the transactional sheet
            var worksheet = package.Workbook.Worksheets["Garment Order Sheet"] ?? package.Workbook.Worksheets[0];
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
                var programNumber = group.Key;
                
                // Check if program already exists
                var existingProgram = await _context.ProgramOrders
                    .Include(o => o.Articles)
                        .ThenInclude(i => i.Colors)
                            .ThenInclude(c => c.SizeBreakdowns)
                    .FirstOrDefaultAsync(o => o.ProgramNumber == programNumber && o.CompanyId == cid);

                // Group by Article
                var articleList = new List<ProgramArticle>();
                var articles = group.GroupBy(o => (o.NewArticleNo ?? "Unknown Art").Trim());
                
                foreach (var artGrp in articles)
                {
                    var firstArt = artGrp.First();
                    var artNo = artGrp.Key;
                    
                    // Buyer lookup/create
                    var buyerName = first.BuyerName?.Trim() ?? "UNKNOWN";
                    var buyerId = buyersInDb.FirstOrDefault(b => b.Name.Trim().Equals(buyerName, StringComparison.OrdinalIgnoreCase))?.Id;

                    if (buyerId == null)
                    {
                        var newBuyer = new Buyer
                        {
                            Name = buyerName,
                            CompanyId = cid,
                            BranchId = bid,
                            ContactPerson = "Auto-Created",
                            Email = "info@buyer.com",
                            Country = "Unknown",
                            PaymentTerms = "N/A",
                            Currency = "USD"
                        };
                        _context.Buyers.Add(newBuyer);
                        await _context.SaveChangesAsync();
                        buyersInDb.Add(newBuyer);
                        buyerId = newBuyer.Id;
                    }

                    // Style lookup/create
                    var styleId = stylesInDb.FirstOrDefault(s => 
                        s.StyleNumber.Trim().Equals(artNo, StringComparison.OrdinalIgnoreCase) || 
                        s.StyleNumber.Trim().Equals(firstArt.StyleNo?.Trim(), StringComparison.OrdinalIgnoreCase))?.Id;

                    if (styleId == null)
                    {
                        var newStyle = new Style {
                            StyleNumber = artNo,
                            ProductType = firstArt.ItemName ?? "Unknown Product",
                            BuyerId = buyerId.Value,
                            CompanyId = cid,
                            BranchId = bid
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
                        TotalQty = 0
                    };

                    foreach (var row in artGrp)
                    {
                        var colorName = row.Color?.Trim() ?? "N/A";
                        var colorId = colorsInDb.FirstOrDefault(c => c.ColorName.Trim().Equals(colorName, StringComparison.OrdinalIgnoreCase))?.Id;
                        
                        var color = new ProgramColor { ColorName = colorName, ColorId = colorId };
                        var breakdown = new ProgramSizeBreakdown {
                            SizeM = row.SizeM, SizeL = row.SizeL, SizeXL = row.SizeXL,
                            SizeXXL = row.SizeXXL, SizeXXXL = row.SizeXXXL, Size3XL = row.Size3XL,
                            Size4XL = row.Size4XL, Size5XL = row.Size5XL, Size6XL = row.Size6XL,
                            BuyerPackingNumber = row.PackRef ?? ""
                        };
                        breakdown.RowTotal = breakdown.SizeM + breakdown.SizeL + breakdown.SizeXL + breakdown.SizeXXL + breakdown.SizeXXXL + breakdown.Size3XL + breakdown.Size4XL + breakdown.Size5XL + breakdown.Size6XL;
                        color.SizeBreakdowns.Add(breakdown);
                        article.Colors.Add(color);
                        article.TotalQty += breakdown.RowTotal;
                    }
                    articleList.Add(article);
                }

                if (existingProgram != null)
                {
                    // UPDATE existing
                    existingProgram.Articles = articleList;
                    await UpdateAsync(existingProgram);
                }
                else
                {
                    // CREATE new
                    var program = new ProgramOrder {
                        CompanyId = cid, BranchId = bid, ProgramNumber = programNumber,
                        BuyerName = first.BuyerName?.Trim() ?? "UNKNOWN",
                        CustomerName = first.CustomerName ?? "",
                        ProgramName = first.ProgramName ?? "",
                        FabricDescription = first.Fabric ?? "", OrderDate = DateTime.UtcNow,
                        Articles = articleList
                    };
                    await CreateAsync(program);
                }
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
            var ws = package.Workbook.Worksheets.Add("Production Analysis");

            // Define Colors
            var greenHeaderBg = Color.FromArgb(22, 163, 74); // #16a34a
            var greySubHeaderBg = Color.FromArgb(243, 244, 246);
            var orangeTotalBg = Color.FromArgb(254, 242, 242); // very light red/orange for Row Qty
            var darkOrangeGrandTotalBg = Color.FromArgb(234, 88, 12); // #ea580c
            var borderLineColor = Color.FromArgb(209, 213, 219);

            // 1. TOP METADATA SECTION
            // Program Info Grid
            ws.Cells["A1:B1"].Merge = true;
            ws.Cells["A1"].Value = "PROGRAM #";
            ws.Cells["A1"].Style.Font.Size = 9;
            ws.Cells["A1"].Style.Font.Bold = true;
            ws.Cells["A1"].Style.Font.Color.SetColor(Color.Gray);

            ws.Cells["A2:B2"].Merge = true;
            ws.Cells["A2"].Value = program.ProgramNumber;
            ws.Cells["A2"].Style.Font.Size = 12;
            ws.Cells["A2"].Style.Font.Bold = true;

            ws.Cells["C1:D1"].Merge = true;
            ws.Cells["C1"].Value = "BUYER ENTITY";
            ws.Cells["C1"].Style.Font.Size = 9;
            ws.Cells["C1"].Style.Font.Bold = true;
            ws.Cells["C1"].Style.Font.Color.SetColor(Color.Gray);

            ws.Cells["C2:D2"].Merge = true;
            ws.Cells["C2"].Value = program.BuyerName;
            ws.Cells["C2"].Style.Font.Size = 12;
            ws.Cells["C2"].Style.Font.Bold = true;

            ws.Cells["E1:F1"].Merge = true;
            ws.Cells["E1"].Value = "CUSTOMER";
            ws.Cells["E1"].Style.Font.Size = 9;
            ws.Cells["E1"].Style.Font.Bold = true;
            ws.Cells["E1"].Style.Font.Color.SetColor(Color.Gray);

            ws.Cells["E2:F2"].Merge = true;
            ws.Cells["E2"].Value = program.CustomerName ?? "WHITE";
            ws.Cells["E2"].Style.Font.Size = 12;
            ws.Cells["E2"].Style.Font.Bold = true;

            ws.Cells["G1:H1"].Merge = true;
            ws.Cells["G1"].Value = "ORDER DATE";
            ws.Cells["G1"].Style.Font.Size = 9;
            ws.Cells["G1"].Style.Font.Bold = true;
            ws.Cells["G1"].Style.Font.Color.SetColor(Color.Gray);

            ws.Cells["G2:H2"].Merge = true;
            ws.Cells["G2"].Value = program.OrderDate.ToString("dd MMM yyyy");
            ws.Cells["G2"].Style.Font.Size = 12;
            ws.Cells["G2"].Style.Font.Bold = true;

            // Fabric Composition and Program Cycle
            ws.Cells["A4:B4"].Merge = true;
            ws.Cells["A4"].Value = "FABRIC COMPOSITION";
            ws.Cells["A4"].Style.Font.Size = 8;
            ws.Cells["A4"].Style.Font.Bold = true;
            ws.Cells["A4"].Style.Font.Color.SetColor(Color.Gray);
            ws.Cells["A5:D5"].Merge = true;
            ws.Cells["A5"].Value = program.FabricDescription ?? "0";
            ws.Cells["A5"].Style.Font.Size = 10;

            ws.Cells["E4:F4"].Merge = true;
            ws.Cells["E4"].Value = "PROGRAM CYCLE";
            ws.Cells["E4"].Style.Font.Size = 8;
            ws.Cells["E4"].Style.Font.Bold = true;
            ws.Cells["E4"].Style.Font.Color.SetColor(Color.Gray);
            ws.Cells["E5:H5"].Merge = true;
            ws.Cells["E5"].Value = program.ProgramName ?? "0";
            ws.Cells["E5"].Style.Font.Size = 10;

            // 2. MAIN TABLE HEADERS (Starting from Row 8)
            int tableStartRow = 8;
            ws.Row(tableStartRow).Height = 30;
            ws.Row(tableStartRow + 1).Height = 30;
            string[] mainHeaders = { "SL", "OLD ART", "NEW ART", "LABEL", "PACK", "ITEM", "TOTAL QTY", "COLOR", "SIZE BREAKDOWN MATRIX", "", "", "", "", "", "", "", "", "ROW QTY", "G.TOTAL", "BUYER ORDER#" };
            
            // Header Row 1
            for (int i = 0; i < mainHeaders.Length; i++)
            {
                var cell = ws.Cells[tableStartRow, i + 1];
                cell.Value = mainHeaders[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.Color.SetColor(Color.White);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(greenHeaderBg);
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.White);

                if (i < 8 || i > 16) // SL to Color and Row Qty to Buyer Order#
                {
                    ws.Cells[tableStartRow, i + 1, tableStartRow + 1, i + 1].Merge = true;
                }
            }
            
            // Merge Matrix Header
            ws.Cells[tableStartRow, 9, tableStartRow, 17].Merge = true;

            // Header Row 2 (Sizes)
            string[] sizes = { "M", "L", "XL", "XXL", "XXXL", "3XL", "4XL", "5XL", "6XL" };
            for (int i = 0; i < sizes.Length; i++)
            {
                var cell = ws.Cells[tableStartRow + 1, 9 + i];
                cell.Value = sizes[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.Color.SetColor(Color.White);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(greenHeaderBg);
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.White);
            }

            // 3. TABLE DATA
            int currentRow = tableStartRow + 2;
            int sl = 1;

            foreach (var art in program.Articles)
            {
                int totalArtRows = art.Colors.Sum(c => c.SizeBreakdowns.Count);
                int startArtRow = currentRow;

                // Article Level Columns (Merged)
                ws.Cells[currentRow, 1, currentRow + totalArtRows - 1, 1].Merge = true;
                ws.Cells[currentRow, 1].Value = sl++;
                ws.Cells[currentRow, 2, currentRow + totalArtRows - 1, 2].Merge = true;
                ws.Cells[currentRow, 2].Value = art.OldArticleNo;
                ws.Cells[currentRow, 3, currentRow + totalArtRows - 1, 3].Merge = true;
                ws.Cells[currentRow, 3].Value = art.NewArticleNo;
                ws.Cells[currentRow, 3].Style.Font.Color.SetColor(Color.Blue);
                ws.Cells[currentRow, 4, currentRow + totalArtRows - 1, 4].Merge = true;
                ws.Cells[currentRow, 4].Value = program.CustomerName ?? "WHITE";
                ws.Cells[currentRow, 5, currentRow + totalArtRows - 1, 5].Merge = true;
                ws.Cells[currentRow, 5].Value = (int)art.PackType == 1 ? "A" : (int)art.PackType == 2 ? "B" : "A-B";
                ws.Cells[currentRow, 6, currentRow + totalArtRows - 1, 6].Merge = true;
                ws.Cells[currentRow, 6].Value = art.ItemName;
                ws.Cells[currentRow, 7, currentRow + totalArtRows - 1, 7].Merge = true;
                ws.Cells[currentRow, 7].Value = art.TotalQty;
                
                // G.TOTAL Merged Column
                ws.Cells[currentRow, 19, currentRow + totalArtRows - 1, 19].Merge = true;
                ws.Cells[currentRow, 19].Value = art.TotalQty;
                ws.Cells[currentRow, 19].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[currentRow, 19].Style.Fill.BackgroundColor.SetColor(greenHeaderBg);
                ws.Cells[currentRow, 19].Style.Font.Color.SetColor(Color.White);
                ws.Cells[currentRow, 19].Style.Font.Bold = true;

                foreach (var col in art.Colors)
                {
                    foreach (var sb in col.SizeBreakdowns)
                    {
                        ws.Row(currentRow).Height = 30;
                        ws.Cells[currentRow, 8].Value = col.ColorName;
                        ws.Cells[currentRow, 8].Style.Font.Bold = true;
                        ws.Cells[currentRow, 8].Style.Font.Color.SetColor(Color.Gray);
                        
                        ws.Cells[currentRow, 9].Value = sb.SizeM;
                        ws.Cells[currentRow, 10].Value = sb.SizeL;
                        ws.Cells[currentRow, 11].Value = sb.SizeXL;
                        ws.Cells[currentRow, 12].Value = sb.SizeXXL;
                        ws.Cells[currentRow, 13].Value = sb.SizeXXXL;
                        ws.Cells[currentRow, 14].Value = sb.Size3XL;
                        ws.Cells[currentRow, 15].Value = sb.Size4XL;
                        ws.Cells[currentRow, 16].Value = sb.Size5XL;
                        ws.Cells[currentRow, 17].Value = sb.Size6XL;

                        // Row Qty with light orange bg
                        ws.Cells[currentRow, 18].Value = sb.RowTotal;
                        ws.Cells[currentRow, 18].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[currentRow, 18].Style.Fill.BackgroundColor.SetColor(orangeTotalBg);
                        ws.Cells[currentRow, 18].Style.Font.Bold = true;
                        ws.Cells[currentRow, 18].Style.Font.Color.SetColor(Color.Brown);

                        ws.Cells[currentRow, 20].Value = sb.BuyerPackingNumber ?? "--";

                        currentRow++;
                    }
                }

                // Apply borders and alignment to the article block
                var artRange = ws.Cells[startArtRow, 1, currentRow - 1, 20];
                artRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                artRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                artRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                artRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                artRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                artRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                artRange.Style.Border.Top.Color.SetColor(borderLineColor);
                artRange.Style.Border.Bottom.Color.SetColor(borderLineColor);
                artRange.Style.Border.Left.Color.SetColor(borderLineColor);
                artRange.Style.Border.Right.Color.SetColor(borderLineColor);
            }

            // 4. FOOTER / SUMMARY AGGREGATION
            ws.Row(currentRow).Height = 35;
            ws.Cells[currentRow, 1, currentRow, 7].Merge = true;
            ws.Cells[currentRow, 1].Value = "SUMMARY AGGREGATION";
            ws.Cells[currentRow, 1].Style.Font.Bold = true;
            ws.Cells[currentRow, 1].Style.Font.Size = 10;
            ws.Cells[currentRow, 1].Style.Font.Color.SetColor(Color.Gray);
            ws.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            ws.Cells[currentRow, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[currentRow, 1, currentRow, 20].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[currentRow, 1, currentRow, 20].Style.Fill.BackgroundColor.SetColor(greySubHeaderBg);

            ws.Cells[currentRow, 8, currentRow, 18].Merge = true;
            ws.Cells[currentRow, 8].Value = "GRAND PROGRAM ACCUMULATION";
            ws.Cells[currentRow, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[currentRow, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[currentRow, 8].Style.Font.Bold = true;
            ws.Cells[currentRow, 8].Style.Font.Size = 10;
            ws.Cells[currentRow, 8].Style.Font.Color.SetColor(Color.Gray);

            ws.Cells[currentRow, 19].Value = program.Articles.Sum(a => a.TotalQty).ToString() + " PCS";
            ws.Cells[currentRow, 19].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[currentRow, 19].Style.Fill.BackgroundColor.SetColor(darkOrangeGrandTotalBg);
            ws.Cells[currentRow, 19].Style.Font.Color.SetColor(Color.White);
            ws.Cells[currentRow, 19].Style.Font.Bold = true;
            ws.Cells[currentRow, 19].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[currentRow, 19].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // 5. SIGNATURE SECTION
            currentRow += 5;
            for(int i=currentRow-4; i<=currentRow; i++) ws.Row(i).Height = 20;

            string[] signatures = { "MERCHANDISER SIGNATURE", "PRODUCTION MANAGER", "AUTHORIZED APPROVAL" };
            int[] sigCols = { 3, 10, 16 };
            for (int i = 0; i < signatures.Length; i++)
            {
                var sigCell = ws.Cells[currentRow, sigCols[i]];
                sigCell.Value = signatures[i];
                sigCell.Style.Font.Bold = true;
                sigCell.Style.Font.Size = 9;
                sigCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sigCell.Style.Border.Top.Style = ExcelBorderStyle.Dashed;
                sigCell.Style.Border.Top.Color.SetColor(Color.Gray);
            }

            // 6. PAGE SETUP AND FORMATTING
            ws.Cells.AutoFitColumns();
            // Set some manual widths for consistency
            ws.Column(1).Width = 5; // SL
            ws.Column(5).Width = 8; // Pack
            ws.Column(18).Width = 10; // Row Qty
            ws.Column(19).Width = 12; // G.Total
            
            for(int i=9; i<=17; i++) ws.Column(i).Width = 6; // Sizes

            ws.PrinterSettings.Orientation = eOrientation.Landscape;
            ws.PrinterSettings.FitToPage = true;
            ws.PrinterSettings.FitToWidth = 1;
            ws.PrinterSettings.FitToHeight = 0;
            
            ws.View.ShowGridLines = false;

            return await package.GetAsByteArrayAsync();
        }
    }
}
