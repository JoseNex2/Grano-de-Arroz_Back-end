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

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpPost]
        [Route("registryclient")]
        public async Task<IActionResult> Registry([FromBody] ClientDTO client)
        {
            var result = await _clientService.ClientRegister(client);
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpGet]
        [Route("clientssearch")]
        public async Task<IActionResult> ClientsSearch()
        {
            var result = await _clientService.ClientsSearch();
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpGet]
        [Route("clientsearch")]
        public async Task<IActionResult> ClientSearch([FromQuery] int id)
        {
            var result = await _clientService.ClientSearch(id);
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpPut]
        [Route("clientupdate")]
        public async Task<IActionResult> ClientUpdate([FromBody] ClientUpdateDTO clientUpdateDTO)
        {
            var result = await _clientService.ClientUpdate(clientUpdateDTO);
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpPut]
        [Route("clientdelete")]
        public async Task<IActionResult> ClientDelete([FromQuery] int id)
        {
            var result = await _clientService.ClientDelete(id);
            return StatusCode(result.Code, result);
        }
    }
}
