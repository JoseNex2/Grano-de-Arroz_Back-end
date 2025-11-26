using DataAccess.Generic;
using Entities.DataContext;
using Entities.Domain;
using Entities.Domain.DTO;
using Entities.Domain.DTO.Response;
using Utilities;

namespace DataAccess
{
    public interface IReportService
    {
        Task<ResultHelper<ReportDetailDTO>> ReportGetByIdAsync(int id);
        Task<ResultHelper<IEnumerable<ReportSearchDTO>>> ReportsSearchAsync(ReportSearchFilterDTO filter);
        Task<ResultHelper<ReportViewDTO>> ReportCreate(BatteryReviewRequest reportRequest);
        Task<ResultHelper<ReportViewDTO>> UpdateMeasurementReportAsync(ReportUpdateDTO update);
        Task<ResultHelper<IEnumerable<ReportViewHistorical>>> GetReportHistory();
    }
    public class ReportService : IReportService 
    {
        private readonly ISqlGenericRepository<Battery, ServiceDbContext> _batterySqlGenericRepository;
        private readonly ISqlGenericRepository<Report, ServiceDbContext> _reportSqlGenericRepository;
        private readonly ISqlGenericRepository<Status, ServiceDbContext> _statusSqlGenericRepository;
        public ReportService(ISqlGenericRepository<Battery, ServiceDbContext> batterySqlGenericRepository,
            ISqlGenericRepository<Report, ServiceDbContext> reportSqlGenericRepository,
            ISqlGenericRepository<Status, ServiceDbContext> statusSqlGenericRepository)
        {
            _reportSqlGenericRepository = reportSqlGenericRepository;
            _batterySqlGenericRepository = batterySqlGenericRepository;
            _statusSqlGenericRepository = statusSqlGenericRepository;
        }

        public async Task<ResultHelper<ReportViewDTO>> ReportCreate(BatteryReviewRequest reportRequest)
        {
            try
            {
                var batteryExist = (await _batterySqlGenericRepository.GetAsync(b => b.ChipId == reportRequest.ChipId, r => r.Client)).FirstOrDefault();

                if (batteryExist == null)
                    return ResultHelper<ReportViewDTO>.Fail(404, Activator.CreateInstance<ReportViewDTO>(), "La batería no existe.");

                Report reportExist = (await _reportSqlGenericRepository.GetAsync(b => b.BatteryId == batteryExist.Id)).FirstOrDefault();

                if (reportExist != null)
                    return ResultHelper<ReportViewDTO>.Fail(409, Activator.CreateInstance<ReportViewDTO>(), "Ya se hizo un reporte de la batería.");

                var pendingStatus = (await _statusSqlGenericRepository.GetAsync(s => s.Name == "Pendiente")).FirstOrDefault();

                if (pendingStatus == null)
                    return ResultHelper<ReportViewDTO>.Fail(500, Activator.CreateInstance<ReportViewDTO>(), "El estado 'Pendiente' no existe en la base de datos.");


                var reportModel = new Report
                {
                    ReportDate = DateTime.UtcNow,
                    BatteryId = batteryExist.Id,
                    StatusId = pendingStatus.Id,
                };

                var id = await _reportSqlGenericRepository.CreateAsync(reportModel);

                if (id == null)
                    return ResultHelper<ReportViewDTO>.Fail(500, Activator.CreateInstance<ReportViewDTO>(), "Error al crear el reporte.");

                var reportView = new ReportViewDTO
                {
                    Id = id.Value,
                    ChipId = batteryExist.ChipId,
                    ClientId = batteryExist.ClientId.Value,
                    ReportDate = DateOnly.FromDateTime(reportModel.ReportDate),
                    ReportState = pendingStatus.Name,
                    Client = new ClientViewDTO
                    {
                        Id = batteryExist.Client.Id,
                        Name = batteryExist.Client.Name,
                        LastName = batteryExist.Client.LastName,
                        Email = batteryExist.Client.Email,
                        PhoneNumber = batteryExist.Client.PhoneNumber,
                        RegisteredDate = batteryExist.Client.RegisteredDate
                    }
                };

                return ResultHelper<ReportViewDTO>.Ok(201, reportView, "Reporte creado.");
            }
            catch (Exception ex)
            {
                return ResultHelper<ReportViewDTO>.Fail(500, Activator.CreateInstance<ReportViewDTO>(), ex.Message);
            }
        }
        public async Task<ResultHelper<IEnumerable<ReportSearchDTO>>> ReportsSearchAsync(ReportSearchFilterDTO filter)
        {
            try
            {
                var reports = await _reportSqlGenericRepository.GetAsync(
                    null,
                    r => r.Battery,
                    r => r.Battery.Client,
                    r => r.Status
                );

                if (!string.IsNullOrWhiteSpace(filter.ChipId))
                {
                    reports = reports.Where(r => r.Battery.ChipId.Contains(filter.ChipId, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filter.ClientName))
                {
                    reports = reports.Where(r => r.Battery.Client.Name.Contains(filter.ClientName, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filter.State))
                {
                    reports = reports.Where(r => r.Status.Name.Equals(filter.State, StringComparison.OrdinalIgnoreCase));
                }

                if (filter.ReportDate.HasValue)
                {
                    reports = reports.Where(r => DateOnly.FromDateTime(r.ReportDate) == filter.ReportDate.Value);
                }

                var reportsList = reports.ToList();

                var reportsView = reportsList.Select(r => new ReportSearchDTO
                {
                    Id = r.Id,
                    ChipId = r.Battery?.ChipId ?? string.Empty, 
                    ClientName = r.Battery?.Client?.Name ?? string.Empty, 
                    ReportState = r.Status?.Name ?? string.Empty, 
                    ReportDate = r.ReportDate != DateTime.MinValue ?
                    DateOnly.FromDateTime(r.ReportDate) : DateOnly.MinValue
                });

                return ResultHelper<IEnumerable<ReportSearchDTO>>.Ok(200, reportsView);
            }
            catch (Exception ex)
            {
                return ResultHelper<IEnumerable<ReportSearchDTO>>.Fail(500, Activator.CreateInstance<IEnumerable<ReportSearchDTO>>(), ex.Message);
            }
        }
        public async Task<ResultHelper<IEnumerable<ReportViewHistorical>>> GetReportHistory() 
        {
            try
            {
                var reports = await _reportSqlGenericRepository.GetAsync(
                    null,
                    r => r.Battery,
                    r => r.Battery.Client,
                    r => r.Status
                );

                var reportsList = reports.ToList();

                var reportsView = reportsList.Select(r => new ReportViewHistorical
                {
                    Id = r.Id,
                    ChipId = r.Battery.ChipId, 
                    ClientName = r.Battery.Client.Name,  
                    ReportState = r.Status?.Name ?? string.Empty, 
                    TypeBattery = r.Battery?.Type ?? string.Empty,
                    ReportDate = r.ReportDate != DateTime.MinValue ?
                    DateOnly.FromDateTime(r.ReportDate) : DateOnly.MinValue
                });

                return ResultHelper<IEnumerable<ReportViewHistorical>>.Ok(200, reportsView);
            }
            catch (Exception ex)
            {
                return ResultHelper<IEnumerable<ReportViewHistorical>>.Fail(500, Activator.CreateInstance<IEnumerable<ReportViewHistorical>>(), ex.Message);
            }   

        }

        public async Task<ResultHelper<ReportViewDTO>> UpdateMeasurementReportAsync(ReportUpdateDTO update)
        {
            try
            {
                var report = (await _reportSqlGenericRepository.GetAsync(
                    r => r.Id == update.ReportId,
                    r => r.Battery
                )).FirstOrDefault();

                if (report == null)
                    return ResultHelper<ReportViewDTO>.Fail(404, Activator.CreateInstance<ReportViewDTO>(), "Reporte no encontrado.");

                if(report.MeasurementsStatus.Count > 0)
                    return ResultHelper<ReportViewDTO>.Fail(404, Activator.CreateInstance<ReportViewDTO>(), "El status de una magnitud ya fue cargada.");

                foreach (var mUpdate in update.MeasurementsState)
                {
                    var newMeasurementStatus = (await _statusSqlGenericRepository.GetAsync(s => s.Name == mUpdate.Status)).FirstOrDefault();

                    if (newMeasurementStatus == null)
                        return ResultHelper<ReportViewDTO>.Fail(400, Activator.CreateInstance<ReportViewDTO>(), $"El estado '{mUpdate.Status}' no existe.");

                    report.MeasurementsStatus.Add(new MeasurementStatus
                    {
                        MeasurementId = mUpdate.MeasurementId,
                        StatusId = newMeasurementStatus.Id,
                        Coment = mUpdate.Coment,
                        ReportId = update.ReportId
                    });
                }

                var newStatus = (await _statusSqlGenericRepository.GetAsync(s => s.Name == update.ReportState)).FirstOrDefault();

                if (newStatus == null)
                    return ResultHelper<ReportViewDTO>.Fail(400, Activator.CreateInstance<ReportViewDTO>(), $"El estado '{update.ReportState}' no existe.");

                report.StatusId = newStatus.Id;

                await _reportSqlGenericRepository.UpdateByEntityAsync(report);

                var dto = new ReportViewDTO
                {
                    Id = report.Id,
                    ChipId = report.Battery.ChipId,
                    ReportState = report.Status.Name,
                    ReportDate = DateOnly.FromDateTime(report.ReportDate)
                };

                return ResultHelper<ReportViewDTO>.Ok(200, dto, "Reporte actualizado con mediciones.");
            }
            catch (Exception ex)
            {
                return ResultHelper<ReportViewDTO>.Fail(500, Activator.CreateInstance<ReportViewDTO>(), ex.Message);
            }
        }

        public async Task<ResultHelper<ReportDetailDTO>> ReportGetByIdAsync(int reportId)
        {
            try
            {
                var report = (await _reportSqlGenericRepository.GetWithStringIncludesAsync(
                    r => r.Id == reportId,
                    "Battery",
                    "Battery.Client",
                    "Battery.Measurements",
                    "Status",
                    "MeasurementsStatus",
                    "MeasurementsStatus.Status"
                )).FirstOrDefault();

                if (report == null)
                    return ResultHelper<ReportDetailDTO>.Fail(404, new ReportDetailDTO(), "Reporte no encontrado.");

                var measurementStatuses = report.MeasurementsStatus ?? new List<MeasurementStatus>();
                var batteryMeasurements = report.Battery?.Measurements ?? new List<Measurement>();

                var measurementsDto = new List<MeasurementReportDTO>();

                foreach (var ms in measurementStatuses)
                {
                    var measurement = batteryMeasurements.FirstOrDefault(m => m.Id == ms.MeasurementId);
                    if (measurement == null)
                        continue;

                    measurementsDto.Add(new MeasurementReportDTO
                    {
                        Id = measurement.Id,
                        Magnitude = measurement.Magnitude,
                        Status = ms.Status?.Name ?? "Sin estado",
                        Coment = ms.Coment,
                        MeasurementDate = measurement.MeasurementDate
                    });
                }

                var reportDetail = new ReportDetailDTO
                {
                    Id = report.Id,
                    ChipId = report.Battery?.ChipId ?? "Desconocido",
                    ReportState = report.Status?.Name ?? "Sin estado",
                    ReportDate = DateOnly.FromDateTime(report.ReportDate),

                    ClientId = report.Battery?.Client?.Id ?? 0,
                    ClientName = report.Battery?.Client?.Name ?? "Desconocido",
                    ClientEmail = report.Battery?.Client?.Email ?? "Desconocido",

                    BatteryType = report.Battery?.Type ?? "N/A",
                    BatteryWorkOrder = report.Battery?.WorkOrder ?? "N/A",
                    SaleDate = report.Battery?.SaleDate,
                    RegisteredDate = report.Battery?.RegisteredDate ?? DateTime.MinValue,
                    
                    Measurements = measurementsDto
                };

                return ResultHelper<ReportDetailDTO>.Ok(200, reportDetail, "Detalle del reporte obtenido.");
            }
            catch (Exception ex)
            {
                return ResultHelper<ReportDetailDTO>.Fail(500, new ReportDetailDTO(), $"Error interno: {ex.Message}");
            }
        }


    }
}
