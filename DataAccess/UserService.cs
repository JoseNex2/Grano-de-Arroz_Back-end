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
        Task<(int, bool, string)> UserRegister(UserDTO userDTO);
        Task<(int, bool, int, string, string)> Login(LoginDTO loginDTO);
        Task<IEnumerable<UserViewDTO>> UsersSearch();
        Task<UserViewDTO> UserSearch(int id);
        Task<(int, bool, string)> RoleUpdate(RoleUpdateDTO roleUpdate);
        Task<(int, bool, int, string)> AccountRecovery(DataRecoveryDTO dataRecovery);
        Task<(int, bool, string)> PasswordRecovery(PasswordRecoveryDTO passwordRecovery);
        Task<(int, bool, string)> PasswordUpdate(PasswordUpdateDTO passwordUpdate);
        Task<(int, bool, string)> UserDelete(int id);

    }
    public class UserService : IUserService
    {
        private readonly ISqlGenericRepository<User, UserDbContext> _sqlGenericRepository;
        private readonly Authentication _authentication;
        public UserService(ISqlGenericRepository<User, UserDbContext> sqlGenericRepository, Authentication authentication)
        {
            _sqlGenericRepository = sqlGenericRepository;
            _authentication = authentication;
        }

        public async Task<(int, bool, string)> UserRegister(UserDTO user)
        {
            try
            {
                bool estado = false;
                User usuarioEncontrado = (await _sqlGenericRepository.GetAsync(a => a.Username == user.Username || a.Email == user.Email)).SingleOrDefault();
                if (usuarioEncontrado == null)
                {
                    User userModel = new User
                    {
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        Password = _authentication.EncryptationSHA256(user.Password),
                        DateRegistered = DateTime.Now
                    };
                    int? id = await _sqlGenericRepository.CreateAsync(userModel);
                    estado = true;
                    if (id != null && estado == true)
                    {
                        return (200, true, "Usuario creado.");
                    }
                    else
                    {
                        return (500, false, "Error al registrar el usuario.");
                    }
                }
                else
                {
                    return (200, false, "Usuario ya existe.");
                }
            }
            catch (Exception ex)
            {
                return (500, true, ex.ToString());
            }
        }

        public async Task<(int, bool, int, string, string)> Login(LoginDTO login)
        {
            try
            {
                User userFound = (await _sqlGenericRepository.GetAsync(a => a.Username == login.Username && a.Password == _authentication.EncryptationSHA256(login.Password))).SingleOrDefault();
                if (userFound == null)
                {
                    return (200, false, 0, "", "");
                }
                else
                {
                    return (200, true, userFound.Id, userFound.Role, _authentication.GenerateAccessJwt(userFound));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (500, true, 0, "", "Error interno del servidor, vuelva a intentarlo.");
            }
        }

        public async Task<IEnumerable<UserViewDTO>> UsersSearch()
        {
            IEnumerable<User> users = await _sqlGenericRepository.GetAllAsync();
            List<UserViewDTO> usersDTO = new List<UserViewDTO>();
            foreach (User user in users)
            {
                UserViewDTO userDTO = new UserViewDTO
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    DateRegistered = user.DateRegistered.ToLocalTime()
                };
                usersDTO.Add(userDTO);
            }
            return usersDTO;
        }

        public async Task<UserViewDTO> UserSearch(int id)
        {
            User user = (await _sqlGenericRepository.GetAsync(a => a.Id == id)).FirstOrDefault();
            UserViewDTO usuarioDTO = new UserViewDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                DateRegistered = user.DateRegistered.ToLocalTime()
            };
            return usuarioDTO;
        }

        public async Task<(int, bool, string)> RoleUpdate(RoleUpdateDTO roleUpdate)
        {
            try
            {
                User userModel = await _sqlGenericRepository.GetByIdAsync(roleUpdate.Id);
                userModel.Role = roleUpdate.Role;
                bool state = await _sqlGenericRepository.UpdateByEntityAsync(userModel);
                if (state)
                {
                    return(200, true, "");
                }
                else
                {
                    return(200, false, "");
                }
            }
            catch (Exception ex)
            {
                return(500, true, "Error interno del servidor, vuelva a intentarlo.");
            }
        }

        public async Task<(int, bool, int, string)> AccountRecovery(DataRecoveryDTO dataRecovery)
        {
            try
            {
                User userFound = (await _sqlGenericRepository.GetAsync(a => a.Email == dataRecovery.Email)).SingleOrDefault();
                if (userFound != null)
                {
                    string tokenRecovery = _authentication.GenerateRecoveryJwt(userFound);
                    MimeMessage emailMessage = new MimeMessage();

                    emailMessage.From.Add(new MailboxAddress("Sistema de recuperación de contraseña", "valentin.martinezdev@gmail.com"));
                    emailMessage.To.Add(new MailboxAddress("", dataRecovery.Email));
                    emailMessage.Subject = "Recuperar contraseña";
                    emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                    {
                        Text = dataRecovery.Url + "/" + tokenRecovery
                    };
                    using (SmtpClient client = new SmtpClient())
                    {
                        await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                        await client.AuthenticateAsync("valentin.martinezdev@gmail.com", "evny knhp vhzc mtqa");
                        await client.SendAsync(emailMessage);

                        await client.DisconnectAsync(true);
                    }
                    return(200, true, userFound.Id, tokenRecovery);
                }
                else
                {
                    return(200, false, 0, "");
                }
            }
            catch (Exception ex)
            {
                return(500, true, 0, "Error interno del servidor, vuelva a intentarlo.");
            }
        }

        public async Task<(int, bool, string)> PasswordRecovery(PasswordRecoveryDTO passwordRecovery)
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
                        return(200, true, "La contraseña se a cambiado correctamente.");
                    }
                    else
                    {
                        return(200, false, "Error al cambiar la contraseña.");
                    }
                }
                else
                {
                    return(200, false, "No puede utilizar la misma contraseña.");
                }
            }
            catch (Exception ex)
            {
                return(500, true, "Error interno del servidor, vuelva a intentarlo.");
            }
        }

        public async Task<(int, bool, string)> PasswordUpdate(PasswordUpdateDTO passwordUpdate)
        {
            try
            {
                string currentPassword = _authentication.EncryptationSHA256(passwordUpdate.CurrentPassword);
                string newPassword = _authentication.EncryptationSHA256(passwordUpdate.NewPassword);
                User user = await _sqlGenericRepository.GetByIdAsync(passwordUpdate.Id);
                if (user != null)
                {
                    if (currentPassword == user.Password)
                    {
                        if (newPassword != currentPassword)
                        {
                            user.Password = newPassword;
                            bool state = await _sqlGenericRepository.UpdateByEntityAsync(user);
                            if (state)
                            {
                                return(200, true, "La contraseña se a cambiado correctamente.");
                            }
                            else
                            {
                                return(200, false, "Error al cambiar la contraseña.");
                            }
                        }
                        else
                        {
                            return(200, false, "La nueva contraseña es igual a la contraseña actual.");
                        }
                    }
                    else
                    {
                        return(200, false, "La contraseña actual no coincide.");
                    }
                }
                else
                {
                    return(200, false, "No se encontro el usuario.");
                }
            }
            catch (Exception ex)
            {
                return(500, true, "Error interno del servidor, vuelva a intentarlo.");
            }
        }

        public async Task<(int, bool, string)> UserDelete(int id)
        {
            try
            {
                User user = (await _sqlGenericRepository.GetAsync(a => a.Id == id)).FirstOrDefault();
                if (user != null)
                {
                    bool state = await _sqlGenericRepository.DeleteByIdAsync(user.Id);
                    if (state == true)
                    {
                        return (200, true, "El usuario fue borrado correctamente.");
                    }
                    else
                    {
                        return (200, true, "Error al eliminar usuario.");
                    }
                }
                else
                {
                    return (200, false, "No se encontro el usuario.");
                }
            }
            catch (Exception ex)
            {
                return (500, true, "Error interno del servidor, vuelva a intentarlo.");
            }
        }
    }
}
