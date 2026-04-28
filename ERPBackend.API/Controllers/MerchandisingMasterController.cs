using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace ERPBackend.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MerchandisingMasterController : ControllerBase
    {
        private readonly IMerchandisingMasterService _masterService;

        public MerchandisingMasterController(IMerchandisingMasterService masterService)
        {
            _masterService = masterService;
        }

        [HttpGet("seasons/{companyId}")]
        public async Task<IActionResult> GetSeasons(int companyId) 
            => Ok(await _masterService.GetAllSeasonsAsync(companyId));

        [HttpPost("seasons")]
        public async Task<IActionResult> CreateSeason(Season season) 
            => Ok(await _masterService.CreateSeasonAsync(season));

        [HttpGet("departments/{companyId}")]
        public async Task<IActionResult> GetDepartments(int companyId) 
            => Ok(await _masterService.GetAllDepartmentsAsync(companyId));

        [HttpGet("suppliers/{companyId}")]
        public async Task<IActionResult> GetSuppliers(int companyId) 
            => Ok(await _masterService.GetAllSuppliersAsync(companyId));

        [HttpGet("knit-machines/{companyId}")]
        public async Task<IActionResult> GetKnitMachines(int companyId) 
            => Ok(await _masterService.GetAllKnitMachinesAsync(companyId));

        [HttpGet("fabric-gsms/{companyId}")]
        public async Task<IActionResult> GetFabricGsms(int companyId) 
            => Ok(await _masterService.GetAllFabricGsmsAsync(companyId));

        [HttpGet("couriers/{companyId}")]
        public async Task<IActionResult> GetCouriers(int companyId) 
            => Ok(await _masterService.GetAllCouriersAsync(companyId));

        [HttpGet("shipment-modes/{companyId}")]
        public async Task<IActionResult> GetShipmentModes(int companyId) 
            => Ok(await _masterService.GetAllShipmentModesAsync(companyId));

        [HttpPost("fabric-gsms")]
        public async Task<IActionResult> CreateFabricGsm(FabricTypeGsm model) 
            => Ok(await _masterService.CreateFabricGsmAsync(model));

        [HttpGet("colors/{companyId}")]
        public async Task<IActionResult> GetColors(int companyId) 
            => Ok(await _masterService.GetAllColorsAsync(companyId));

        [HttpPost("colors")]
        public async Task<IActionResult> CreateColor(FabricColorPantone color) 
            => Ok(await _masterService.CreateColorAsync(color));

        [HttpPut("colors")]
        public async Task<IActionResult> UpdateColor(FabricColorPantone color) 
            => Ok(await _masterService.UpdateColorAsync(color));

        [HttpDelete("colors/{id}")]
        public async Task<IActionResult> DeleteColor(int id) 
        {
            var result = await _masterService.DeleteColorAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Color deleted successfully" });
        }

        [HttpPost("colors/import/{companyId}/{branchId}")]
        public async Task<IActionResult> ImportColors(IFormFile file, [FromRoute] int companyId, [FromRoute] int branchId)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");
            
            using var stream = file.OpenReadStream();
            int count = await _masterService.ImportColorsAsync(stream, companyId, branchId);
            
            return Ok(new { message = $"Successfully imported {count} colors", count });
        }

        [HttpGet("colors/template")]
        public async Task<IActionResult> GetColorTemplate()
        {
            var fileName = "Color_Library_Template.xlsx";
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Colors");
            
            worksheet.Cells[1, 1].Value = "Color Name";
            worksheet.Cells[1, 2].Value = "Pantone or Hex Code";
            
            // Sample data
            worksheet.Cells[2, 1].Value = "Midnight Blue";
            worksheet.Cells[2, 2].Value = "#191970";
            
            worksheet.Cells["A1:B1"].Style.Font.Bold = true;
            worksheet.Cells.AutoFitColumns();
            
            var fileBytes = await package.GetAsByteArrayAsync();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
