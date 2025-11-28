using DataAccess.Generic;
using DataAccess.SupportServices;
using Entities.DataContext;
using Entities.Domain;
using Entities.Domain.DTO;
using Entities.Domain.DTO.Response;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Utilities;

namespace DataAccess
{
    public interface IUserService
    {
        Task<ResultHelper<UserViewDTO>> UserRegister(UserDTO userDTO);
        Task<ResultHelper<LoginResponseDTO>> Login(LoginDTO loginDTO);
        Task<ResultHelper<IEnumerable<UserViewDTO>>> UsersSearch();
        Task<ResultHelper<UserViewDTO>> UserSearch(int id);
        Task<ResultHelper<UserViewDTO>> RoleUpdate(RoleUpdateDTO roleUpdate);
        Task<ResultHelper<DataRecoveryResponseDTO>> AccountRecovery(DataRecoveryDTO dataRecovery);
        Task<ResultHelper<object>> PasswordRecovery(PasswordRecoveryDTO passwordRecovery);
        Task<ResultHelper<object>> PasswordUpdate(PasswordUpdateDTO passwordUpdate);
        Task<ResultHelper<object>> UserDelete(int id);
        Task<ResultHelper<IEnumerable<RoleViewDTO>>> RolesSearch();

    }
    public class UserService : IUserService
    {
        private readonly ISqlGenericRepository<User, ServiceDbContext> _userSqlGenericRepository;
        private readonly ISqlGenericRepository<Role, ServiceDbContext> _roleSqlGenericRepository;
        private readonly IAuthenticationService _authentication;
        private readonly IMailService _mailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlEncoderHelper _urlEncoderHelper;

        public UserService(
            ISqlGenericRepository<User, ServiceDbContext> userSqlGenericRepository, 
            ISqlGenericRepository<Role, ServiceDbContext> roleSqlGenericRepository, 
            IAuthenticationService authentication,
            IMailService mailService,
            IHttpContextAccessor httpContextAccessor,
            IUrlEncoderHelper urlEncoderHelper)
        {
            _userSqlGenericRepository = userSqlGenericRepository;
            _roleSqlGenericRepository = roleSqlGenericRepository;
            _authentication = authentication;
            _mailService = mailService;
            _httpContextAccessor = httpContextAccessor;
            _urlEncoderHelper = urlEncoderHelper;
        }

        public async Task<ResultHelper<UserViewDTO>> UserRegister(UserDTO userDTO)
        {
            try
            {
                bool estado = false;
                User? userFound = (await _userSqlGenericRepository.GetAsync(a => a.NationalId == userDTO.NationalId || a.Email == userDTO.Email)).SingleOrDefault();
                if (userFound != null)
                {
                    return ResultHelper<UserViewDTO>.Fail(409, Activator.CreateInstance<UserViewDTO>(), "Usuario ya existe.");

                }
                User userModel = new User
                {
                    Name = userDTO.Name,
                    Lastname = userDTO.Lastname,
                    NationalId = userDTO.NationalId,
                    Email = userDTO.Email,
                    PhoneNumber = userDTO.PhoneNumber,
                    RoleId = userDTO.RoleId,
                    Password = _authentication.EncryptationSHA256(userDTO.NationalId),
                    RegisteredDate = DateTime.Now
                };
                int? id = await _userSqlGenericRepository.CreateAsync(userModel);
                estado = true;
                Role? roleFound = (await _roleSqlGenericRepository.GetAsync(a => a.Id == userDTO.RoleId)).SingleOrDefault();
                UserViewDTO userView = new UserViewDTO
                {
                    Id = id.Value,
                    Name = userModel.Name,
                    Lastname = userModel.Lastname,
                    NationalId = userModel.NationalId,
                    Email = userModel.Email,
                    PhoneNumber = userModel.PhoneNumber,
                    Role = roleFound.Name,
                    RegisteredDate = userModel.RegisteredDate
                };
                if (id != null && estado == true)
                {
                    try
                    {
                        string tokenWelcome = await _authentication.GenerateSecureRandomToken(userModel, new TimeSpan(0, 0, 30, 0));
                        string frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? Environment.GetEnvironmentVariable("URL_DOMAIN") ?? "";
                        
                        WelcomeEmailDTO welcomeData = new WelcomeEmailDTO
                        {
                            Email = userModel.Email,
                            UserName = $"{userModel.Name} {userModel.Lastname}",
                            Role = roleFound.Name,
                            Url = frontendUrl,
                            Token = tokenWelcome
                        };
                        
                        await _mailService.SendWelcomeEmailAsync(welcomeData);
                    }
                    catch (Exception ex)
                    {
                        
                    }
                    
                    return ResultHelper<UserViewDTO>.Ok(201, userView, "Usuario creado y correo de bienvenida enviado correctamente.");

                }
                else
                {
                    return ResultHelper<UserViewDTO>.Fail(500, Activator.CreateInstance<UserViewDTO>(), "Error al registrar el usuario.");

                }
            }
            catch (Exception ex)
            {
                return ResultHelper<UserViewDTO>.Fail(500, Activator.CreateInstance<UserViewDTO>(), ex.Message);
                
            }
        }

        public async Task<ResultHelper<LoginResponseDTO>> Login(LoginDTO login)
        {
            try
            {
                User? userFound = (await _userSqlGenericRepository.GetAsync(a => a.Email == login.Email && a.Password == _authentication.EncryptationSHA256(login.Password), u => u.Role)).SingleOrDefault();
                if (userFound == null)
                {
                    return ResultHelper<LoginResponseDTO>.Fail(404, Activator.CreateInstance<LoginResponseDTO>(), "Usuario no encontrado.");
                    
                }

                LoginResponseDTO loginResponseDTO = new LoginResponseDTO()
                {
                    Id = userFound.Id,
                    Name = userFound.Name,
                    Lastname = userFound.Lastname,
                    Email = userFound.Email,
                    NationalId = userFound.NationalId,
                    PhoneNumber = userFound.PhoneNumber,
                    Role = userFound.Role.Name,
                    Token = _authentication.GenerateAccessJwt(userFound)
                };

                return ResultHelper<LoginResponseDTO>.Ok(200, loginResponseDTO);
            }
            catch (Exception ex)
            {
                return ResultHelper<LoginResponseDTO>.Fail(500, Activator.CreateInstance<LoginResponseDTO>(), "Error interno del servidor, vuelva a inentarlo. " + ex.Message);

            }
        }

        public async Task<ResultHelper<IEnumerable<UserViewDTO>>> UsersSearch()
        {
            try
            {
                IEnumerable<User> users = await _userSqlGenericRepository.GetAsync(includes: u => u.Role);
                List<UserViewDTO> usersDTO = new List<UserViewDTO>();
                foreach (User user in users)
                {
                    UserViewDTO userDTO = new UserViewDTO
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Lastname = user.Lastname,
                        Email = user.Email,
                        NationalId = user.NationalId,
                        PhoneNumber = user.PhoneNumber,
                        Role = user.Role.Name,
                        RegisteredDate = user.RegisteredDate.ToLocalTime()
                    };
                    usersDTO.Add(userDTO);
                }
                return ResultHelper<IEnumerable<UserViewDTO>>.Ok(200, usersDTO);
            }
            catch (Exception ex)
            {
                return ResultHelper<IEnumerable<UserViewDTO>>.Fail(
                    500,
                    Activator.CreateInstance<IEnumerable<UserViewDTO>>(),
                    "Error interno del servidor, vuelva a intentarlo. " + ex.Message);
            }
        }

        public async Task<ResultHelper<UserViewDTO>> UserSearch(int id)
        {
            User? user = (await _userSqlGenericRepository.GetAsync(a => a.Id == id, u => u.Role)).FirstOrDefault();

            if (user == null)
            {
                return ResultHelper<UserViewDTO>.Ok(404, Activator.CreateInstance<UserViewDTO>(), "Usuario no encontrado.");
            }

            UserViewDTO userView = new UserViewDTO
            {
                Id = user.Id,
                Name = user.Name,
                Lastname = user.Lastname,
                Email = user.Email,
                NationalId = user.NationalId,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.Name,
                RegisteredDate = user.RegisteredDate
            };
            return ResultHelper<UserViewDTO>.Ok(200, userView);
        }

        public async Task<ResultHelper<UserViewDTO>> RoleUpdate(RoleUpdateDTO roleUpdate)
        {
            try
            {
                User? userModel = (await _userSqlGenericRepository.GetAsync(a => a.Id == roleUpdate.Id)).FirstOrDefault();
                userModel.RoleId = roleUpdate.RoleId;
                bool state = await _userSqlGenericRepository.UpdateByEntityAsync(userModel);
                if (state)
                {
                    return ResultHelper<UserViewDTO>.Ok(200, Activator.CreateInstance<UserViewDTO>(), "Rol actualizado.");
                }
                else
                {
                    return ResultHelper<UserViewDTO>.Fail(404, Activator.CreateInstance<UserViewDTO>(), "El usuario no existe.");
                    
                }
            }
            catch (Exception ex)
            {
                return ResultHelper<UserViewDTO>.Fail(500, Activator.CreateInstance<UserViewDTO>(), "Error interno del servidor, vuleva a intentarlo." + ex.Message);
            }
        }

        public async Task<ResultHelper<DataRecoveryResponseDTO>> AccountRecovery(DataRecoveryDTO dataRecovery)
        {
            try
            {
                User? userFound = await _userSqlGenericRepository.GetByIdAsync(a => a.Email == dataRecovery.Email);
                if (userFound == null)
                {
                    return ResultHelper<DataRecoveryResponseDTO>.Fail(404, Activator.CreateInstance<DataRecoveryResponseDTO>(), "El usuario no se encuentra registrado.");
                }
                
                string tokenRecovery = await _authentication.GenerateSecureRandomToken(userFound, new TimeSpan(0, 0, 30, 0)); 

                await _mailService.SendRecoveryEmailAsync(dataRecovery, tokenRecovery);
                
                DataRecoveryResponseDTO responseDTO = new DataRecoveryResponseDTO
                {
                    Token = tokenRecovery,
                    Url = $"{dataRecovery.Url}/{_urlEncoderHelper.Encode(tokenRecovery)}"
                };

                return ResultHelper<DataRecoveryResponseDTO>.Ok(200, responseDTO, "Cuenta recuperada. Se ha enviado un email con las instrucciones.");

            }
            catch (Exception ex)
            {
                return ResultHelper<DataRecoveryResponseDTO>.Fail(500, Activator.CreateInstance<DataRecoveryResponseDTO>(),
                    "Error interno del servidor, vuelva a intentarlo." + ex.Message);
            }
        }

        public async Task<ResultHelper<object>> PasswordRecovery(PasswordRecoveryDTO passwordRecovery)
        {
            try
            {
                ClaimsPrincipal userClaims = _httpContextAccessor.HttpContext.User;
                string userId = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                User? user = await _userSqlGenericRepository.GetByIdAsync(a => a.Id == Convert.ToInt32(userId));
                string newPassword = _authentication.EncryptationSHA256(passwordRecovery.NewPassword);
                if (newPassword == user.Password)
                {
                    return ResultHelper<object>.Fail(200, Activator.CreateInstance<object>(), "No puede utilizar la misma contraseña.");
                }
                user.Password = newPassword;
                bool state = await _userSqlGenericRepository.UpdateByEntityAsync(user);
                if (state)
                {
                    return ResultHelper<object>.Ok(200, Activator.CreateInstance<object>(), "La contraseña se a cambiado correctamente.");
                }
                else
                {
                    return ResultHelper<object>.Fail(200, Activator.CreateInstance<object>(), "El usuario no existe.");
                }
            }
            catch (Exception ex)
            {
                return ResultHelper<object>.Fail(200, Activator.CreateInstance<object>(), "Error interno del servidor, vuelva a intentarlo." + ex.Message);
            }
        }

        public async Task<ResultHelper<object>> PasswordUpdate(PasswordUpdateDTO passwordUpdate)
        {
            try
            {
                string currentPassword = _authentication.EncryptationSHA256(passwordUpdate.CurrentPassword);
                string newPassword = _authentication.EncryptationSHA256(passwordUpdate.NewPassword);
                User? user = (await _userSqlGenericRepository.GetAsync(a => a.Id == passwordUpdate.Id)).FirstOrDefault();

                if (currentPassword != user.Password)
                {
                    return ResultHelper<object>.Fail(401, Activator.CreateInstance<object>(), "La contraseña no coincide.");
                }
                if (newPassword == currentPassword)
                {
                    return ResultHelper<object>.Fail(409, Activator.CreateInstance<object>(), "La nueva contraseña es igual a la contraseña actual.");
                }
                user.Password = newPassword;
                bool state = await _userSqlGenericRepository.UpdateByEntityAsync(user);
                if (state)
                {
                    return ResultHelper<object>.Ok(200, Activator.CreateInstance<object>(), "La contraseña se a cambiado correctamente.");
                }
                else
                {
                    return ResultHelper<object>.Fail(404, Activator.CreateInstance<object>(), "El usuario no existe.");
                }
            }
            catch (Exception ex)
            {
                return ResultHelper<object>.Fail(500, Activator.CreateInstance<object>(), "Error interno del servidor, vuelva a intentarlo." + ex.Message);
            }
        }

        public async Task<ResultHelper<object>> UserDelete(int id)
        {
            try
            {
                User? user = (await _userSqlGenericRepository.GetAsync(a => a.Id == id)).FirstOrDefault();
                if (user == null)
                {
                    return ResultHelper<object>.Fail(404, Activator.CreateInstance<object>(), "No se encontro el usuario.");
                }
                bool state = await _userSqlGenericRepository.DeleteByIdAsync(user.Id);
                if (state == true)
                {
                    return ResultHelper<object>.Ok(200, Activator.CreateInstance<object>(), "Usuario borrado correctamente.");
                }
                else
                {
                    return ResultHelper<object>.Ok(404, Activator.CreateInstance<object>(), "Error al eliminar usuario.");
                }
            }
            catch (Exception ex)
            {
                return ResultHelper<object>.Fail(500, Activator.CreateInstance<object>(), "Error interno del servidor, vuelva a intentarlo." + ex.Message);

            }
        }

        public async Task<ResultHelper<IEnumerable<RoleViewDTO>>> RolesSearch()
        {
            try
            {
                IEnumerable<Role> roles = await _roleSqlGenericRepository.GetAsync();
                List<RoleViewDTO> rolesDTO = new List<RoleViewDTO>();
                foreach (Role role in roles)
                {
                    RoleViewDTO roleDTO = new RoleViewDTO
                    {
                        Id = role.Id,
                        Name = role.Name
                    };
                    rolesDTO.Add(roleDTO);
                }
                return ResultHelper<IEnumerable<RoleViewDTO>>.Ok(200, rolesDTO);
            }
            catch (Exception ex)
            {
                return ResultHelper<IEnumerable<RoleViewDTO>>.Fail(500, Activator.CreateInstance<IEnumerable<RoleViewDTO>>(), "Error interno del servidor, vuelva a intentarlo. " + ex.Message);
            }
        }
    }
}
