using Microsoft.AspNetCore.Mvc;

namespace ERPBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WelcomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "Welcome to the fresh HR Hub Backend API!" });
    }
}
