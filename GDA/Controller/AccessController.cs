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
            (int code, bool state, string message) = await _userService.UserRegister(user);
            return StatusCode(code, new { isSuccess = state, message = message });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            (int code, bool state, int id, string role, string token) = await _userService.Login(login);
            return StatusCode(code, new { isSuccess = state, id = id, role = role, token = token });
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpGet]
        [Route("UsersSearch")]
        public async Task<IEnumerable<UserViewDTO>> UsersSearch()
        {
            return await _userService.UsersSearch();
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpGet]
        [Route("UserSearch")]
        public async Task<UserViewDTO> UserSearch([FromQuery] int id)
        {
            return await _userService.UserSearch(id);
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpPut]
        [Route("RoleUpdate")]
        public async Task<IActionResult> RoleUpdate([FromBody] RoleUpdateDTO role)
        {
            (int code, bool state, string message) = await _userService.RoleUpdate(role);
            return StatusCode(code, new { isSuccess = state, message = message });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AccountRecovery")]
        public async Task<IActionResult> AccountRecovery([FromBody] DataRecoveryDTO data)
        {
            (int code, bool state, int id, string token) = await _userService.AccountRecovery(data);
            return StatusCode(code, new { isSuccess = state, id = id, message = token });
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
            (int code, bool state, string message) = await _userService.PasswordRecovery(passwordData);
            return StatusCode(code, new { isSuccess = state, message = message });
        }

        [Authorize(AuthenticationSchemes = "AccessScheme")]
        [HttpPut]
        [Route("PasswordUpdate")]
        public async Task<IActionResult> PasswordUpdate([FromBody] PasswordUpdateDTO password)
        {
            (int code, bool state, string message) = await _userService.PasswordUpdate(password);
            return StatusCode(code, new { isSuccess = state, message = message });
        }
    }
}
