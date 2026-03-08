using ERPBackend.Core.Interfaces;
using ERPBackend.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    }
}
