using DataAccess;
using Entities.Domain.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GDA.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BatteryController : ControllerBase
    {
        private readonly IBatteryService _batteryService;

        public BatteryController(IBatteryService batteryService)
        {
            _batteryService = batteryService;
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpPost]
        [Route("registryBattery")]
        public async Task<IActionResult> Batteryregister([FromBody] BatteryDTO battery)
        {
            var result = await _batteryService.BatteryRegister(battery);
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpPut]
        [Route("BatteriesSearch")]
        public async Task<IActionResult> BatteriesSearch()
        {
            var result = await _batteryService.BatteriesSearch();
            return StatusCode(result.Code, result);
        }
    }
}
