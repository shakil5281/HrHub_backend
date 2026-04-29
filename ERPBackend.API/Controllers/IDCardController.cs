using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERPBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class IDCardController : ControllerBase
    {
        private readonly IIDCardService _idCardService;

        public IDCardController(IIDCardService idCardService)
        {
            _idCardService = idCardService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateIDCards([FromBody] IDCardRequest request)
        {
            try
            {
                Console.WriteLine($"IDCard generation requested for {request?.EmployeeIds?.Count ?? 0} employees. Design: {request?.Design}");

                if (request == null || request.EmployeeIds == null || request.EmployeeIds.Count == 0)
                {
                    Console.WriteLine("BadRequest: No employees selected.");
                    return BadRequest("No employees selected.");
                }

                var pdfBytes = await _idCardService.GenerateIDCardsAsync(request.EmployeeIds, request.Design);
                
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    Console.WriteLine("NotFound: No employees found for the provided IDs.");
                    return NotFound("No employees found for the provided IDs.");
                }

                var fileName = $"IDCards_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                Console.WriteLine($"Success: PDF generated, size: {pdfBytes.Length} bytes.");
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("FATAL ERROR in IDCard generation:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception:");
                    Console.WriteLine(ex.InnerException.Message);
                }
                return StatusCode(500, ex.Message);
            }
        }
    }

    public class IDCardRequest
    {
        public List<int> EmployeeIds { get; set; } = new List<int>();
        public string Design { get; set; } = "modern";
    }
}
