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
        public async Task<ActionResult<IEnumerable<ProgramOrderDto>>> GetAll(int companyId)
        {
            try 
            {
                var results = await _orderSheetService.GetAllAsync(companyId);
                return Ok(results);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching programs", error = ex.Message });
            }
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult<ProgramOrderDto>> GetById(int id)
        {
            var program = await _orderSheetService.GetDtoByIdAsync(id);
            if (program == null) return NotFound();
            return Ok(program);
        }

        [HttpPost]
        public async Task<ActionResult<ProgramOrder>> Create(ProgramOrder programOrder)
        {
            var created = await _orderSheetService.CreateAsync(programOrder);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProgramOrder programOrder)
        {
            try 
            {
                if (id != programOrder.Id) return BadRequest(new { message = "ID mismatch" });
                await _orderSheetService.UpdateAsync(programOrder);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error updating program", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _orderSheetService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("{id}/summary")]
        public ActionResult<ProgramSummaryDto> GetSummary(int id)
        {
            return Ok(_orderSheetService.GetOrderSummary(id));
        }

        [HttpGet("{companyId}/global-summary")]
        public async Task<ActionResult<GlobalProgramSummaryDto>> GetGlobalSummary(int companyId)
        {
            return Ok(await _orderSheetService.GetGlobalSummaryAsync(companyId));
        }

        [HttpGet("template")]
        public async Task<IActionResult> GetTemplate()
        {
            var bytes = await _orderSheetService.DownloadTemplateAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Order_Template.xlsx");
        }

        [HttpPost("preview")]
        public async Task<IActionResult> Preview([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");
            using var stream = file.OpenReadStream();
            var result = await _orderSheetService.ParseExcelAsync(stream);
            return Ok(result);
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import(MultiSheetOrderImportDto data)
        {
            var companyId = 1; // From Auth usually
            var branchId = 1;
            int count = await _orderSheetService.ImportOrderSheetsAsync(data, companyId, branchId);
            return Ok(new { message = $"Successfully imported {count} programs", count });
        }

        [HttpGet("export/{id}")]
        public async Task<IActionResult> Export(int id)
        {
            var bytes = await _orderSheetService.ExportOrderSheetAsync(id);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Order_{id}.xlsx");
        }
    }
}
