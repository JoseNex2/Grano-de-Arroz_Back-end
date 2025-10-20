using DataAccess;
using Entities.Domain;
using Entities.Domain.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utilities;

namespace GDA.Controller
{
    [Route("api/report")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<BatteryController> _logger;

        public ReportController(IReportService reportService, ILogger<BatteryController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [Authorize(AuthenticationSchemes = "AccessScheme", Roles = "Sucursal")]
        [HttpPost]
        [Route("reportcreate")]
        public async Task<IActionResult> ReportCreate([FromBody] BatteryReviewRequest reportRequest)
        {
            var result = await _reportService.ReportCreate(reportRequest);
            _logger.LogInformation("Se creo un reporte.");
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme", Roles = "Administrador, Sucursal, Laboratorio")]
        [HttpPost]
        [Route("reportssearch")]
        public async Task<IActionResult> ReportsSearchAsync([FromBody] ReportSearchFilter filter)
        {
            var result = await _reportService.ReportsSearchAsync(filter);
            _logger.LogInformation("Se busco un reporte.");
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme", Roles = "Laboratorio")]
        [HttpPut]
        [Route("updatemeasurementreport")]
        public async Task<IActionResult> UpdateMeasurementReportAsync([FromBody] ReportUpdateDTO update)
        {
            var result = await _reportService.UpdateMeasurementReportAsync(update);
            _logger.LogInformation("Se actualizo un reporte.");
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme", Roles = "Administrador, Sucursal, Laboratorio")]
        [HttpGet]
        [Route("reportgetbyid")]
        public async Task<IActionResult> ReportGetByIdAsync([FromQuery] int reportId)
        {
            var result = await _reportService.ReportGetByIdAsync(reportId);
            _logger.LogInformation("Se busco un reporte por id.");
            return StatusCode(result.Code, result);
        }
    }
}
