using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Generic;
using Entities.DataContext;
using Entities.Domain;
using Entities.Domain.DTO;
using Utilities;

namespace DataAccess
{
    public interface IBatteryService
    {
        Task<Result<BatteryViewDTO>> BatteryRegister(BatteryDTO clientDTO);
        Task<Result<BatteriesSearchResponseDTO>> BatteriesSearch();
        //Task<Result<BatteryViewDTO>> BatterySearch(int id);
        //Task<Result<BatteryViewDTO>> BatteryUpdate(BatteryUpdateDTO clientDTO);
        //Task<Result<BatteryViewDTO>> BatteryDelete(int id);
    }


    public class BatteryService : IBatteryService
    {
        private readonly ISqlGenericRepository<Battery, ServiceDbContext> _batterySqlGenericRepository;
        private readonly ISqlGenericRepository<Client, ServiceDbContext> _clientSqlGenericRepository;

        public BatteryService(ISqlGenericRepository<Battery, ServiceDbContext> batterySqlGenericRepository, ISqlGenericRepository<Client, ServiceDbContext> clientSqlRepository)
        {
            _batterySqlGenericRepository = batterySqlGenericRepository;
            _clientSqlGenericRepository = clientSqlRepository;
        }
        public async Task<Result<BatteryViewDTO>> BatteryRegister(BatteryDTO batteryDTO)
        {
            try
            {
                bool estado = false;
                Battery? batteryFound = (await _batterySqlGenericRepository.GetAsync(a => a.ID_Chip == batteryDTO.ID_Chip)).FirstOrDefault();

                if (batteryFound == null)
                {
                    Battery batteryModel = new Battery
                    {
                        ID_Chip = batteryDTO.ID_Chip,
                        OT = batteryDTO.OT,
                        Type = batteryDTO.Type,
                        Status = batteryDTO.Status,
                        SaleDate = batteryDTO.SaleDate,
                        DateRegistered = DateTime.Now
                    };
                    int? id = await _batterySqlGenericRepository.CreateAsync(batteryModel);
                    estado = true;

                    BatteryViewDTO batteryView = new BatteryViewDTO
                    {
                        Id = id.Value,
                        ID_Chip = batteryDTO.ID_Chip,
                        OT = batteryDTO.OT,
                        Type = batteryDTO.Type,
                        Status = batteryDTO.Status,
                        SaleDate = batteryDTO.SaleDate,

                    };
                    if (id != null && estado == true)
                    {
                        return Result<BatteryViewDTO>.Ok(201, batteryView, "Bateria registrado.");

                    }
                    else
                    {
                        return Result<BatteryViewDTO>.Fail(500, Activator.CreateInstance<BatteryViewDTO>(), "Error al registrar el cliente.");

                    }
                }
                else
                {
                    return Result<BatteryViewDTO>.Fail(409, Activator.CreateInstance<BatteryViewDTO>(), "Cliente ya registrado.");

                }
            }
            catch (Exception ex)
            {
                return Result<BatteryViewDTO>.Fail(500, Activator.CreateInstance<BatteryViewDTO>(), ex.Message);

            }
        }
        public async Task<Result<BatteriesSearchResponseDTO>> BatteriesSearch()
        {
            try
            {
                IEnumerable<Battery> batteries = await _batterySqlGenericRepository.GetAllAsync(b => b.Client);
                List<BatteryViewDTO> batteriesDTO = new List<BatteryViewDTO>();
                foreach (Battery battery in batteries)
                {
                    BatteryViewDTO batteryDTO = new BatteryViewDTO
                    {
                        Id = battery.Id,
                        ID_Chip = battery.ID_Chip,
                        OT = battery.OT,
                        Type = battery.Type,
                        Status = battery.Status,
                        SaleDate = battery.SaleDate,
                        Client = new ClientViewDTO
                        {
                            Id = battery.ClientID,
                            Name = battery.Client.Name,
                            LastName = battery.Client.LastName,
                            NationalId = battery.Client.NationalId,
                            Email = battery.Client.Email,
                            PhoneNumber = battery.Client.PhoneNumber,
                            DateRegistered = battery.Client.DateRegistered,
                        }
                    };
                    batteriesDTO.Add(batteryDTO);

                }

                BatteriesSearchResponseDTO response = new BatteriesSearchResponseDTO
                {
                    TotalBatteries = batteriesDTO.Count,
                    Batteries = batteriesDTO
                };

                return Result<BatteriesSearchResponseDTO>.Ok(200, response);
            }
            catch (Exception ex)
            {
                return Result<BatteriesSearchResponseDTO>.Fail(
                    500,
                    Activator.CreateInstance<BatteriesSearchResponseDTO>(),
                    "Error interno del servidor, vuelva a intentarlo. " + ex.Message);
            }
        }

    }
}
