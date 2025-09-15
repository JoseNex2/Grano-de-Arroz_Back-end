using DataAccess;
using Entities.Domain.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GDA.Controller
{
    [Route("api/clients")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IClientService clientService, ILogger<ClientController> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpPost]
        [Route("registryclient")]
        public async Task<IActionResult> Registry([FromBody] ClientDTO client)
        {
            var result = await _clientService.ClientRegister(client);
            _logger.LogInformation("Se registro nuevo cliente.");
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpGet]
        [Route("clientssearch")]
        public async Task<IActionResult> ClientsSearch()
        {
            var result = await _clientService.ClientsSearch();
            _logger.LogInformation("Se buscaron todos los clientes.");
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpGet]
        [Route("clientsearch")]
        public async Task<IActionResult> ClientSearch([FromQuery] int id)
        {
            var result = await _clientService.ClientSearch(id);
            _logger.LogInformation($"Se busco un cliente por id: {id.ToString()}.");
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpPut]
        [Route("clientupdate")]
        public async Task<IActionResult> ClientUpdate([FromBody] ClientUpdateDTO clientUpdateDTO)
        {
            var result = await _clientService.ClientUpdate(clientUpdateDTO);
            _logger.LogInformation($"Se actualizo el cliente con id: {clientUpdateDTO.Id.ToString()}.");
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpPut]
        [Route("clientdelete")]
        public async Task<IActionResult> ClientDelete([FromQuery] int id)
        {
            var result = await _clientService.ClientDelete(id);
            _logger.LogInformation($"Se elimino el cliente con id: {id.ToString()}.");
            return StatusCode(result.Code, result);
        }
    }
}
