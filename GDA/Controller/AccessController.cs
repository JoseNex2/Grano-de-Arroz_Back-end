using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Entities.Domain.DTO;
using DataAccess;

namespace GDA.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessController : ControllerBase
    {
        private readonly IUserService _userService;

        public AccessController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpPost]
        [Route("registry")]
        public async Task<IActionResult> Registry([FromBody] UserDTO user)
        {
            var result = await _userService.UserRegister(user);
            return StatusCode(result.Code, result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var result = await _userService.Login(login);
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpGet]
        [Route("UsersSearch")]
        public async Task<IActionResult> UsersSearch()
        {
            var result = await _userService.UsersSearch();
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpGet]
        [Route("UserSearch")]
        public async Task<IActionResult> UserSearch([FromQuery] int id)
        {   
            var result = await _userService.UserSearch(id);
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpPut]
        [Route("RoleUpdate")]
        public async Task<IActionResult> RoleUpdate([FromBody] RoleUpdateDTO role)
        {
            var result = await _userService.RoleUpdate(role);
            return StatusCode(result.Code, result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AccountRecovery")]
        public async Task<IActionResult> AccountRecovery([FromBody] DataRecoveryDTO data)
        {
            var result = await _userService.AccountRecovery(data);
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "RecoveryScheme")]
        [HttpGet]
        [Route("AccountRecoveryState")]
        public IActionResult AccountRecoveryState()
        {
            return StatusCode(StatusCodes.Status200OK, new { isSuccess = true });
        }

        [Authorize(AuthenticationSchemes = "RecoveryScheme")]
        [HttpPut]
        [Route("PasswordRecovery")]
        public async Task<IActionResult> PasswordRecovery([FromBody] PasswordRecoveryDTO passwordData)
        {
            var result = await _userService.PasswordRecovery(passwordData);
            return StatusCode(result.Code, result);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpPut]
        [Route("PasswordUpdate")]
        public async Task<IActionResult> PasswordUpdate([FromBody] PasswordUpdateDTO password)
        {
            var result = await _userService.PasswordUpdate(password);
            return StatusCode(result.Code, result);
        }
    }
}
