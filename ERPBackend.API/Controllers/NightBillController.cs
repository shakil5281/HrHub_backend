using ERPBackend.Core.DTOs;
using ERPBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NightBillController : ControllerBase
    {
        private readonly INightBillService _nightBillService;

        public NightBillController(INightBillService nightBillService)
        {
            _nightBillService = nightBillService;
        }

        [HttpGet]
        public async Task<ActionResult<NightBillResponseDto>> GetNightBills(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? employeeId,
            [FromQuery] int? departmentId,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm)
        {
            try
            {
                var result = await _nightBillService.GetNightBillsAsync(fromDate, toDate, employeeId, departmentId, status, searchTerm);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching night bills", error = ex.Message });
            }
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessNightBills([FromBody] BillProcessRequestDto request)
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
                var processedCount = await _nightBillService.ProcessNightBillsAsync(request, userName);
                return Ok(new { message = $"Successfully processed {processedCount} Night Bill records." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing night bills", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNightBill(int id)
        {
            try
            {
                var success = await _nightBillService.DeleteNightBillAsync(id);
                if (!success) return NotFound();
                return Ok(new { message = "Record deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting record", error = ex.Message });
            }
        }

        [HttpPost("delete-multiple")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            try
            {
                var deletedCount = await _nightBillService.DeleteMultipleAsync(ids);
                if (deletedCount == 0) return NotFound();
                return Ok(new { message = $"Successfully deleted {deletedCount} records" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting records", error = ex.Message });
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportExcel(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? employeeId,
            [FromQuery] int? departmentId,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm)
        {
            try
            {
                var data = await _nightBillService.GetNightBillsAsync(fromDate, toDate, employeeId, departmentId, status, searchTerm);
                
                using var package = new OfficeOpenXml.ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Night Bill Report");

                // Page Setup
                worksheet.PrinterSettings.PaperSize = OfficeOpenXml.ePaperSize.A4;
                worksheet.PrinterSettings.TopMargin = 0.50M;
                worksheet.PrinterSettings.BottomMargin = 0.25M;
                worksheet.PrinterSettings.LeftMargin = 0.25M;
                worksheet.PrinterSettings.RightMargin = 0.25M;
                worksheet.View.ShowGridLines = false;

                // Footer
                worksheet.HeaderFooter.OddFooter.LeftAlignedText = "Prepared By";
                worksheet.HeaderFooter.OddFooter.CenteredText = "Admin(A.G.M)";
                worksheet.HeaderFooter.OddFooter.RightAlignedText = "Approved By";

                // Get some metadata from first record if exists
                string companyName = "EKUSHE FASHIONS LTD"; 
                string address = "Masterbari, Gazipur City, Gazipur.";
                string dateStr = fromDate?.ToString("dd-MM-yyyy") ?? DateTime.Now.ToString("dd-MM-yyyy");
                string shiftName = data.Records.FirstOrDefault()?.ShiftName ?? "General";

                // Styling
                var titleStyle = worksheet.Cells["A1:H1"];
                titleStyle.Merge = true;
                titleStyle.Value = companyName;
                titleStyle.Style.Font.Size = 18;
                titleStyle.Style.Font.Bold = true;
                titleStyle.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                var addrStyle = worksheet.Cells["A2:H2"];
                addrStyle.Merge = true;
                addrStyle.Value = address;
                addrStyle.Style.Font.Size = 11;
                addrStyle.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                var reportTitleStyle = worksheet.Cells["A3:H3"];
                reportTitleStyle.Merge = true;
                reportTitleStyle.Value = "Night Bill";
                reportTitleStyle.Style.Font.Size = 12;
                reportTitleStyle.Style.Font.Bold = true;
                reportTitleStyle.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                var dateHeaderStyle = worksheet.Cells["A4:H4"];
                dateHeaderStyle.Merge = true;
                dateHeaderStyle.Value = dateStr;
                dateHeaderStyle.Style.Font.Size = 11;
                dateHeaderStyle.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // Table Header (Now on Row 5)
                string[] headers = { "Sl", "ID", "Name", "Designation", "In time", "Out time", "Amount", "Signature" };
                var headerRow = worksheet.Row(5);
                headerRow.Height = 25;
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[5, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                }

                // Data Rows (Starting from Row 6)
                int rowIdx = 6;
                int sl = 1;
                foreach (var item in data.Records)
                {
                    worksheet.Row(rowIdx).Height = 25;
                    worksheet.Cells[rowIdx, 1].Value = sl++;
                    worksheet.Cells[rowIdx, 2].Value = item.EmployeeId;
                    worksheet.Cells[rowIdx, 3].Value = item.EmployeeName;
                    worksheet.Cells[rowIdx, 4].Value = item.Designation;
                    worksheet.Cells[rowIdx, 5].Value = item.InTime?.ToString("HH:mm") ?? "";
                    worksheet.Cells[rowIdx, 6].Value = item.OutTime?.ToString("HH:mm") ?? "";
                    worksheet.Cells[rowIdx, 7].Value = item.Amount;
                    worksheet.Cells[rowIdx, 8].Value = ""; // Signature column

                    // Borders and Alignment
                    for (int c = 1; c <= 8; c++)
                    {
                        var cell = worksheet.Cells[rowIdx, c];
                        cell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        
                        // Rule: Name (3) and Designation (4) are Left, others are Center
                        if (c == 3 || c == 4)
                            cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        else
                            cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }

                    rowIdx++;
                }

                // Total Row
                worksheet.Row(rowIdx).Height = 25;
                var totalRowRange = worksheet.Cells[rowIdx, 1, rowIdx, 6];
                totalRowRange.Merge = true;
                totalRowRange.Value = "Total";
                totalRowRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                totalRowRange.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                totalRowRange.Style.Font.Bold = true;
                totalRowRange.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                var totalAmountCell = worksheet.Cells[rowIdx, 7];
                totalAmountCell.Formula = $"SUM(G6:G{rowIdx - 1})";
                totalAmountCell.Style.Font.Bold = true;
                totalAmountCell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                totalAmountCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                totalAmountCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                worksheet.Cells[rowIdx, 8].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                rowIdx++;

                // In words row
                decimal totalAmount = data.Summary.TotalAmount;
                string inWords = NumberToWords((double)totalAmount);
                var inWordsRange = worksheet.Cells[rowIdx, 1, rowIdx, 8];
                inWordsRange.Merge = true;
                inWordsRange.Value = $"In words taka : {inWords} taka only.";
                inWordsRange.Style.Font.Italic = true;

                // Column Widths
                worksheet.Column(1).Width = 5;  // Sl
                worksheet.Column(2).Width = 10; // ID (user didn't specify, keeping reasonable)
                worksheet.Column(3).Width = 25; // Name
                worksheet.Column(4).Width = 21; // Designation
                worksheet.Column(5).Width = 8;  // InTime
                worksheet.Column(6).Width = 8;  // OutTime
                worksheet.Column(7).Width = 8;  // Amount
                worksheet.Column(8).Width = 13;// Signature

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"Night_Bill_{dateStr}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Export failed", error = ex.Message });
            }
        }

        private static string NumberToWords(double number)
        {
            if (number == 0) return "Zero";
            if (number < 0) return "Minus " + NumberToWords(Math.Abs(number));

            string words = "";

            long intPart = (long)number;
            if (intPart > 0)
            {
                if (intPart >= 100000)
                {
                    words += NumberToWords(intPart / 100000) + " lac ";
                    intPart %= 100000;
                }
                
                if (intPart >= 1000)
                {
                    words += NumberToWords(intPart / 1000) + " thousand ";
                    intPart %= 1000;
                }

                if (intPart >= 100)
                {
                    words += NumberToWords(intPart / 100) + " hundred ";
                    intPart %= 100;
                }

                if (intPart > 0)
                {
                    if (words != "") words += "and ";
                    var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                    var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                    if (intPart < 20)
                        words += unitsMap[intPart];
                    else
                    {
                        words += tensMap[intPart / 10];
                        if ((intPart % 10) > 0) words += "-" + unitsMap[intPart % 10];
                    }
                }
            }

            return words.Trim();
        }
    }
}
