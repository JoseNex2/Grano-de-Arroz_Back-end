using System.Linq;
using System.Reactive.Joins;
using DataAccess.Generic;
using Entities.DataContext;
using Entities.Domain;
using Entities.Domain.DTO;
using Entities.Domain.DTO.Response;
using MongoDB.Driver;
using Utilities;

namespace DataAccess
{
    public interface IBatteryService
    {
        Task<ResultService<BatteryViewDTO>> BatteryRegister(BatteryDTO batteryDTO);
        Task<ResultService<BatteriesSearchResponseDTO>> BatteriesSearch();
        Task<ResultService<BatteriesSearchResponseDTO>> BatteriesSearchWithFilter(BatterySearchFilterDTO filter);
        Task<ResultService<BatterySearchResponseDTO>> BatterySearchWithId(int id);
        Task<ResultService<IEnumerable<BatteryByClientResponse>>> BatteriesSearchByClient(int ClientId);
        Task<ResultService<BatteryAnalysisPercentageResponse>> GetBatteryAnalysisPercentageAsync();
        Task<ResultService<BatteryMetricsPercentageResponse>> GetBatteryMetricsPercentageAsync();
    }


    public class BatteryService : IBatteryService
    {
        private readonly ISqlGenericRepository<Battery, ServiceDbContext> _batterySqlGenericRepository;
        private readonly ISqlGenericRepository<Measurement, ServiceDbContext> _measurementSqlGenericRepository;
        private readonly ISqlGenericRepository<Report, ServiceDbContext> _reportSqlGenericRepository;
        private readonly INonSqlGenericRepository<MetricsRecord> _nonSqlGenericRepository;
        private string statusNotInit = "No iniciada";

        public BatteryService(ISqlGenericRepository<Battery, ServiceDbContext> batterySqlGenericRepository, 
            ISqlGenericRepository<Measurement, ServiceDbContext> measurementSqlGenericRepository,
            ISqlGenericRepository<Report, ServiceDbContext> reportSqlGenericRepository,
            INonSqlGenericRepository<MetricsRecord> nonSqlGenericRepository)
        {
            _batterySqlGenericRepository = batterySqlGenericRepository;
            _measurementSqlGenericRepository = measurementSqlGenericRepository;
            _reportSqlGenericRepository = reportSqlGenericRepository;
            _nonSqlGenericRepository = nonSqlGenericRepository;
        }
        public async Task<ResultHelper<BatteryViewDTO>> BatteryRegister(BatteryDTO batteryDTO)
        {
            try
            {
                bool estado = false;
                Battery? batteryFound = (await _batterySqlGenericRepository.GetAsync(a => a.ChipId == batteryDTO.ChipId)).FirstOrDefault();
                if (batteryFound == null)
                {
                    return ResultHelper<BatteryViewDTO>.Fail(409, Activator.CreateInstance<BatteryViewDTO>(), "No se encuentra la bateria registrada.");
                }

                if (batteryFound.WorkOrder != null && batteryFound.SaleDate != null && batteryFound.ClientId != null)
                {
                    return ResultHelper<BatteryViewDTO>.Fail(409, Activator.CreateInstance<BatteryViewDTO>(), "La bateria ya se encuentra asociada a un cliente.");
                }

                batteryFound.WorkOrder = batteryDTO.WorkOrder;
                batteryFound.SaleDate = batteryDTO.SaleDate;
                batteryFound.ClientId = batteryDTO.ClientId;
                estado = await _batterySqlGenericRepository.UpdateByEntityAsync(batteryFound);

                if (estado == true)
                {
                    return ResultHelper<BatteryViewDTO>.Ok(201, Activator.CreateInstance<BatteryViewDTO>(), "Bateria asociada al cliente.");
                }
                else
                {
                    return ResultHelper<BatteryViewDTO>.Fail(500, Activator.CreateInstance<BatteryViewDTO>(), "Error al asociar la bateria.");
                }
            }
            catch (Exception ex)
            {
                return ResultHelper<BatteryViewDTO>.Fail(500, Activator.CreateInstance<BatteryViewDTO>(), ex.Message);

            }
        }
        public async Task<ResultHelper<BatteriesSearchResponseDTO>> BatteriesSearch()
        {
            try
            {
                var batteries = await _batterySqlGenericRepository.GetAsync(includes: b => b.Client);

                var batteriesDTO = batteries.Select(b => new BatteryViewDTO
                {
                    Id = b.Id,
                    ChipId = b.ChipId,
                    WorkOrder = b.WorkOrder,
                    Type = b.Type,
                    SaleDate = b.SaleDate,
                    Client = b.Client == null ? null : new ClientViewDTO
                    {
                        Id = b.Client.Id,
                        Name = b.Client.Name,
                        LastName = b.Client.LastName,
                        NationalId = b.Client.NationalId,
                        Email = b.Client.Email,
                        PhoneNumber = b.Client.PhoneNumber,
                        RegisteredDate = b.Client.RegisteredDate,
                    }
                }).ToList();
                BatteriesSearchResponseDTO response = new BatteriesSearchResponseDTO
                {
                    TotalBatteries = batteriesDTO.Count,
                    Batteries = batteriesDTO
                };

                return ResultHelper<BatteriesSearchResponseDTO>.Ok(200, response);
            }
            catch (Exception ex)
            {
                return ResultHelper<BatteriesSearchResponseDTO>.Fail(500, Activator.CreateInstance<BatteriesSearchResponseDTO>(), "Error interno del servidor, vuelva a intentarlo. " + ex.Message);
            }
        }

        public async Task<ResultHelper<BatteriesSearchResponseDTO>> BatteriesSearchWithFilter(BatterySearchFilterDTO filter)
        {
            try
            {
                var batteries = await _batterySqlGenericRepository.GetAsync(
                    b => b.Client != null,
                    b => b.Client,
                    b => b.Report,
                    b => b.Report.Status
                );

                if (!string.IsNullOrWhiteSpace(filter.ChipId))
                {
                    batteries = batteries.Where(b =>
                        b.ChipId.Contains(filter.ChipId, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filter.ClientName))
                {
                    batteries = batteries.Where(b =>
                        b.Client.Name.Contains(filter.ClientName, StringComparison.OrdinalIgnoreCase));
                }

                if (filter.SaleDate.HasValue)
                {
                    batteries = batteries.Where(b =>
                        b.SaleDate.HasValue &&
                        DateOnly.FromDateTime(b.SaleDate.Value) == filter.SaleDate.Value);
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
                    else
                    {
                        reportValid = report.Status?.Name ?? "Pendiente";
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
                            RegisteredDate = battery.Client.RegisteredDate,
                        }
                    };
                    batteriesDTO.Add(batteryDTO);

                }

                var response = new BatteriesSearchResponseDTO
                {
                    TotalBatteries = batteriesDTO.Count,
                    Batteries = batteriesDTO
                };

                return ResultHelper<BatteriesSearchResponseDTO>.Ok(200, response);
            }
            catch (Exception ex)
            {
                return ResultHelper<BatteriesSearchResponseDTO>.Fail(
                    500,
                    Activator.CreateInstance<BatteriesSearchResponseDTO>(),
                    "Error interno del servidor, vuelva a intentarlo. " + ex.Message
                );
            }
        }


        public async Task<ResultHelper<BatterySearchResponseDTO>> BatterySearchWithId(int id)
        {
            try
            {
                var batteryFound = (await _batterySqlGenericRepository.GetAsync(r => r.Id == id, r => r.Measurements)).FirstOrDefault();

                if (batteryFound == null)
                {
                    return ResultHelper<BatterySearchResponseDTO>.Fail(
                        409, 
                        Activator.CreateInstance<BatterySearchResponseDTO>(), 
                        "No se encuentra la bateria registrada.");
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

                foreach (Measurement measurement in batteryFound.Measurements)
                {
                    MetricsRecord? metricsRecord = (await _nonSqlGenericRepository.GetByParameterAsync(a => a.Id == measurement.Id)).FirstOrDefault();

                    MeasurementDTO measurementDto = new MeasurementDTO
                    {
                        Id = measurement.Id,
                        Magnitude = measurement.Magnitude,
                        MeasurementDate = DateOnly.FromDateTime(measurement.MeasurementDate),
                        Metrics = metricsRecord?.Metrics != null ? metricsRecord.Metrics.ToDictionary(kvp => TimeOnly.Parse(kvp.Key), kvp => kvp.Value) : new Dictionary<TimeOnly, double>()
                    };
                    measurementsDto.Add(measurementDto);
                }

                BatterySearchResponseDTO response = new BatterySearchResponseDTO
                {
                    battery = batteryDto,
                    measurements = measurementsDto
                };

                return ResultHelper<BatterySearchResponseDTO>.Ok(200, response);
            }
            catch(Exception ex)
            {
                return ResultHelper<BatterySearchResponseDTO>.Fail(
                    500, 
                    Activator.CreateInstance<BatterySearchResponseDTO>(), 
                    "Error interno del servidor, vuelva a intentarlo. " + ex.Message);
            }
        }

        public async Task<ResultHelper<IEnumerable<BatteryByClientResponse>>> BatteriesSearchByClient(int ClientId)
        {
            try
            {
                var batteries = await _batterySqlGenericRepository.GetAsync(
                    b => b.ClientId == ClientId,
                    b => b.Report,
                    b => b.Report.Status
                );

                var batteriesDTO = batteries.Select(b => new BatteryByClientResponse
                {
                    Id = b.Id,
                    ChipId = b.ChipId,
                    WorkOrder = b.WorkOrder,
                    Status = b.Report != null ? (b.Report.Status.Name) : statusNotInit.ToString(),
                    SaleDate = b.SaleDate

                }).ToList();

                return ResultHelper<IEnumerable<BatteryByClientResponse>>.Ok(200, batteriesDTO);
            }
            catch (Exception ex)
            {
                return ResultHelper<IEnumerable<BatteryByClientResponse>>.Fail(
                    500, 
                    Activator.CreateInstance<IEnumerable<BatteryByClientResponse>>(), 
                    "Error interno del servidor, vuelva a intentarlo. " + ex.Message);
            }
        }

        public async Task<ResultHelper<BatteryAnalysisPercentageResponse>> GetBatteryAnalysisPercentageAsync()
        {
            try
            {
                const int EstadoAprobadoId = 1;
                const int EstadoDesaprobadoId = 2;

                var batteries = await _batterySqlGenericRepository.GetAsync(
                    b => b.Report != null,
                    b => b.Report,
                    b => b.Report.Status
                );

                if (batteries == null || !batteries.Any())
                {
                    return ResultHelper<BatteryAnalysisPercentageResponse>.Fail(
                        404,
                        new BatteryAnalysisPercentageResponse(),
                        "No hay baterías evaluadas."
                    );
                }

                var batteriesEvaluated = batteries
                    .Where(b => b.Report?.Status != null &&
                                (b.Report.Status.Id == EstadoAprobadoId || b.Report.Status.Id == EstadoDesaprobadoId))
                    .ToList();

                if (!batteriesEvaluated.Any())
                {
                    return ResultHelper<BatteryAnalysisPercentageResponse>.Fail(
                        404,
                        new BatteryAnalysisPercentageResponse(),
                        "No hay baterías con reportes válidos (Aprobado/Desaprobado)."
                    );
                }

                int totalEvaluated = batteriesEvaluated.Count;
                int approved = batteriesEvaluated.Count(b => b.Report.Status.Id == EstadoAprobadoId);
                int rejected = batteriesEvaluated.Count(b => b.Report.Status.Id == EstadoDesaprobadoId);

                var result = new BatteryAnalysisPercentageResponse
                {
                    ApprovedPercentage = totalEvaluated > 0 ?
                        Math.Round((double)approved / totalEvaluated * 100, 2) : 0,
                    RejectedPercentage = totalEvaluated > 0 ?
                        Math.Round((double)rejected / totalEvaluated * 100, 2) : 0
                };

                return ResultHelper<BatteryAnalysisPercentageResponse>.Ok(200, result);
            }
            catch (Exception ex)
            {
                return ResultHelper<BatteryAnalysisPercentageResponse>.Fail(
                    500,
                    new BatteryAnalysisPercentageResponse(),
                    "Error interno. " + ex.Message
                );
            }
        }

        public async Task<ResultService<BatteryMetricsPercentageResponse>> GetBatteryMetricsPercentageAsync()
        {
            try
            {
                var batteries = await _batterySqlGenericRepository.GetAsync(
                    null,
                    b => b.Client,
                    b => b.Report
                );

                if (batteries == null || !batteries.Any())
                {
                    return ResultService<BatteryMetricsPercentageResponse>.Fail(
                        404,
                        new BatteryMetricsPercentageResponse(),
                        "No hay baterías registradas."
                    );
                }

                var soldBatteries = batteries
                    .Where(b => b.ClientId != null)
                    .ToList();

                var soldWithReport = soldBatteries
                    .Where(b => b.Report != null)
                    .ToList();

                int total = batteries.Count();
                int totalSold = soldBatteries.Count();
                int totalSoldWithReport = soldWithReport.Count();

                var result = new BatteryMetricsPercentageResponse
                {
                    SoldPercentage = total > 0 ?
                        Math.Round((double)totalSold / total * 100, 2) : 0,

                    SoldWithReportPercentage = totalSold > 0 ?
                        Math.Round((double)totalSoldWithReport / totalSold * 100, 2) : 0
                };

                return ResultService<BatteryMetricsPercentageResponse>.Ok(200, result);
            }
            catch (Exception ex)
            {
                return ResultService<BatteryMetricsPercentageResponse>.Fail(
                    500,
                    new BatteryMetricsPercentageResponse(),
                    "Error interno. " + ex.Message
                );
            }
        }

    }

    /*public async Task<ResultService<RawDataResponseDTO>> UploadRawData(RawDataDTO rawDataDTO)
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
                        MetricsRecord pointsRecord = new MetricsRecord
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
                            MetricsRecord pointsRecord = new MetricsRecord
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
    }*/
}

