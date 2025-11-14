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
        private readonly ILogger<BatteryController> _logger;

        public BatteryController(IBatteryService batteryService, ILogger<BatteryController> logger)
        {
            _batteryService = batteryService;
            _logger = logger;
        }

        [Authorize(AuthenticationSchemes = "AccessScheme", Roles = "Sucursal")]
        [HttpPost]
        [Route("registrybattery")]
        public async Task<IActionResult> Batteryregister([FromBody] BatteryDTO battery)
        {
            var result = await _batteryService.BatteryRegister(battery);
            _logger.LogInformation("Se asocio una bateria a un cliente.");
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpGet]
        [Route("batteriessearch")]
        public async Task<IActionResult> BatteriesSearch()
        {
            var result = await _batteryService.BatteriesSearch();
            _logger.LogInformation("Se buscaron todas las baterias.");
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpPost]
        [Route("batteriessearchwithfilter")]
        public async Task<IActionResult> BatteriesSearchWithFilter(BatterySearchFilterDTO filter)
        {
            var result = await _batteryService.BatteriesSearchWithFilter(filter);
            _logger.LogInformation("Se buscaron ciertas baterias.");
            return StatusCode(result.Code, result);
        }


        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpGet]
        [Route("batterysearchwithid")]
        public async Task<IActionResult> BatterySearchWithId(int id)
        {
            var result = await _batteryService.BatterySearchWithId(id);
            _logger.LogInformation("Se busco una bateria por Id.");
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpGet]
        [Route("batterysearchbyclientid")]
        public async Task<IActionResult> BatterySearchByClientId(int ClientId)
        {
            var result = await _batteryService.BatteriesSearchByClient(ClientId);
            _logger.LogInformation("Se busco una bateria por ClienteId.");
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpGet]
        [Route("getbatteryanalysispercentageasync")]
        public async Task<IActionResult> GetBatteryAnalysisPercentageAsync()
        {
            var result = await _batteryService.GetBatteryAnalysisPercentageAsync();
            _logger.LogInformation("Porcentaje de baterias analizadas");
            return StatusCode(result.Code, result);
        }




        /*[AllowAnonymous]
        [HttpPost]
        [Route("uploadrawdata")]
        public async Task<IActionResult> UploadRawData([FromForm] RawDataDTO rawDataDTO)
        {
            var result = await _batteryService.UploadRawData(rawDataDTO);
            _logger.LogInformation("Se subieron nuevas mediciones.");
            return StatusCode(result.Code, result);
        }*/
    }
}
