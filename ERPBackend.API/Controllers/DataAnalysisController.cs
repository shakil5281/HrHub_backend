using Microsoft.AspNetCore.Mvc;
using ERPBackend.Core.Interfaces;
using ERPBackend.Core.DTOs;

namespace ERPBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataAnalysisController : ControllerBase
{
    private readonly IDataAnalysisService _analysisService;
    private readonly ILogger<DataAnalysisController> _logger;

    public DataAnalysisController(
        IDataAnalysisService analysisService,
        ILogger<DataAnalysisController> logger)
    {
        _analysisService = analysisService;
        _logger = logger;
    }

    [HttpGet("statistics/{entityType}")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> GetStatistics(string entityType)
    {
        try
        {
            var stats = await _analysisService.GetEntityStatisticsAsync(entityType);
            return Ok(ApiResponse<Dictionary<string, object>>.SuccessResponse(stats, "Statistics retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics for {EntityType}", entityType);
            return StatusCode(500, ApiResponse<Dictionary<string, object>>.ErrorResponse("Failed to retrieve statistics"));
        }
    }

    [HttpPost("aggregate")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> GetAggregateData([FromBody] DataAnalysisRequest request)
    {
        try
        {
            var result = await _analysisService.GetAggregateDataAsync(
                request.EntityType,
                request.GroupByFields ?? Array.Empty<string>(),
                request.AggregateFields ?? Array.Empty<string>()
            );

            return Ok(ApiResponse<Dictionary<string, object>>.SuccessResponse(result, "Aggregation completed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing aggregation");
            return StatusCode(500, ApiResponse<Dictionary<string, object>>.ErrorResponse("Aggregation failed"));
        }
    }
}
