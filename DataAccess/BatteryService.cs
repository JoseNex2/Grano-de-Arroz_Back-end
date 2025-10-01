using DataAccess.Generic;
using Entities.DataContext;
using Entities.Domain;
using Entities.Domain.DTO;
using Entities.Domain.DTO.Response;
using Utilities;

namespace DataAccess
{
    public interface IBatteryService
    {
        Task<ResultService<BatteryViewDTO>> BatteryRegister(BatteryDTO batteryDTO);
        Task<ResultService<BatteriesSearchResponseDTO>> BatteriesSearch();
        Task<ResultService<RawDataResponseDTO>> UploadRawData(RawDataDTO rawDataDTO);
    }


    public class BatteryService : IBatteryService
    {
        private readonly ISqlGenericRepository<Battery, ServiceDbContext> _batterySqlGenericRepository;
        private readonly ISqlGenericRepository<Measurement, ServiceDbContext> _measurementSqlGenericRepository;
        private readonly INonSqlGenericRepository<PointsRecord> _nonSqlGenericRepository;
        private readonly ICsvService _csvService;

        public BatteryService(ISqlGenericRepository<Battery, ServiceDbContext> batterySqlGenericRepository, ISqlGenericRepository<Measurement, ServiceDbContext> measurementSqlGenericRepository, INonSqlGenericRepository<PointsRecord> nonSqlGenericRepository, ICsvService csvService)
        {
            _batterySqlGenericRepository = batterySqlGenericRepository;
            _measurementSqlGenericRepository = measurementSqlGenericRepository;
            _nonSqlGenericRepository = nonSqlGenericRepository;
            _csvService = csvService;
        }
        public async Task<ResultService<BatteryViewDTO>> BatteryRegister(BatteryDTO batteryDTO)
        {
            try
            {
                bool estado = false;
                Battery? batteryFound = (await _batterySqlGenericRepository.GetAsync(a => a.BatteryGDA == batteryDTO.BatteryGDA)).FirstOrDefault();
                if (batteryFound != null)
                {
                    if (batteryFound.Ot == null && batteryFound.SaleDate == null && batteryFound.ClientId == null)
                    {
                        batteryFound.Ot = batteryDTO.Ot;
                        batteryFound.SaleDate = batteryDTO.SaleDate;
                        batteryFound.ClientId = batteryDTO.ClientId;
                        estado = await _batterySqlGenericRepository.UpdateByEntityAsync(batteryFound);

                        BatteryViewDTO batteryView = new BatteryViewDTO
                        {
                            Id = batteryFound.Id,
                            BatteryGDA = batteryFound.BatteryGDA,
                            Ot = batteryFound.Ot,
                            Type = batteryFound.Type,
                            SaleDate = batteryFound.SaleDate.Value,

                        };
                        if (estado == true)
                        {
                            return ResultService<BatteryViewDTO>.Ok(201, batteryView, "Bateria asociada al cliente.");
                        }
                        else
                        {
                            return ResultService<BatteryViewDTO>.Fail(500, Activator.CreateInstance<BatteryViewDTO>(), "Error al asociar la bateria.");
                        }
                    }
                    else
                    {
                        return ResultService<BatteryViewDTO>.Fail(409, Activator.CreateInstance<BatteryViewDTO>(), "La bateria ya se encuentra asociada a un cliente.");
                    }
                }
                else
                {
                    return ResultService<BatteryViewDTO>.Fail(409, Activator.CreateInstance<BatteryViewDTO>(), "No se encuentra la bateria registrada.");
                }
            }
            catch (Exception ex)
            {
                return ResultService<BatteryViewDTO>.Fail(500, Activator.CreateInstance<BatteryViewDTO>(), ex.Message);

            }
        }
        public async Task<ResultService<BatteriesSearchResponseDTO>> BatteriesSearch()
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
                        BatteryGDA = battery.BatteryGDA,
                        Ot = battery.Ot,
                        Type = battery.Type,
                        SaleDate = battery.SaleDate,
                        Client = new ClientViewDTO
                        {
                            Id = battery.Client.Id,
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

                return ResultService<BatteriesSearchResponseDTO>.Ok(200, response);
            }
            catch (Exception ex)
            {
                return ResultService<BatteriesSearchResponseDTO>.Fail(500, Activator.CreateInstance<BatteriesSearchResponseDTO>(), "Error interno del servidor, vuelva a intentarlo. " + ex.Message);
            }
        }

        public async Task<ResultService<RawDataResponseDTO>> UploadRawData(RawDataDTO rawDataDTO)
        {
            try
            {
                var extension = Path.GetExtension(rawDataDTO.File.FileName).ToLowerInvariant();
                if (rawDataDTO.File == null || rawDataDTO.File.Length == 0)
                {
                    return ResultService<RawDataResponseDTO>.Fail(409, Activator.CreateInstance<RawDataResponseDTO>(), "Archivo no proporcionado.");
                }
                else if (extension != ".csv")
                {
                    return ResultService<RawDataResponseDTO>.Fail(409, Activator.CreateInstance<RawDataResponseDTO>(), "Solo archivos CSV permitidos.");
                }
                else
                {
                    DateTime measurementDateTime = rawDataDTO.MeasurementDate.ToDateTime(TimeOnly.MinValue);
                    Battery? batteryFound = (await _batterySqlGenericRepository.GetAsync(a => a.BatteryGDA == rawDataDTO.BatteryGDA)).FirstOrDefault();
                    if (batteryFound == null)
                    {
                        Battery battery = new Battery
                        {
                            BatteryGDA = rawDataDTO.BatteryGDA,
                            Ot = null,
                            Type = rawDataDTO.Type,
                            SaleDate = null,
                            DateRegistered = DateTime.Now,
                            ClientId = null
                        };
                        int? id = await _batterySqlGenericRepository.CreateAsync(battery);
                        Measurement measurement = new Measurement
                        {
                            Magnitude = rawDataDTO.Magnitude,
                            MeasurementDate = measurementDateTime,
                            BatteryId = id.Value
                        };
                        id = await _measurementSqlGenericRepository.CreateAsync(measurement);
                        using (StreamReader reader = new StreamReader(rawDataDTO.File.OpenReadStream()))
                        {
                            Dictionary<TimeOnly, float> points = await _csvService.CsvToDictionary(reader);
                            PointsRecord pointsRecord = new PointsRecord
                            {
                                Id = id.Value,
                                Points = points
                            };
                            await _nonSqlGenericRepository.CreateAsync(pointsRecord);
                            return ResultService<RawDataResponseDTO>.Ok(200, Activator.CreateInstance<RawDataResponseDTO>(), "Las mediciones fueron cargadas correctamente.");
                        }
                    }
                    else
                    {
                        Measurement? measurementFound = (await _measurementSqlGenericRepository.GetAsync(a => a.MeasurementDate == measurementDateTime && a.Magnitude == rawDataDTO.Magnitude)).FirstOrDefault();
                        if (measurementFound == null)
                        {
                            Measurement measurement = new Measurement
                            {
                                Magnitude = rawDataDTO.Magnitude,
                                MeasurementDate = measurementDateTime,
                                BatteryId = batteryFound.Id
                            };
                            int? id = await _measurementSqlGenericRepository.CreateAsync(measurement);
                            using (StreamReader reader = new StreamReader(rawDataDTO.File.OpenReadStream()))
                            {
                                Dictionary<TimeOnly, float> points = await _csvService.CsvToDictionary(reader);
                                PointsRecord pointsRecord = new PointsRecord
                                {
                                    Id = id.Value,
                                    Points = points
                                };
                                await _nonSqlGenericRepository.CreateAsync(pointsRecord);
                            }
                            return ResultService<RawDataResponseDTO>.Ok(200, Activator.CreateInstance<RawDataResponseDTO>(), "Las mediciones fueron cargadas correctamente.");
                        }
                        else
                        {
                            return ResultService<RawDataResponseDTO>.Fail(409, Activator.CreateInstance<RawDataResponseDTO>(), "Las mediciones ya se encuentran en el sistema.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ResultService<RawDataResponseDTO>.Fail(500, Activator.CreateInstance<RawDataResponseDTO>(), "Error interno del servidor, vuelva a intentarlo. " + ex.Message);
            }
        }
    }
}
