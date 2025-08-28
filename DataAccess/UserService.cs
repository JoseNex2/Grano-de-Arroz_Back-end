using DataAccess.Generic;
using Utilities;
using Entities.Domain;
using Entities.Domain.DTO;
using Entities.DataContext;
using MimeKit;
using MailKit.Security;
using MailKit.Net.Smtp;

namespace DataAccess
{
    public interface IUserService
    {
        Task<Result<UserViewDTO>> UserRegister(UserDTO userDTO);
        Task<Result<LoginResponseDTO>> Login(LoginDTO loginDTO);
        Task<Result<IEnumerable<UserViewDTO>>> UsersSearch();
        Task<Result<UserViewDTO>> UserSearch(int id);
        Task<Result<UserViewDTO>> RoleUpdate(RoleUpdateDTO roleUpdate);
        Task<Result<DataRecoberyResponseDTO>> AccountRecovery(DataRecoveryDTO dataRecovery);
        Task<Result<object>> PasswordRecovery(PasswordRecoveryDTO passwordRecovery);
        Task<Result<object>> PasswordUpdate(PasswordUpdateDTO passwordUpdate);
        Task<Result<object>> UserDelete(int id);

    }
    public class UserService : IUserService
    {
        private readonly ISqlGenericRepository<User, ServiceDbContext> _sqlGenericRepository;
        private readonly Authentication _authentication;
        public UserService(ISqlGenericRepository<User, ServiceDbContext> sqlGenericRepository, Authentication authentication)
        {
            _sqlGenericRepository = sqlGenericRepository;
            _authentication = authentication;
        }

        public async Task<Result<UserViewDTO>> UserRegister(UserDTO user)
        {
            try
            {
                bool estado = false;
                User? usuarioEncontrado = (await _sqlGenericRepository.GetAsync(a => a.NationalId == user.NationalId || a.Email == user.Email)).SingleOrDefault();
                if (usuarioEncontrado == null)
                {
                    User userModel = new User
                    {
                        Name = user.Name,
                        Lastname =user.Lastname,
                        NationalId = user.NationalId,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Role = user.Role,
                        Password = _authentication.EncryptationSHA256(user.Password),
                        DateRegistered = DateTime.Now
                    };
                    int? id = await _sqlGenericRepository.CreateAsync(userModel);
                    estado = true;

                    UserViewDTO userView = new UserViewDTO
                    {
                        Id = id.Value,
                        Name = userModel.Name,
                        Lastname = userModel.Lastname,
                        NationalId = userModel.NationalId,
                        Email = userModel.Email,
                        PhoneNumber = userModel.PhoneNumber,
                        Role = userModel.Role,
                        DateRegistered = userModel.DateRegistered
                    };
                    if (id != null && estado == true)
                    {
                        return Result<UserViewDTO>.Ok(201, userView, "Usuario creado.");

                    }
                    else
                    {
                        return Result<UserViewDTO>.Fail(500, Activator.CreateInstance<UserViewDTO>(),"Error al registrar el usuario.");

                    }
                }
                else
                {
                    return Result<UserViewDTO>.Fail(409, Activator.CreateInstance<UserViewDTO>(), "Usuario ya existe.");
                    
                }
            }
            catch (Exception ex)
            {
                return Result<UserViewDTO>.Fail(500, Activator.CreateInstance<UserViewDTO>(), ex.Message);
                
            }
        }

        public async Task<Result<LoginResponseDTO>> Login(LoginDTO login)
        {
            try
            {
                User? userFound = (await _sqlGenericRepository.GetAsync(a => a.Email == login.Email && a.Password == _authentication.EncryptationSHA256(login.Password))).SingleOrDefault();
                if (userFound == null)
                {
                    return Result<LoginResponseDTO>.Fail(404, Activator.CreateInstance<LoginResponseDTO>(), "Usuario no encontrado.");
                    
                }
                else
                {
                    LoginResponseDTO loginResponseDTO = new LoginResponseDTO()
                    {
                        Email = userFound.Email,
                        Role = userFound.Role,
                        Token = _authentication.GenerateAccessJwt(userFound)
                    };

                    return Result<LoginResponseDTO>.Ok(200, loginResponseDTO);

                }
            }
            catch (Exception ex)
            {
                return Result<LoginResponseDTO>.Fail(500, Activator.CreateInstance<LoginResponseDTO>(), "Error interno del servidor, vuelva a inentarlo. " + ex.Message);

            }
        }

        public async Task<Result<IEnumerable<UserViewDTO>>> UsersSearch()
        {
            try
            {
                IEnumerable<User> users = await _sqlGenericRepository.GetAllAsync();
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
                        Role = user.Role,
                        DateRegistered = user.DateRegistered.ToLocalTime()
                    };
                    usersDTO.Add(userDTO);
                }
                return Result<IEnumerable<UserViewDTO>>.Ok(200, usersDTO);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<UserViewDTO>>.Fail(
                    500,
                    Activator.CreateInstance<IEnumerable<UserViewDTO>>(),
                    "Error interno del servidor, vuelva a intentarlo. " + ex.Message);
            }
        }

        public async Task<Result<UserViewDTO>> UserSearch(int id)
        {
            User? user = (await _sqlGenericRepository.GetAsync(a => a.Id == id)).FirstOrDefault();

            if (user == null)
            {
                return Result<UserViewDTO>.Ok(404, Activator.CreateInstance<UserViewDTO>(), "Usuario no encontrado.");
            }

            UserViewDTO userView = new UserViewDTO
            {
                Id = user.Id,
                Name = user.Name,
                Lastname = user.Lastname,
                Email = user.Email,
                NationalId = user.NationalId,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                DateRegistered = user.DateRegistered
            };
            return Result<UserViewDTO>.Ok(200, userView);
        }

        public async Task<Result<UserViewDTO>> RoleUpdate(RoleUpdateDTO roleUpdate)
        {
            try
            {
                User userModel = await _sqlGenericRepository.GetByIdAsync(roleUpdate.Id);
                userModel.Role = roleUpdate.Role;
                bool state = await _sqlGenericRepository.UpdateByEntityAsync(userModel);
                if (state)
                {
                    return Result<UserViewDTO>.Ok(200, Activator.CreateInstance<UserViewDTO>(), "Rol actualizado.");
                }
                else
                {
                    return Result<UserViewDTO>.Fail(404, Activator.CreateInstance<UserViewDTO>(), "El usuario no existe.");
                    
                }
            }
            catch (Exception ex)
            {
                return Result<UserViewDTO>.Fail(500, Activator.CreateInstance<UserViewDTO>(), "Error interno del servidor, vuleva a intentarlo." + ex.Message);

            }
        }

        public async Task<Result<DataRecoberyResponseDTO>> AccountRecovery(DataRecoveryDTO dataRecovery)
        {
            try
            {
                User? userFound = (await _sqlGenericRepository.GetAsync(a => a.Email == dataRecovery.Email)).SingleOrDefault();
                if (userFound != null)
                {
                    string tokenRecovery = _authentication.GenerateRecoveryJwt(userFound);
                    MimeMessage emailMessage = new MimeMessage();

                    emailMessage.From.Add(new MailboxAddress("Sistema de recuperación de contraseña", Environment.GetEnvironmentVariable("MAIL_RECOVERY")));
                    emailMessage.To.Add(new MailboxAddress("", dataRecovery.Email));
                    emailMessage.Subject = "Recuperar contraseña";
                    emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                    {
                        Text = dataRecovery.Url + "/" + tokenRecovery
                    };
                    using (SmtpClient client = new SmtpClient())
                    {
                        await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                        await client.AuthenticateAsync(Environment.GetEnvironmentVariable("MAIL_RECOVERY"), "evny knhp vhzc mtqa");
                        await client.SendAsync(emailMessage);

                        await client.DisconnectAsync(true);
                    }
                    DataRecoberyResponseDTO responseDTO = new DataRecoberyResponseDTO
                    {
                        Id = userFound.Id,
                        Token = tokenRecovery,
                    };

                    return Result<DataRecoberyResponseDTO>.Ok(200, responseDTO, "Cuenta recuperada.") ;
                }
                else
                {
                    return Result<DataRecoberyResponseDTO>.Fail(404, Activator.CreateInstance<DataRecoberyResponseDTO>(), "El usuario no se encuentra registrado.");
                }
            }
            catch (Exception ex)
            {
                return Result<DataRecoberyResponseDTO>.Fail(500, Activator.CreateInstance<DataRecoberyResponseDTO>(),
                    "Error interno del servidor, vuelva a intentarlo." + ex.Message);
            }
        }

        public async Task<Result<object>> PasswordRecovery(PasswordRecoveryDTO passwordRecovery)
        {
            try
            {
                User user = await _sqlGenericRepository.GetByIdAsync(passwordRecovery.Id);
                string newPassword = _authentication.EncryptationSHA256(passwordRecovery.NewPassword);
                if (newPassword != user.Password)
                {
                    user.Password = newPassword;
                    bool state = await _sqlGenericRepository.UpdateByEntityAsync(user);
                    if (state)
                    {
                        return Result<object>.Ok(200, Activator.CreateInstance<object>(), "La contraseña se a cambiado correctamente.");
                    }
                    else
                    {
                        return Result<object>.Fail(200, Activator.CreateInstance<object>(), "El usuario no existe.");
                    }
                }
                else
                {
                    return Result<object>.Fail(200, Activator.CreateInstance<object>(), "No puede utilizar la misma contraseña.");
                }
            }
            catch (Exception ex)
            {
                return Result<object>.Fail(200, Activator.CreateInstance<object>(), "Error interno del servidor, vuelva a intentarlo." + ex.Message);
            }
        }

        public async Task<Result<object>> PasswordUpdate(PasswordUpdateDTO passwordUpdate)
        {
            try
            {
                string currentPassword = _authentication.EncryptationSHA256(passwordUpdate.CurrentPassword);
                string newPassword = _authentication.EncryptationSHA256(passwordUpdate.NewPassword);
                User user = await _sqlGenericRepository.GetByIdAsync(passwordUpdate.Id);

                if (currentPassword == user.Password)
                {
                    if (newPassword != currentPassword)
                    {
                        user.Password = newPassword;
                        bool state = await _sqlGenericRepository.UpdateByEntityAsync(user);
                        if (state)
                        {
                            return Result<object>.Ok(200, Activator.CreateInstance<object>(), "La contraseña se a cambiado correctamente.");
                        }
                        else
                        {
                            return Result<object>.Fail(404, Activator.CreateInstance<object>(), "El usuario no existe.");
                        }
                    }
                    else
                    {
                        return Result<object>.Fail(409, Activator.CreateInstance<object>(), "La nueva contraseña es igual a la contraseña actual.");
                    }
                }
                return Result<object>.Fail(401, Activator.CreateInstance<object>(), "La contraseña no coincide.");
            }
            catch (Exception ex)
            {
                return Result<object>.Fail(500, Activator.CreateInstance<object>(), "Error interno del servidor, vuelva a intentarlo." + ex.Message);
            }
        }

        public async Task<Result<object>> UserDelete(int id)
        {
            try
            {
                User? user = (await _sqlGenericRepository.GetAsync(a => a.Id == id)).FirstOrDefault();
                if (user != null)
                {
                    bool state = await _sqlGenericRepository.DeleteByIdAsync(user.Id);
                    if (state == true)
                    {
                        return Result<object>.Ok(200, Activator.CreateInstance<object>(), "Usuario borrado correctamente.");
                    }
                    else
                    {
                        return Result<object>.Ok(404, Activator.CreateInstance<object>(), "Error al eliminar usuario.");

                    }
                }
                else
                {
                    return Result<object>.Fail(404, Activator.CreateInstance<object>(), "No se encontro el usuario.");

                }
            }
            catch (Exception ex)
            {
                return Result<object>.Fail(500, Activator.CreateInstance<object>(), "Error interno del servidor, vuelva a intentarlo." + ex.Message);

            }
        }
    }
}
