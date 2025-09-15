using DataAccess;
using Entities.Domain.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GDA.Controller
{
    [Route("api/battery")]
    [ApiController]
    public class BatteryController : ControllerBase
    {
        private readonly IBatteryService _batteryService;

        public BatteryController(IBatteryService batteryService)
        {
            _batteryService = batteryService;
        }

        [Authorize(AuthenticationSchemes = "AccessScheme", Roles = "Sucursal")]
        [HttpPost]
        [Route("registrybattery")]
        public async Task<IActionResult> Batteryregister([FromBody] BatteryDTO battery)
        {
            var result = await _batteryService.BatteryRegister(battery);
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpGet]
        [Route("batteriessearch")]
        public async Task<IActionResult> BatteriesSearch()
        {
            var result = await _batteryService.BatteriesSearch();
            return StatusCode(result.Code, result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("uploadrawdata")]
        public async Task<IActionResult> UploadRawData([FromForm] RawDataDTO rawDataDTO)
        {
            var result = await _batteryService.UploadRawData(rawDataDTO);
            return StatusCode(result.Code, result);
        }
    }
}
