using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Generic;
using Entities.DataContext;
using Entities.Domain;
using Entities.Domain.DTO;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Core.Clusters;
using Utilities;

namespace DataAccess
{
    public interface IClientService
    {
        Task<Result<ClientViewDTO>> ClientRegister(ClientDTO clientDTO);
        Task<Result<IEnumerable<ClientViewDTO>>> ClientsSearch();
        Task<Result<ClientViewDTO>> ClientSearch(int id);
        Task<Result<ClientViewDTO>> ClientUpdate(ClientUpdateDTO clientDTO);
        Task<Result<object>> ClientDelete(int id);
    }

    public class ClientService : IClientService
    {
        private readonly ISqlGenericRepository<Client, ServiceDbContext> _sqlGenericRepository;

        public ClientService(ISqlGenericRepository<Client, ServiceDbContext> sqlGenericRepository)
        {
            _sqlGenericRepository = sqlGenericRepository;
        }
        public async Task<Result<ClientViewDTO>> ClientRegister(ClientDTO clientDTO)
        {
            try
            {
                bool estado = false;
                Client? clienteEncontrado = (await _sqlGenericRepository.GetAsync(a => a.NationalId == clientDTO.NationalId || a.Email == clientDTO.Email)).SingleOrDefault();
                if (clienteEncontrado == null)
                {
                    Client clientModel = new Client
                    {
                        Name = clientDTO.Name,
                        Email = clientDTO.Email,
                        LastName = clientDTO.LastName,
                        NationalId = clientDTO.NationalId,
                        NroGDA = clientDTO.NroGDA,
                        PhoneNumber = clientDTO.PhoneNumber,
                        SaleDate = clientDTO.SaleDate,
                        DateRegistered = DateTime.Now
                    };
                    int? id = await _sqlGenericRepository.CreateAsync(clientModel);
                    estado = true;

                    ClientViewDTO clientView = new ClientViewDTO
                    {
                        Id = clientModel.Id,
                        Name = clientModel.Name,
                        Email = clientModel.Email,
                        LastName = clientModel.LastName,
                        NationalId = clientModel.NationalId,
                        NroGDA = clientModel.NroGDA,
                        PhoneNumber = clientModel.PhoneNumber,
                        SaleDate = clientModel.SaleDate,
                        DateRegistered = clientModel.DateRegistered

                    };
                    if (id != null && estado == true)
                    {
                        return Result<ClientViewDTO>.Ok(201, clientView, "Cliente registrado.");

                    }
                    else
                    {
                        return Result<ClientViewDTO>.Fail(500, Activator.CreateInstance<ClientViewDTO>(), "Error al registrar el cliente.");

                    }
                }
                else
                {
                    return Result<ClientViewDTO>.Fail(409, Activator.CreateInstance<ClientViewDTO>(), "Cliente ya registrado.");

                }
            }
            catch (Exception ex)
            {
                return Result<ClientViewDTO>.Fail(500, Activator.CreateInstance<ClientViewDTO>(), ex.Message);

            }
        }
        public async Task<Result<IEnumerable<ClientViewDTO>>> ClientsSearch()
        {
            try
            {
                IEnumerable<Client> clients = await _sqlGenericRepository.GetAllAsync();
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
                        NroGDA = client.NroGDA,
                        PhoneNumber = client.PhoneNumber,
                        SaleDate = client.SaleDate,
                        DateRegistered = client.DateRegistered
                    };
                    clientsDTO.Add(clientDTO);
                }
                return Result<IEnumerable<ClientViewDTO>>.Ok(200, clientsDTO);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<ClientViewDTO>>.Fail(
                    500,
                    Activator.CreateInstance<IEnumerable<ClientViewDTO>>(),
                    "Error interno del servidor, vuelva a intentarlo. " + ex.Message);
            }

        }

        public async Task<Result<ClientViewDTO>> ClientSearch(int id)
        {
            try
            {
                Client? client = (await _sqlGenericRepository.GetAsync(a => a.Id == id)).FirstOrDefault();

                if (client == null)
                {
                    return Result<ClientViewDTO>.Ok(404, Activator.CreateInstance<ClientViewDTO>(), "Usuario no encontrado.");
                }

                ClientViewDTO clientView = new ClientViewDTO
                {
                    Id = client.Id,
                    Name = client.Name,
                    Email = client.Email,
                    LastName = client.LastName,
                    NationalId = client.NationalId,
                    NroGDA = client.NroGDA,
                    PhoneNumber = client.PhoneNumber,
                    SaleDate = client.SaleDate,
                    DateRegistered = client.DateRegistered
                };
                return Result<ClientViewDTO>.Ok(200, clientView);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Result<ClientViewDTO>> ClientUpdate(ClientUpdateDTO clientDTO)
        {
            try
            {
                Client client = await _sqlGenericRepository.GetByIdAsync(clientDTO.Id);

                if (client == null)
                {
                    return Result<ClientViewDTO>.Fail(404, Activator.CreateInstance<ClientViewDTO>(), "El cliente no se encuentra registrado");
                }

                client.Name = clientDTO.Name;
                client.Email = clientDTO.Email;
                client.LastName = clientDTO.LastName;
                client.NationalId = clientDTO.NationalId;
                client.PhoneNumber = clientDTO.PhoneNumber;
                client.SaleDate = clientDTO.SaleDate;
                client.NroGDA = clientDTO.NroGDA;

                bool state = await _sqlGenericRepository.UpdateByEntityAsync(client);

                ClientViewDTO clientView = new ClientViewDTO
                {
                    Id = client.Id,
                    Name = client.Name,
                    Email = client.Email,
                    LastName = client.LastName,
                    NationalId = client.NationalId,
                    PhoneNumber = client.PhoneNumber,
                    SaleDate = client.SaleDate,
                    DateRegistered = client.DateRegistered,
                    NroGDA = client.NroGDA
                };

                return Result<ClientViewDTO>.Ok(200, clientView, "Cliente  actualizado correctamente");
            }
            catch (Exception ex)
            {

                throw new InvalidOperationException("Error al actualizar la entidad.", ex);
            }
        }

        public async Task<Result<object>> ClientDelete(int id)
        {
            try
            {
                Client? client = (await _sqlGenericRepository.GetAsync(a => a.Id == id)).FirstOrDefault();
                if (client != null)
                {
                    bool state = await _sqlGenericRepository.DeleteByIdAsync(client.Id);
                    if (state == true)
                    {
                        return Result<object>.Ok(200, Activator.CreateInstance<object>(), "Cliente borrado correctamente.");
                    }
                    else
                    {
                        return Result<object>.Ok(404, Activator.CreateInstance<object>(), "No se encontro el cliente.");

                    }
                }
                else
                {
                    return Result<object>.Fail(404, Activator.CreateInstance<object>(), "No se encontro el cliente.");

                }
            }
            catch (Exception ex)
            {
                return Result<object>.Fail(500, Activator.CreateInstance<object>(), "Error interno del servidor, vuelva a intentarlo." + ex.Message);

            }
        }
    }
}

        
    


