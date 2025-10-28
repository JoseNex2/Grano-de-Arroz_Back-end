using DataAccess.Generic;
using Entities.DataContext;
using Entities.Domain;
using Entities.Domain.DTO;
using Entities.Domain.DTO.Response;
using System.Diagnostics.Metrics;
using Utilities;

namespace DataAccess
{
    public interface IBatteryService
    {
        Task<ResultService<BatteryViewDTO>> BatteryRegister(BatteryDTO batteryDTO);
        Task<ResultService<BatteriesSearchResponseDTO>> BatteriesSearch();
        Task<ResultService<BatteriesSearchResponseDTO>> BatteriesSearchWithFilter(BatterySearchFilterDTO filter);
        Task<ResultService<BatterySearchResponseDTO>> BatterySearchWithId(int id);
        Task<ResultService<RawDataResponseDTO>> UploadRawData(RawDataDTO rawDataDTO);
    }


    public class BatteryService : IBatteryService
    {
        private readonly ISqlGenericRepository<Battery, ServiceDbContext> _batterySqlGenericRepository;
        private readonly ISqlGenericRepository<Measurement, ServiceDbContext> _measurementSqlGenericRepository;
        private readonly ISqlGenericRepository<Report, ServiceDbContext> _reportSqlGenericRepository;
        private readonly INonSqlGenericRepository<PointsRecord> _nonSqlGenericRepository;
        private readonly ICsvService _csvService;

        public BatteryService(ISqlGenericRepository<Battery, ServiceDbContext> batterySqlGenericRepository, 
            ISqlGenericRepository<Measurement, ServiceDbContext> measurementSqlGenericRepository,
            ISqlGenericRepository<Report, ServiceDbContext> reportSqlGenericRepository,
            INonSqlGenericRepository<PointsRecord> nonSqlGenericRepository, ICsvService csvService)
        {
            _batterySqlGenericRepository = batterySqlGenericRepository;
            _measurementSqlGenericRepository = measurementSqlGenericRepository;
            _reportSqlGenericRepository = reportSqlGenericRepository;
            _nonSqlGenericRepository = nonSqlGenericRepository;
            _csvService = csvService;
        }
        public async Task<ResultService<BatteryViewDTO>> BatteryRegister(BatteryDTO batteryDTO)
        {
            try
            {
                bool estado = false;
                Battery? batteryFound = (await _batterySqlGenericRepository.GetAsync(a => a.ChipId == batteryDTO.ChipId)).FirstOrDefault();
                if (batteryFound == null)
                {
                    return ResultService<BatteryViewDTO>.Fail(409, Activator.CreateInstance<BatteryViewDTO>(), "No se encuentra la bateria registrada.");
                }

                if (batteryFound.WorkOrder != null && batteryFound.SaleDate != null && batteryFound.ClientId != null)
                {
                    return ResultService<BatteryViewDTO>.Fail(409, Activator.CreateInstance<BatteryViewDTO>(), "La bateria ya se encuentra asociada a un cliente.");
                }

                batteryFound.WorkOrder = batteryDTO.WorkOrder;
                batteryFound.SaleDate = batteryDTO.SaleDate;
                batteryFound.ClientId = batteryDTO.ClientId;
                estado = await _batterySqlGenericRepository.UpdateByEntityAsync(batteryFound);

                if (estado == true)
                {
                    return ResultService<BatteryViewDTO>.Ok(201, Activator.CreateInstance<BatteryViewDTO>(), "Bateria asociada al cliente.");
                }
                else
                {
                    return ResultService<BatteryViewDTO>.Fail(500, Activator.CreateInstance<BatteryViewDTO>(), "Error al asociar la bateria.");
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
                IEnumerable<Battery> batteries = await _batterySqlGenericRepository.GetAsync(includes: b => b.Client);
                List<BatteryViewDTO> batteriesDTO = new List<BatteryViewDTO>();
                foreach (Battery battery in batteries)
                {
                    BatteryViewDTO batteryDTO = new BatteryViewDTO
                    {
                        Id = battery.Id,
                        ChipId = battery.ChipId,
                        WorkOrder = battery.WorkOrder,
                        Type = battery.Type,
                        SaleDate = battery.SaleDate,
                        Client = battery.Client == null ? null : new ClientViewDTO
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

        public async Task<ResultService<BatteriesSearchResponseDTO>> BatteriesSearchWithFilter(BatterySearchFilterDTO filter)
        {
            try
            {
                var batteries = await _batterySqlGenericRepository.GetAsync(r => r.Client != null,r => r.Client); // && r.Report == null

                if (!string.IsNullOrWhiteSpace(filter.ChipId))
                {
                    batteries = batteries.Where(r => r.ChipId.Contains(filter.ChipId, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filter.ClientName))
                {
                    batteries = batteries.Where(r => r.Client != null && r.Client.Name.Contains(filter.ClientName, StringComparison.OrdinalIgnoreCase));
                }

                if (filter.SaleDate.HasValue)
                {
                    batteries = batteries.Where(r => r.SaleDate.HasValue && DateOnly.FromDateTime(r.SaleDate.Value) == filter.SaleDate.Value);
                }

                List<BatteryViewDTO> batteriesDTO = new List<BatteryViewDTO>();
                foreach (Battery battery in batteries)
                {
                    var reports = await _reportSqlGenericRepository.GetAsync(a => a.BatteryId == battery.Id);
                    Report? report = reports?.FirstOrDefault();

                    string reportValid = "";

                    if (report == null)
                    {
                        reportValid = "No iniciado";
                    }
                    else if(report != null)
                    {
                        reportValid = report.Status.Name;
                    }

                    BatteryViewDTO batteryDTO = new BatteryViewDTO
                    {
                        Id = battery.Id,
                        ChipId = battery.ChipId,
                        WorkOrder = battery.WorkOrder,
                        Type = battery.Type,
                        Status = reportValid,
                        SaleDate = battery.SaleDate,
                        Client = battery.Client == null ? null : new ClientViewDTO
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

        public async Task<ResultService<BatterySearchResponseDTO>> BatterySearchWithId(int id)
        {
            try
            {
                var batteryFound = (await _batterySqlGenericRepository.GetAsync(r => r.Id == id, r => r.Measurements)).FirstOrDefault();
                if (batteryFound == null)
                {
                    return ResultService<BatterySearchResponseDTO>.Fail(409, Activator.CreateInstance<BatterySearchResponseDTO>(), "No se encuentra la bateria registrada.");
                }

                BatteryViewDTO batteryDto = new BatteryViewDTO
                {
                    Id = id,
                    ChipId = batteryFound.ChipId,
                    WorkOrder = batteryFound.WorkOrder,
                    Type = batteryFound.Type,
                    SaleDate = batteryFound.SaleDate
                };
                List<MeasurementDTO> measurementsDto = new List<MeasurementDTO>();

                foreach(Measurement measurement in batteryFound.Measurements)
                {
                    PointsRecord ?pointsRecord = (await _nonSqlGenericRepository.GetByParameterAsync(a => a.Id == measurement.Id)).FirstOrDefault();

                    if (pointsRecord == null)
                    {
                        return ResultService<BatterySearchResponseDTO>.Fail(409, Activator.CreateInstance<BatterySearchResponseDTO>(), "No se encontraron mediciones cargadas para esta bateria.");
                    }

                    MeasurementDTO measurementDto = new MeasurementDTO
                    {
                        Id = measurement.Id,
                        Magnitude = measurement.Magnitude,
                        MeasurementDate = measurement.MeasurementDate,
                        Points = pointsRecord.Points
                    };
                    measurementsDto.Add(measurementDto);
                }

                BatterySearchResponseDTO response = new BatterySearchResponseDTO
                {
                    battery = batteryDto,
                    measurements = measurementsDto
                };

                return ResultService<BatterySearchResponseDTO>.Ok(200, response);
            }
            catch(Exception ex)
            {
                return ResultService<BatterySearchResponseDTO>.Fail(500, Activator.CreateInstance<BatterySearchResponseDTO>(), "Error interno del servidor, vuelva a intentarlo. " + ex.Message);
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
                    Battery? batteryFound = (await _batterySqlGenericRepository.GetAsync(a => a.ChipId == rawDataDTO.ChipId)).FirstOrDefault();
                    if (batteryFound == null)
                    {
                        Battery battery = new Battery
                        {
                            ChipId = rawDataDTO.ChipId,
                            WorkOrder = null,
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
