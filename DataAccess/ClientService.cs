using DataAccess.Generic;
using Entities.DataContext;
using Entities.Domain.DTO;
using Entities.Domain.DTO.Response;
using Utilities;

namespace DataAccess
{
    public interface IClientService
    {
        Task<ResultService<ClientViewDTO>> ClientRegister(ClientDTO clientDTO);
        Task<ResultService<ClientsSearchResponseDTO>> ClientsSearch();
        Task<ResultService<ClientViewDTO>> ClientSearch(int id);
        Task<ResultService<ClientViewDTO>> ClientUpdate(ClientUpdateDTO clientDTO);
        Task<ResultService<ClientViewDTO>> ClientDelete(int id);
    }

    public class ClientService : IClientService
    {
        private readonly ISqlGenericRepository<Client, ServiceDbContext> _sqlGenericRepository;

        public ClientService(ISqlGenericRepository<Client, ServiceDbContext> sqlGenericRepository)
        {
            _sqlGenericRepository = sqlGenericRepository;
        }
        public async Task<ResultService<ClientViewDTO>> ClientRegister(ClientDTO clientDTO)
        {
            try
            {
                bool estado = false;
                Client? clienteEncontrado = (await _sqlGenericRepository.GetAsync(a => a.NationalId == clientDTO.NationalId || a.Email == clientDTO.Email)).SingleOrDefault();
                if (clienteEncontrado != null)
                {
                    return ResultService<ClientViewDTO>.Fail(409, Activator.CreateInstance<ClientViewDTO>(), "Cliente ya registrado.");
                }
                Client clientModel = new Client
                {
                    Name = clientDTO.Name,
                    Email = clientDTO.Email,
                    LastName = clientDTO.LastName,
                    NationalId = clientDTO.NationalId,
                    PhoneNumber = clientDTO.PhoneNumber,
                    RegisteredDate = DateTime.Now
                };
                int? id = await _sqlGenericRepository.CreateAsync(clientModel);
                estado = true;

                ClientViewDTO clientView = new ClientViewDTO
                {
                    Id = id.Value,
                    Name = clientModel.Name,
                    Email = clientModel.Email,
                    LastName = clientModel.LastName,
                    NationalId = clientModel.NationalId,
                    PhoneNumber = clientModel.PhoneNumber,
                    RegisteredDate = clientModel.RegisteredDate

                };
                if (id != null && estado == true)
                {
                    return ResultService<ClientViewDTO>.Ok(201, clientView, "Cliente registrado.");
                }
                else
                {
                    return ResultService<ClientViewDTO>.Fail(500, Activator.CreateInstance<ClientViewDTO>(), "Error al registrar el cliente.");
                }
            }
            catch (Exception ex)
            {
                return ResultService<ClientViewDTO>.Fail(500, Activator.CreateInstance<ClientViewDTO>(), ex.Message);
            }
        }
        public async Task<ResultService<ClientsSearchResponseDTO>> ClientsSearch()
        {
            try
            {
                IEnumerable<Client> clients = await _sqlGenericRepository.GetAsync();
                List<ClientViewDTO> clientsDTO = new List<ClientViewDTO>();
                foreach (Client client in clients)
                {
                    ClientViewDTO clientDTO = new ClientViewDTO
                    {
                        Id = client.Id,
                        Name = client.Name,
                        Email = client.Email,
                        LastName = client.LastName,
                        NationalId = client.NationalId,
                        PhoneNumber = client.PhoneNumber,
                        RegisteredDate = client.RegisteredDate
                    };
                    clientsDTO.Add(clientDTO);
                }

                ClientsSearchResponseDTO response = new ClientsSearchResponseDTO
                {
                    TotalClients = clientsDTO.Count,
                    Clients = clientsDTO
                };

                return ResultService<ClientsSearchResponseDTO>.Ok(200, response);
            }
            catch (Exception ex)
            {
                return ResultService<ClientsSearchResponseDTO>.Fail(500, Activator.CreateInstance<ClientsSearchResponseDTO>(), "Error interno del servidor, vuelva a intentarlo. " + ex.Message);
            }

        }

        public async Task<ResultService<ClientViewDTO>> ClientSearch(int id)
        {
            try
            {
                Client? client = (await _sqlGenericRepository.GetAsync(a => a.Id == id)).FirstOrDefault();

                if (client == null)
                {
                    return ResultService<ClientViewDTO>.Ok(404, Activator.CreateInstance<ClientViewDTO>(), "Usuario no encontrado.");
                }

                ClientViewDTO clientView = new ClientViewDTO
                {
                    Id = client.Id,
                    Name = client.Name,
                    Email = client.Email,
                    LastName = client.LastName,
                    NationalId = client.NationalId,
                    PhoneNumber = client.PhoneNumber,
                    RegisteredDate = client.RegisteredDate
                };
                return ResultService<ClientViewDTO>.Ok(200, clientView);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResultService<ClientViewDTO>> ClientUpdate(ClientUpdateDTO clientDTO)
        {
            try
            {
                Client client = (await _sqlGenericRepository.GetAsync(a => a.Id == clientDTO.Id)).SingleOrDefault();

                if (client == null)
                {
                    return ResultService<ClientViewDTO>.Fail(404, Activator.CreateInstance<ClientViewDTO>(), "El cliente no se encuentra registrado");
                }

                client.Email = clientDTO.Email;
                client.PhoneNumber = clientDTO.PhoneNumber;

                bool state = await _sqlGenericRepository.UpdateByEntityAsync(client);

                ClientViewDTO clientView = new ClientViewDTO
                {
                    Id = client.Id,
                    Name = client.Name,
                    Email = client.Email,
                    LastName = client.LastName,
                    NationalId = client.NationalId,
                    PhoneNumber = client.PhoneNumber,
                    RegisteredDate = client.RegisteredDate
                };

                return ResultService<ClientViewDTO>.Ok(200, clientView, "Cliente  actualizado correctamente");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al actualizar la entidad.", ex);
            }
        }

        public async Task<ResultService<ClientViewDTO>> ClientDelete(int id)
        {
            try
            {
                Client? client = (await _sqlGenericRepository.GetAsync(a => a.Id == id)).FirstOrDefault();
                if (client == null)
                {
                    return ResultService<ClientViewDTO>.Fail(404, Activator.CreateInstance<ClientViewDTO>(), "No se encontro el cliente.");
                }
                bool state = await _sqlGenericRepository.DeleteByIdAsync(client.Id);
                if (state == true)
                {
                    return ResultService<ClientViewDTO>.Ok(200, Activator.CreateInstance<ClientViewDTO>(), "Cliente borrado correctamente.");
                }
                else
                {
                    return ResultService<ClientViewDTO>.Ok(404, Activator.CreateInstance<ClientViewDTO>(), "No se encontro el cliente.");
                }
            }
            catch (Exception ex)
            {
                return ResultService<ClientViewDTO>.Fail(500, Activator.CreateInstance<ClientViewDTO>(), "Error interno del servidor, vuelva a intentarlo." + ex.Message);
            }
        }
    }
}

        
    


