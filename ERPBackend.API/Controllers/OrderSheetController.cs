using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using ERPBackend.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ERPBackend.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderSheetController : ControllerBase
    {
        private readonly IOrderSheetService _orderSheetService;

        public OrderSheetController(IOrderSheetService orderSheetService)
        {
            _orderSheetService = orderSheetService;
        }

        [HttpGet("{companyId}")]
        public async Task<ActionResult<IEnumerable<OrderSheetDto>>> GetAll(int companyId)
        {
            try 
            {
                var results = await _orderSheetService.GetAllAsync(companyId);
                return Ok(results);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Error fetching order sheets", 
                    error = ex.Message, 
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace 
                });
            }
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult<OrderSheetDto>> GetById(int id)
        {
            var orderSheet = await _orderSheetService.GetDtoByIdAsync(id);
            if (orderSheet == null) return NotFound();
            return Ok(orderSheet);
        }

        [HttpPost]
        public async Task<ActionResult<OrderSheet>> Create(OrderSheet orderSheet)
        {
            var created = await _orderSheetService.CreateAsync(orderSheet);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, OrderSheet orderSheet)
        {
            if (id != orderSheet.Id) return BadRequest();
            await _orderSheetService.UpdateAsync(orderSheet);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _orderSheetService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("{id}/summary")]
        public ActionResult<OrderSummaryDto> GetSummary(int id)
        {
            return Ok(_orderSheetService.GetOrderSummary(id));
        }

        [HttpGet("{companyId}/global-summary")]
        public async Task<ActionResult<GlobalOrderSummaryDto>> GetGlobalSummary(int companyId)
        {
            return Ok(await _orderSheetService.GetGlobalSummaryAsync(companyId));
        }

        [HttpPost("preview")]
        public async Task<ActionResult<List<OrderSheetImportDto>>> Preview(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File is empty");
            using var stream = file.OpenReadStream();
            var results = await _orderSheetService.ParseExcelAsync(stream);
            return Ok(results);
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] List<OrderSheetImportDto> data, [FromQuery] int companyId, [FromQuery] int branchId)
        {
            if (data == null || !data.Any()) return BadRequest("No data to import");
            var count = await _orderSheetService.ImportOrderSheetsAsync(data, companyId, branchId);
            return Ok(new { message = "Import successful", count });
        }

        [HttpGet("template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            var bytes = await _orderSheetService.DownloadTemplateAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OrderSheet_Import_Template.xlsx");
        }

        [HttpGet("export/{id}")]
        public async Task<IActionResult> Export(int id)
        {
            var bytes = await _orderSheetService.ExportOrderSheetAsync(id);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"OrderSheet_{id}_Analysis.xlsx");
        }
    }
}
