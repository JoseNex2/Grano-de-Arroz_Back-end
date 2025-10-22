using DataAccess.Generic;
using Entities.DataContext;
using Entities.Domain;
using Entities.Domain.DTO;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Sprache;
using Utilities;

namespace DataAccess
{
    public interface IReportService
    {
        Task<ResultService<ReportDetailDTO>> ReportGetByIdAsync(int id);
        Task<ResultService<IEnumerable<ReportSearchDTO>>> ReportsSearchAsync(ReportSearchFilter filter);
        Task<ResultService<ReportViewDTO>> ReportCreate(BatteryReviewRequest reportRequest);
        Task<ResultService<ReportViewDTO>> UpdateMeasurementReportAsync(ReportUpdateDTO update);
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

        public async Task<ResultService<ReportViewDTO>> ReportCreate(BatteryReviewRequest reportRequest)
        {
            try
            {
                var batteryExist = (await _batterySqlGenericRepository.GetAsync(b => b.ChipId == reportRequest.ChipId, r => r.Client)).FirstOrDefault();

                if (batteryExist == null)
                    return ResultService<ReportViewDTO>.Fail(404, Activator.CreateInstance<ReportViewDTO>(), "La batería no existe.");

                var reportExist = (await _reportSqlGenericRepository.GetAsync(b => b.BatteryId == batteryExist.Id));

                if (reportExist != null)
                    return ResultService<ReportViewDTO>.Fail(409, Activator.CreateInstance<ReportViewDTO>(), "Ya se hizo un reporte de la batería.");

                var pendingStatus = (await _statusSqlGenericRepository.GetAsync(s => s.Name == "Pendiente")).FirstOrDefault();

                if (pendingStatus == null)
                    return ResultService<ReportViewDTO>.Fail(500, Activator.CreateInstance<ReportViewDTO>(), "El estado 'Pendiente' no existe en la base de datos.");


                var reportModel = new Report
                {
                    ReportDate = DateTime.UtcNow,
                    BatteryId = batteryExist.Id,
                    StatusId = pendingStatus.Id,
                };

                var id = await _reportSqlGenericRepository.CreateAsync(reportModel);

                if (id == null)
                    return ResultService<ReportViewDTO>.Fail(500, Activator.CreateInstance<ReportViewDTO>(), "Error al crear el reporte.");

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
                        DateRegistered = batteryExist.Client.DateRegistered,
                    }
                };

                return ResultService<ReportViewDTO>.Ok(201, reportView, "Reporte creado.");
            }
            catch (Exception ex)
            {
                return ResultService<ReportViewDTO>.Fail(500, Activator.CreateInstance<ReportViewDTO>(), ex.Message);
            }
        }
        public async Task<ResultService<IEnumerable<ReportSearchDTO>>> ReportsSearchAsync(ReportSearchFilter filter)
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

                return ResultService<IEnumerable<ReportSearchDTO>>.Ok(200, reportsView);
            }
            catch (Exception ex)
            {
                return ResultService<IEnumerable<ReportSearchDTO>>.Fail(500, Activator.CreateInstance<IEnumerable<ReportSearchDTO>>(), ex.Message);
            }
        }

        public async Task<ResultService<ReportViewDTO>> UpdateMeasurementReportAsync(ReportUpdateDTO update)
        {
            try
            {
                var report = (await _reportSqlGenericRepository.GetAsync(
                    r => r.Id == update.ReportId,
                    r => r.Battery,
                    r => r.Battery.Measurements
                )).FirstOrDefault();

                if (report == null)
                    return ResultService<ReportViewDTO>.Fail(404, Activator.CreateInstance<ReportViewDTO>(), "Reporte no encontrado.");

                foreach (var mUpdate in update.Measurements)
                {
                    var measurement = report.Battery.Measurements.FirstOrDefault(m => m.Id == mUpdate.MeasurementId);
                    if (measurement != null)
                    {
                        measurement.Status = mUpdate.Status;
                        measurement.Coment = mUpdate.Coment;
                    }
                }

                var newStatus = (await _statusSqlGenericRepository.GetAsync(s => s.Name == update.ReportState)).FirstOrDefault();

                if (newStatus == null)
                    return ResultService<ReportViewDTO>.Fail(400, Activator.CreateInstance<ReportViewDTO>(), $"El estado '{update.ReportState}' no existe.");

                report.StatusId = newStatus.Id;

                await _batterySqlGenericRepository.UpdateByEntityAsync(report.Battery);
                await _reportSqlGenericRepository.UpdateByEntityAsync(report);

                var dto = new ReportViewDTO
                {
                    Id = report.Id,
                    ChipId = report.Battery.ChipId,
                    ReportState = report.Status.Name,
                    ReportDate = DateOnly.FromDateTime(report.ReportDate)
                };

                return ResultService<ReportViewDTO>.Ok(200, dto, "Reporte actualizado con mediciones.");
            }
            catch (Exception ex)
            {
                return ResultService<ReportViewDTO>.Fail(500, Activator.CreateInstance<ReportViewDTO>(), ex.Message);
            }
        }
        public async Task<ResultService<ReportDetailDTO>> ReportGetByIdAsync(int reportId)
        {
            try
            {
                var report = (await _reportSqlGenericRepository.GetAsync(
                    r => r.Id == reportId,
                    r => r.Battery.Client,
                    r => r.Battery,
                    r => r.Battery.Measurements,
                    r => r.Status
                )).FirstOrDefault();

                if (report == null)
                    return ResultService<ReportDetailDTO>.Fail(404, new ReportDetailDTO(), "Reporte no encontrado.");

                var measurementsDto = report.Battery.Measurements
                    .Select(m => new MeasurementDTO
                    {
                        Id = m.Id,
                        Magnitude = m.Magnitude,
                        Status = m.Status,
                        Coment = m.Coment,
                        MeasurementDate = m.MeasurementDate
                    })
                    .ToList();

                var reportDetail = new ReportDetailDTO
                {
                    Id = report.Id,
                    ChipId = report.Battery.ChipId,
                    ReportState = report.Status.Name,
                    ReportDate = DateOnly.FromDateTime(report.ReportDate),

                    ClientId = report.Battery.Client.Id,
                    ClientName = report.Battery.Client.Name,
                    ClientEmail = report.Battery.Client.Email,

                    BatteryType = report.Battery.Type,
                    BatteryWorkOrder = report.Battery.WorkOrder,
                    SaleDate = report.Battery.SaleDate,
                    DateRegistered = report.Battery.DateRegistered,

                    Measurements = measurementsDto
                };

                return ResultService<ReportDetailDTO>.Ok(200, reportDetail, "Detalle del reporte obtenido.");
            }
            catch (Exception ex)
            {
                return ResultService<ReportDetailDTO>.Fail(500, new ReportDetailDTO(), ex.Message);
            }
        }

    }
}
