using System;
using System.IO;
using System.Threading.Tasks;
using ERPBackend.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Ideally restrict to SuperAdmin in production: [Authorize(Roles = "SuperAdmin")]
    public class DatabaseController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;

        public DatabaseController(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpPost("backup")]
        public async Task<IActionResult> Backup()
        {
            try
            {
                var fileName = await _databaseService.BackupDatabaseAsync();
                return Ok(new { message = "Backup created successfully.", fileName });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("restore")]
        public async Task<IActionResult> Restore([FromBody] RestoreRequest request)
        {
            if (string.IsNullOrEmpty(request.FileName))
                return BadRequest("File name is required.");

            try
            {
                var result = await _databaseService.RestoreDatabaseAsync(request.FileName);
                return Ok(new { message = "Database restored successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("upload-bak")]
        public async Task<IActionResult> UploadBak(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (!file.FileName.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only .bak files are allowed.");

            try
            {
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    var fileName = await _databaseService.UploadBackupFileAsync(ms.ToArray(), file.FileName);
                    return Ok(new { message = "Backup file uploaded successfully.", fileName });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("download-backup/{fileName}")]
        public IActionResult DownloadBackup(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Backups", fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound("Backup file not found.");

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }
    }

    public class RestoreRequest
    {
        public string FileName { get; set; } = string.Empty;
    }
}
