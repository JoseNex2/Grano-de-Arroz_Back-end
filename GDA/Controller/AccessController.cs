using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Entities.Domain.DTO;
using DataAccess;

namespace GDA.Controller
{
    [Route("api/access")]
    [ApiController]
    public class AccessController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AccessController> _logger;

        public AccessController(IUserService userService, ILogger<AccessController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [Authorize(AuthenticationSchemes = "AccessScheme", Roles = "Administrador")]
        [HttpPost]
        [Route("registry")]
        public async Task<IActionResult> Registry([FromBody] UserDTO user)
        {
            var result = await _userService.UserRegister(user);
            _logger.LogInformation("Se registro nuevo usuario.");
            return StatusCode(result.Code, result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var result = await _userService.Login(login);
            _logger.LogInformation("Usuario logueado.");
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme", Roles = "Administrador")]
        [HttpGet]
        [Route("userssearch")]
        public async Task<IActionResult> UsersSearch()
        {
            var result = await _userService.UsersSearch();
            _logger.LogInformation("Se buscaron todos los usuarios.");
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme", Roles = "Administrador")]
        [HttpGet]
        [Route("usersearch")]
        public async Task<IActionResult> UserSearch([FromQuery] int id)
        {   
            var result = await _userService.UserSearch(id);
            _logger.LogInformation($"Se busco un usuario específico: {id}");
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme", Roles = "Administrador")]
        [HttpPut]
        [Route("roleupdate")]
        public async Task<IActionResult> RoleUpdate([FromBody] RoleUpdateDTO role)
        {
            var result = await _userService.RoleUpdate(role);
            _logger.LogInformation($"Se actualizo el rol del usuario con id: {role.Id.ToString()}.");
            return StatusCode(result.Code, result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("accountrecovery")]
        public async Task<IActionResult> AccountRecovery([FromBody] DataRecoveryDTO data)
        {
            var result = await _userService.AccountRecovery(data);
            _logger.LogInformation("Inicio de recuperación de cuenta.");
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "ExternalScheme")]
        [HttpPut]
        [Route("passwordrecovery")]
        public async Task<IActionResult> PasswordRecovery([FromBody] PasswordRecoveryDTO passwordData)
        {
            var result = await _userService.PasswordRecovery(passwordData);
            _logger.LogInformation("Se actualizo la contraseña de la cuenta recuperada.");
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpPut]
        [Route("passwordupdate")]
        public async Task<IActionResult> PasswordUpdate([FromBody] PasswordUpdateDTO password)
        {
            var result = await _userService.PasswordUpdate(password);
            _logger.LogInformation($"Se actualizo la contraseña del usuario con id: {password.Id.ToString()}.");
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme", Roles = "Administrador")]
        [HttpPut]
        [Route("userdelete")]
        public async Task<IActionResult> UserDelete([FromQuery] int id)
        {
            _logger.LogInformation($"Se elimino el usuario con id: {id.ToString()}.");
            var result = await _userService.UserDelete(id);
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme", Roles = "Administrador")]
        [HttpGet]
        [Route("rolessearch")]
        public async Task<IActionResult> RolesSearch()
        {
            var result = await _userService.RolesSearch();
            _logger.LogInformation("Se buscaron todos los roles.");
            return StatusCode(result.Code, result);
        }
    }
}
