using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPBackend.Core.Entities;
using ERPBackend.Infrastructure.Data;
using ERPBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace ERPBackend.Services.Services
{
    public class IDCardService : IIDCardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        // A4 = 210mm wide. Margin 5mm each side = 200mm available.
        // 4 cards per row with 2mm gap between pair = (200 - 2) / 4 = 49.5mm per card
        // We use 48mm = 4.8cm per card, with a 2mm centre gap between pairs
        private const float CardWidthCm = 4.7f;  // card width in cm
        private const float CardHeightCm = 8.5f; // card height in cm (Standard CR80-ish)
        private const float GapBetweenPairsMm = 4f; // gap between Emp1-Back and Emp2-Front

        public IDCardService(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<byte[]> GenerateIDCardsAsync(List<int> employeeIds, string design)
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Include(e => e.Section)
                .Include(e => e.Line)
                .Include(e => e.Company)
                .Where(e => employeeIds.Contains(e.Id))
                .ToListAsync();

            if (!employees.Any())
                return Array.Empty<byte>();

            // A4 in points: 595.28 x 841.89 pt
            // We work in cm. Margin 0.5cm each side.
            // Total width available = 21.0 - 1.0 = 20.0cm
            // 4 cards * 4.7cm = 18.8cm + gap 0.4cm between pairs = 19.2cm  ✓

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginHorizontal(0.5f, Unit.Centimetre);
                    page.MarginVertical(0.4f, Unit.Centimetre);
                    page.PageColor(Colors.White);

                    page.Content().Table(table =>
                    {
                        // 4 constant-width columns matching the card width exactly
                        // plus a tiny gap column in the middle between pairs
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(CardWidthCm, Unit.Centimetre); // Emp A - Front
                            cols.ConstantColumn(CardWidthCm, Unit.Centimetre); // Emp A - Back
                            cols.ConstantColumn(GapBetweenPairsMm, Unit.Millimetre); // visual separator
                            cols.ConstantColumn(CardWidthCm, Unit.Centimetre); // Emp B - Front
                            cols.ConstantColumn(CardWidthCm, Unit.Centimetre); // Emp B - Back
                        });

                        // Each table row = 2 employees (pair)
                        // 3 rows × 2 employees = 6 employees = 12 cards per page
                        for (int i = 0; i < employees.Count; i += 2)
                        {
                            var empA = employees[i];
                            var empB = (i + 1 < employees.Count) ? employees[i + 1] : null;

                            float rowGap = 6f; // vertical gap between rows in mm

                            // Emp A - Front
                            table.Cell().PaddingBottom(rowGap, Unit.Millimetre)
                                .Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Height(CardHeightCm, Unit.Centimetre)
                                .Element(c => ComposeCardFront(c, empA, design));

                            // Emp A - Back
                            table.Cell().PaddingBottom(rowGap, Unit.Millimetre)
                                .Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Height(CardHeightCm, Unit.Centimetre)
                                .Element(c => ComposeCardBack(c, empA, design));

                            // Gap column (empty separator)
                            table.Cell().PaddingBottom(rowGap, Unit.Millimetre).Element(c => c.Container());

                            // Emp B - Front
                            if (empB != null)
                            {
                                table.Cell().PaddingBottom(rowGap, Unit.Millimetre)
                                    .Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                                    .Height(CardHeightCm, Unit.Centimetre)
                                    .Element(c => ComposeCardFront(c, empB, design));

                                table.Cell().PaddingBottom(rowGap, Unit.Millimetre)
                                    .Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                                    .Height(CardHeightCm, Unit.Centimetre)
                                    .Element(c => ComposeCardBack(c, empB, design));
                            }
                            else
                            {
                                table.Cell().PaddingBottom(rowGap, Unit.Millimetre).Element(c => c.Container());
                                table.Cell().PaddingBottom(rowGap, Unit.Millimetre).Element(c => c.Container());
                            }
                        }
                    });
                });
            });

            return document.GeneratePdf();
        }

        // ─── Front dispatcher ──────────────────────────────────────────────────────

        private void ComposeCardFront(IContainer container, Employee emp, string design)
        {
            switch (design?.ToLower())
            {
                case "classic":  ComposeClassicFront(container, emp);  break;
                case "minimal":  ComposeMinimalFront(container, emp);  break;
                case "compact":  ComposeCompactFront(container, emp);  break;
                case "corporate": ComposeCorporateFront(container, emp); break;
                case "vibrant":   ComposeVibrantFront(container, emp);   break;
                case "industrial": ComposeIndustrialFront(container, emp); break;
                case "professional": ComposeProfessionalFront(container, emp); break;
                default:         ComposeModernFront(container, emp);   break;
            }
        }

        // ─── MODERN FRONT ──────────────────────────────────────────────────────────

        private void ComposeModernFront(IContainer container, Employee emp)
        {
            container.Background(Colors.White).Column(column =>
            {
                // Header bar with Logo and Company Name
                column.Item()
                    .MinHeight(1.2f, Unit.Centimetre)
                    .Background(Colors.Blue.Darken4)
                    .PaddingHorizontal(5)
                    .AlignMiddle()
                    .Row(row =>
                    {
                        // Logo
                        row.ConstantItem(0.8f, Unit.Centimetre).AlignCenter().AlignMiddle().Element(c => 
                        {
                            DrawLogo(c, emp.Company?.LogoPath);
                        });
                        
                        // Company Name
                        row.RelativeItem().PaddingLeft(5).AlignMiddle().Text(emp.Company?.CompanyNameEn ?? "HR HUB TECH")
                            .FontColor(Colors.White).FontSize(8).Bold();
                    });

                // Photo
                column.Item().PaddingTop(4).AlignCenter()
                    .Width(1.8f, Unit.Centimetre).Height(1.8f, Unit.Centimetre)
                    .Background(Colors.White).Border(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(2).Background(Colors.Grey.Lighten4)
                    .Element(c => DrawImage(c, emp.ProfileImageUrl));

                // Centralized Details
                column.Item().PaddingHorizontal(4).PaddingTop(2).Column(c =>
                {
                    // Card No
                    c.Item().AlignCenter().Text("Card No: " + emp.EmployeeId).FontSize(6.5f).Bold().FontColor(Colors.Grey.Darken3);
                    
                    // Name
                    c.Item().AlignCenter().Text(emp.FullNameEn).FontSize(8.5f).Bold().FontColor(Colors.Blue.Darken4);
                    
                    // Designation
                    c.Item().AlignCenter().Text(emp.Designation?.NameEn ?? "").FontSize(6.5f).Bold().FontColor(Colors.BlueGrey.Medium);
                    
                    // Department
                    c.Item().AlignCenter().Text(emp.Department?.NameEn ?? "").FontSize(6f).FontColor(Colors.Grey.Medium);
                    
                    // Section
                    c.Item().AlignCenter().Text("Section: " + (emp.Section?.NameEn ?? "-")).FontSize(5.5f).FontColor(Colors.Grey.Medium);
                    
                    // Line
                    c.Item().AlignCenter().Text("Line: " + (emp.Line?.NameEn ?? "-")).FontSize(5.5f).FontColor(Colors.Grey.Medium);

                    // Joining Date
                    c.Item().PaddingTop(2).AlignCenter().Text("Joining: " + emp.JoinDate.ToString("dd MMM yyyy")).FontSize(5.5f).FontColor(Colors.Grey.Medium);
                });
            });
        }

        // ─── CLASSIC FRONT ─────────────────────────────────────────────────────────

        private void ComposeClassicFront(IContainer container, Employee emp)
        {
            container.Background(Colors.Grey.Lighten5).Column(column =>
            {
                column.Item().Padding(4).Border(1).BorderColor(Colors.Black)
                    .Background(Colors.White).AlignCenter()
                    .Text("OFFICIAL ID CARD").FontSize(8).Bold().FontColor(Colors.Black);

                column.Item().PaddingTop(6).AlignCenter()
                    .Width(2.0f, Unit.Centimetre).Height(2.4f, Unit.Centimetre)
                    .Border(1).BorderColor(Colors.Black).Padding(2)
                    .Element(c => DrawImage(c, emp.ProfileImageUrl));

                column.Item().Padding(4).Column(c =>
                {
                    c.Item().AlignCenter().Text(emp.FullNameEn).FontSize(9).Bold().Underline();
                    c.Item().AlignCenter().Text(emp.Designation?.NameEn ?? "").FontSize(6.5f);
                    c.Item().AlignCenter().Text(emp.Department?.NameEn ?? "").FontSize(6).Italic();
                    c.Spacing(6);
                    c.Item().AlignCenter().Text("ID: " + emp.EmployeeId).FontSize(7).Bold();
                });

                column.Item().AlignBottom().PaddingBottom(4).AlignCenter()
                    .Text(emp.Company?.CompanyNameEn ?? "HR HUB TECH LTD.").FontSize(6).Bold();
            });
        }

        // ─── MINIMAL FRONT ─────────────────────────────────────────────────────────

        private void ComposeMinimalFront(IContainer container, Employee emp)
        {
            container.Background(Colors.White).Padding(8).Column(column =>
            {
                column.Item().AlignCenter()
                    .Width(2.0f, Unit.Centimetre).Height(2.0f, Unit.Centimetre)
                    .Background(Colors.Grey.Lighten4)
                    .Element(c => DrawImage(c, emp.ProfileImageUrl));

                column.Item().PaddingTop(8).AlignCenter().Text(emp.FullNameEn)
                    .FontSize(9).Bold().FontColor(Colors.BlueGrey.Darken4);
                column.Item().AlignCenter().Text(emp.Designation?.NameEn ?? "")
                    .FontSize(6).FontColor(Colors.Grey.Medium);

                column.Item().PaddingTop(8).Background(Colors.Grey.Lighten5).Padding(4).Column(c =>
                {
                    c.Item().Text("IDENTITY NUMBER").FontSize(4.5f).FontColor(Colors.Grey.Medium);
                    c.Item().Text(emp.EmployeeId).FontSize(8).Bold().FontColor(Colors.BlueGrey.Darken2);
                });

                column.Item().AlignBottom().Height(0.25f, Unit.Centimetre).Background(Colors.Blue.Medium);
            });
        }

        // ─── COMPACT FRONT ─────────────────────────────────────────────────────────

        private void ComposeCompactFront(IContainer container, Employee emp)
        {
            container.Background(Colors.BlueGrey.Darken4).Padding(8).Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.ConstantItem(8).Height(8).Background(Colors.Blue.Medium)
                        .AlignCenter().AlignMiddle().Text("H").FontSize(5).FontColor(Colors.White).Bold();
                    row.RelativeItem().PaddingLeft(4)
                        .Text(emp.Company?.CompanyNameEn ?? "HR HUB TECH").FontSize(6.5f).Bold().FontColor(Colors.White);
                });

                column.Item().PaddingTop(8).Row(row =>
                {
                    row.ConstantItem(1.6f, Unit.Centimetre).Height(2.0f, Unit.Centimetre)
                        .Border(1).BorderColor(Colors.Blue.Medium)
                        .Element(c => DrawImage(c, emp.ProfileImageUrl));

                    row.RelativeItem().PaddingLeft(6).Column(c =>
                    {
                        c.Item().Text(emp.FullNameEn).FontSize(8).Bold().FontColor(Colors.White);
                        c.Item().Text(emp.Designation?.NameEn ?? "").FontSize(5.5f).Bold().FontColor(Colors.Blue.Lighten2);
                        c.Item().Text(emp.Department?.NameEn ?? "").FontSize(5).FontColor(Colors.Grey.Lighten1);
                    });
                });

                column.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("ISSUE DATE").FontSize(4.5f).FontColor(Colors.Grey.Lighten2);
                        c.Item().Text(DateTime.Now.ToString("dd/MM/yyyy")).FontSize(6).FontColor(Colors.White);
                    });
                    row.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().AlignRight().Text("CARD NO").FontSize(4.5f).FontColor(Colors.Grey.Lighten2);
                        c.Item().AlignRight().Text(emp.EmployeeId).FontSize(6.5f).Bold().FontColor(Colors.Blue.Lighten2);
                    });
                });
            });
        }

        // ─── CORPORATE FRONT ────────────────────────────────────────────────────────

        private void ComposeCorporateFront(IContainer container, Employee emp)
        {
            container.Background(Colors.White).Column(column =>
            {
                column.Item().Height(0.3f, Unit.Centimetre).Background(Colors.Indigo.Darken4);
                
                column.Item().Padding(5).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(emp.Company?.CompanyNameEn ?? "CORPORATE HUB").FontSize(8).Bold().FontColor(Colors.Indigo.Darken4);
                        c.Item().Text("EXECUTIVE IDENTITY").FontSize(4).LetterSpacing(0.2f).FontColor(Colors.Grey.Medium);
                    });
                    row.ConstantItem(0.6f, Unit.Centimetre).Element(c => DrawLogo(c, emp.Company?.LogoPath));
                });

                column.Item().PaddingHorizontal(5).Row(row =>
                {
                    row.ConstantItem(1.8f, Unit.Centimetre).Height(2.2f, Unit.Centimetre).Background(Colors.Grey.Lighten4).Element(c => DrawImage(c, emp.ProfileImageUrl));
                    row.RelativeItem().PaddingLeft(5).Column(c =>
                    {
                        c.Item().PaddingTop(5).Text(emp.FullNameEn).FontSize(9).Bold().FontColor(Colors.Black);
                        c.Item().Text(emp.Designation?.NameEn ?? "").FontSize(6).FontColor(Colors.Indigo.Medium);
                        
                        c.Item().PaddingTop(8).Column(details =>
                        {
                            details.Item().Text("DEPARTMENT").FontSize(4).FontColor(Colors.Grey.Medium);
                            details.Item().Text(emp.Department?.NameEn ?? "-").FontSize(5.5f).Bold();
                        });
                    });
                });

                column.Item().Padding(5).Background(Colors.Indigo.Darken4).Row(row =>
                {
                    row.RelativeItem().Text("ID: " + emp.EmployeeId).FontSize(6).Bold().FontColor(Colors.White);
                    row.RelativeItem().AlignRight().Text("JOINED: " + emp.JoinDate.ToString("yyyy")).FontSize(6).FontColor(Colors.White);
                });
            });
        }

        // ─── VIBRANT FRONT ──────────────────────────────────────────────────────────

        private void ComposeVibrantFront(IContainer container, Employee emp)
        {
            container.Background(Colors.White).Column(column =>
            {
                column.Item().MinHeight(2.2f, Unit.Centimetre).Background(Colors.DeepOrange.Medium).Padding(5).Column(c =>
                {
                    c.Item().Text(emp.Company?.CompanyNameEn ?? "VIBRANT TECH").FontColor(Colors.White).FontSize(9).Bold();
                    c.Item().AlignCenter().PaddingTop(5).Width(2.2f, Unit.Centimetre).Height(2.2f, Unit.Centimetre).Container().Background(Colors.White).Padding(2).Container().Element(c2 => DrawImage(c2, emp.ProfileImageUrl));
                });

                column.Item().PaddingTop(1.2f, Unit.Centimetre).AlignCenter().Column(c =>
                {
                    c.Item().AlignCenter().Text(emp.FullNameEn).FontSize(10).Bold().FontColor(Colors.Grey.Darken4);
                    c.Item().AlignCenter().Text(emp.Designation?.NameEn ?? "").FontSize(7).FontColor(Colors.DeepOrange.Medium).Bold();
                    
                    c.Item().PaddingTop(5).PaddingHorizontal(10).Row(r =>
                    {
                        r.RelativeItem().Column(details =>
                        {
                            details.Item().AlignCenter().Text("ID NO").FontSize(4).FontColor(Colors.Grey.Medium);
                            details.Item().AlignCenter().Text(emp.EmployeeId).FontSize(6).Bold();
                        });
                        r.RelativeItem().Column(details =>
                        {
                            details.Item().AlignCenter().Text("BLOOD").FontSize(4).FontColor(Colors.Grey.Medium);
                            details.Item().AlignCenter().Text(emp.BloodGroup ?? "O+").FontSize(6).Bold().FontColor(Colors.Red.Medium);
                        });
                    });
                });

                column.Item().AlignBottom().PaddingBottom(4).AlignCenter().Text("WWW.HRHUB.COM").FontSize(4).FontColor(Colors.Grey.Lighten1);
            });
        }

        // ─── INDUSTRIAL FRONT ───────────────────────────────────────────────────────

        private void ComposeIndustrialFront(IContainer container, Employee emp)
        {
            container.Background(Colors.Grey.Lighten4).Border(1).BorderColor(Colors.Grey.Darken3).Column(column =>
            {
                column.Item().Background(Colors.Grey.Darken3).Padding(4).Row(row =>
                {
                    row.RelativeItem().Text(emp.Company?.CompanyNameEn ?? "INDUSTRIAL WORKS").FontColor(Colors.Amber.Medium).FontSize(8).Bold();
                    row.ConstantItem(15).Height(15).Element(c => DrawLogo(c, emp.Company?.LogoPath));
                });

                column.Item().Padding(5).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("NAME").FontSize(4).FontColor(Colors.Grey.Medium);
                        c.Item().Text(emp.FullNameEn).FontSize(9).Bold();
                        
                        c.Item().PaddingTop(4).Text("DESIGNATION").FontSize(4).FontColor(Colors.Grey.Medium);
                        c.Item().Text(emp.Designation?.NameEn ?? "").FontSize(7).Bold();
                        
                        c.Item().PaddingTop(4).Text("UNIT/LINE").FontSize(4).FontColor(Colors.Grey.Medium);
                        c.Item().Text((emp.Section?.NameEn ?? "GEN") + " / " + (emp.Line?.NameEn ?? "L1")).FontSize(6).Bold();
                    });
                    
                    row.ConstantItem(1.8f, Unit.Centimetre).Height(2.4f, Unit.Centimetre).Border(1).BorderColor(Colors.Grey.Darken3).Element(c => DrawImage(c, emp.ProfileImageUrl));
                });

                column.Item().AlignBottom().Background(Colors.Amber.Medium).PaddingVertical(2).AlignCenter().Text("ID: " + emp.EmployeeId).FontSize(7).Bold().FontColor(Colors.Black);
            });
        }

        // ─── PROFESSIONAL FRONT ─────────────────────────────────────────────────────

        private void ComposeProfessionalFront(IContainer container, Employee emp)
        {
            container.Background(Colors.White).Column(column =>
            {
                column.Item().Padding(5).Row(row =>
                {
                    row.ConstantItem(0.7f, Unit.Centimetre).Element(c => DrawLogo(c, emp.Company?.LogoPath));
                    row.RelativeItem().PaddingLeft(5).AlignMiddle().Text(emp.Company?.CompanyNameEn ?? "PROFESSIONAL HUB").FontSize(7).Bold().FontColor(Colors.Blue.Darken4);
                });

                column.Item().PaddingHorizontal(5).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().PaddingTop(5).Text(emp.FullNameEn).FontSize(10).Bold();
                        c.Item().Text(emp.Designation?.NameEn ?? "").FontSize(7).FontColor(Colors.Blue.Medium);
                        c.Item().PaddingTop(5).Text("Employee ID").FontSize(4).FontColor(Colors.Grey.Medium);
                        c.Item().Text(emp.EmployeeId).FontSize(7).Bold();
                    });
                    row.ConstantItem(1.8f, Unit.Centimetre).Height(2.4f, Unit.Centimetre).Element(c => DrawImage(c, emp.ProfileImageUrl));
                });

                column.Item().PaddingHorizontal(5).PaddingTop(5).Column(c =>
                {
                    c.Item().BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(2).Row(r =>
                    {
                        r.RelativeItem().Text("Department").FontSize(5).FontColor(Colors.Grey.Medium);
                        r.RelativeItem().AlignRight().Text(emp.Department?.NameEn ?? "-").FontSize(5).Bold();
                    });
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Joining Date").FontSize(5).FontColor(Colors.Grey.Medium);
                        r.RelativeItem().AlignRight().Text(emp.JoinDate.ToString("dd/MM/yyyy")).FontSize(5).Bold();
                    });
                });

                column.Item().AlignBottom().Height(0.5f, Unit.Centimetre).Background(Colors.Blue.Darken4);
            });
        }

        // ─── BACK (shared for all designs) ────────────────────────────────────────

        private void ComposeCardBack(IContainer container, Employee emp, string design)
        {
            container.Background(Colors.White).Column(column =>
            {
                column.Item().Padding(5).Column(c =>
                {
                    c.Item().AlignCenter().Text("TERMS & CONDITIONS")
                        .FontSize(7).Bold().FontColor(Colors.Blue.Darken4);

                    c.Item().PaddingTop(4)
                        .Text("• This card is the property of " + (emp.Company?.CompanyNameEn ?? "Company") + ".")
                        .FontSize(5.5f);
                    c.Item().Text("• Loss of this card must be reported immediately to HR department.")
                        .FontSize(5.5f);
                    c.Item().Text("• This card is non-transferable and must be carried at all times.")
                        .FontSize(5.5f);
                    c.Item().Text("• Please return this card to HR upon termination or resignation.")
                        .FontSize(5.5f);

                    c.Spacing(6);

                    c.Item().AlignCenter().Text("EMERGENCY CONTACT")
                        .FontSize(6).Bold().FontColor(Colors.BlueGrey.Darken2);
                    c.Item().AlignCenter()
                        .Text(emp.EmergencyContactPhone ?? "Admin: +880 1700-000000")
                        .FontSize(6).Bold();

                    c.Spacing(8);

                    c.Item().Row(row =>
                    {
                        row.RelativeItem().Column(sig =>
                        {
                            sig.Item().AlignCenter().Height(0.4f, Unit.Centimetre).Background(Colors.Grey.Lighten5);
                            sig.Item().PaddingTop(2).BorderTop(0.5f).AlignCenter()
                                .Text("Employee Signature").FontSize(4.5f);
                        });
                        row.ConstantItem(8);
                        row.RelativeItem().Column(sig =>
                        {
                            sig.Item().AlignCenter().Height(0.4f, Unit.Centimetre).Background(Colors.Grey.Lighten5);
                            sig.Item().PaddingTop(2).BorderTop(0.5f).AlignCenter()
                                .Text("Authorized Signature").FontSize(4.5f);
                        });
                    });
                });

                column.Item().AlignBottom().PaddingBottom(6).Column(c =>
                {
                    c.Item().AlignCenter()
                        .Text(emp.Company?.AddressEn ?? "Factory: Masterbari, Gazipur, Bangladesh")
                        .FontSize(5).FontColor(Colors.Grey.Medium);
                    c.Item().AlignCenter().Text("System Powered by: HR HUB TECH")
                        .FontSize(4.5f).FontColor(Colors.Blue.Medium);
                });
            });
        }

        // ─── Helpers ───────────────────────────────────────────────────────────────

        private void DrawLogo(IContainer container, string? logoPath)
        {
            string path = !string.IsNullOrEmpty(logoPath)
                ? Path.Combine(_hostEnvironment.WebRootPath, logoPath.TrimStart('/'))
                : "";

            if (File.Exists(path))
                container.Image(path);
            else
                container.AlignCenter().AlignMiddle().Text("LOGO").FontSize(4).FontColor(Colors.White);
        }

        private void DrawImage(IContainer container, string? profileImageUrl)
        {
            string imagePath = !string.IsNullOrEmpty(profileImageUrl)
                ? Path.Combine(_hostEnvironment.WebRootPath, profileImageUrl.TrimStart('/'))
                : "";

            if (File.Exists(imagePath))
                container.Image(imagePath);
            else
                container.AlignCenter().AlignMiddle().Text("PHOTO").FontSize(5).FontColor(Colors.Grey.Lighten1);
        }

        private void DrawDetailRow(IContainer container, string label, string value, string? valueColor = null)
        {
            container.Row(r =>
            {
                r.RelativeItem().Text(label).FontSize(5.5f).Bold().FontColor(Colors.Grey.Medium);
                r.RelativeItem().AlignRight().Text(value).FontSize(5.5f).Bold()
                    .FontColor(valueColor ?? Colors.Black);
            });
        }
    }
}
