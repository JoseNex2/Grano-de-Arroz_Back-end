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
        Task ReceiveRawData();
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
                Battery? batteryFound = (await _batterySqlGenericRepository.GetAsync(a => a.ChipId == batteryDTO.ChipId)).FirstOrDefault();

                if (batteryFound == null)
                {
                    Battery batteryModel = new Battery
                    {
                        ChipId = batteryDTO.ChipId,
                        Ot = batteryDTO.Ot,
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
                        ChipId = batteryDTO.ChipId,
                        Ot = batteryDTO.Ot,
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
                        return Result<BatteryViewDTO>.Fail(500, Activator.CreateInstance<BatteryViewDTO>(), "Error al registrar la bateria.");

                    }
                }
                else
                {
                    return Result<BatteryViewDTO>.Fail(409, Activator.CreateInstance<BatteryViewDTO>(), "Bateria ya registrada.");

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
                        ChipId = battery.ChipId,
                        Ot = battery.Ot,
                        Type = battery.Type,
                        Status = battery.Status,
                        SaleDate = battery.SaleDate,
                        Client = new ClientViewDTO
                        {
                            Id = battery.ClientId,
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

        public async Task ReceiveRawData()
        {

        }
    }
}
