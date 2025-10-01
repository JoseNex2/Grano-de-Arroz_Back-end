using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Generic;
using Entities.DataContext;
using Entities.Domain;
using Entities.Domain.DTO;
using Entities.Domain.DTO.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using MySqlX.XDevAPI.Common;
using Sprache;
using Utilities;

namespace DataAccess
{
    public interface IReportService
    {
        Task<ResultService<ReportDetailDTO>> GetReportByIdAsync(int id);
        Task<ResultService<IEnumerable<ReportSearchDTO>>> SearchReportsAsync(ReportSearchFilter filter);
        Task<ResultService<ReportViewDTO>> CreateReport(BatteryReviewRequest reportRequest);
        Task<ResultService<ReportViewDTO>> UpdateReportMeasurementsAsync(ReportUpdateDTO update);
    }
    public class ReportService : IReportService 
    {
        private readonly ISqlGenericRepository<Battery, ServiceDbContext> _batterySqlGenericRepository;
        private readonly ISqlGenericRepository<Report, ServiceDbContext> _reportSqlGenericRepository;
        public ReportService(ISqlGenericRepository<Battery, ServiceDbContext> batterySqlGenericRepository,
            ISqlGenericRepository<Report, ServiceDbContext> reportSqlGenericRepository,
            ISqlGenericRepository<Client, ServiceDbContext> clientSqlGenericRepository)
        {
            _reportSqlGenericRepository = reportSqlGenericRepository;
            _batterySqlGenericRepository = batterySqlGenericRepository;
        }

        public async Task<ResultService<ReportViewDTO>> CreateReport(BatteryReviewRequest reportRequest)
        {
            try
            {
                var batteryExists = (await _batterySqlGenericRepository
                    .GetAsync(b => b.BatteryGDA == reportRequest.BatteryGDA))
                    .Any();

                if (!batteryExists)
                    return ResultService<ReportViewDTO>.Fail(404, Activator.CreateInstance<ReportViewDTO>(), "La batería no existe.");

                var existingReport = (await _reportSqlGenericRepository
                    .GetAsync(r => r.BatteryGDA == reportRequest.BatteryGDA))
                    .FirstOrDefault();

                if (existingReport != null)
                    return ResultService<ReportViewDTO>.Fail(409, Activator.CreateInstance<ReportViewDTO>(), "Ya se hizo un reporte de la batería.");

                var reportModel = new Report
                {
                    BatteryGDA = reportRequest.BatteryGDA,
                    ReportState = "Pendiente",
                    ReportDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    ClientId = reportRequest.ClientId
                };

                var id = await _reportSqlGenericRepository.CreateAsync(reportModel);

                if (id == null)
                    return ResultService<ReportViewDTO>.Fail(500, Activator.CreateInstance<ReportViewDTO>(), "Error al crear el reporte.");

                var reportView = new ReportViewDTO
                {
                    Id = id.Value,
                    BatteryGDA = reportModel.BatteryGDA,
                    ClientId = reportModel.ClientId,
                    ReportDate = reportModel.ReportDate,
                    ReportState = reportModel.ReportState
                };

                return ResultService<ReportViewDTO>.Ok(201, reportView, "Reporte creado.");
            }
            catch (Exception ex)
            {
                return ResultService<ReportViewDTO>.Fail(500, Activator.CreateInstance<ReportViewDTO>(), ex.Message);
            }
        }
        public async Task<ResultService<IEnumerable<ReportSearchDTO>>> SearchReportsAsync(ReportSearchFilter filter)
        {
            try
            {
                var reports = await _reportSqlGenericRepository.GetAllAsync(
                    null,
                    r => r.Client
                );

                if (!string.IsNullOrWhiteSpace(filter.BatteryGDA))
                {
                    reports = reports.Where(r => r.BatteryGDA.Contains(filter.BatteryGDA, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filter.ClientName))
                {
                    reports = reports.Where(r => r.Client.Name.Contains(filter.ClientName, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(filter.State))
                {
                    reports = reports.Where(r => r.ReportState == filter.State);
                }

                if (filter.ReportDate.HasValue)
                {
                    reports = reports.Where(r => r.ReportDate == filter.ReportDate.Value);
                }

                var reportsView = reports.Select(r => new ReportSearchDTO
                {
                    Id = r.Id,
                    BatteryGDA = r.BatteryGDA,
                    ClientName = r.Client.Name,
                    ReportState = r.ReportState,
                    ReportDate = r.ReportDate
                });

                return ResultService<IEnumerable<ReportSearchDTO>>.Ok(200, reportsView);
            }
            catch (Exception ex)
            {
                return ResultService<IEnumerable<ReportSearchDTO>>.Fail(500, Activator.CreateInstance<IEnumerable<ReportSearchDTO>>(), ex.Message);
            }
        }

        public async Task<ResultService<ReportViewDTO>> UpdateReportMeasurementsAsync(ReportUpdateDTO update)
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

                report.ReportState = update.ReportState;

                await _batterySqlGenericRepository.UpdateByEntityAsync(report.Battery);

                var dto = new ReportViewDTO
                {
                    Id = report.Id,
                    BatteryGDA = report.BatteryGDA,
                    ReportState = report.ReportState,
                    ReportDate = report.ReportDate
                };

                return ResultService<ReportViewDTO>.Ok(200, dto, "Reporte actualizado con mediciones.");
            }
            catch (Exception ex)
            {
                return ResultService<ReportViewDTO>.Fail(500, Activator.CreateInstance<ReportViewDTO>(), ex.Message);
            }
        }
        public async Task<ResultService<ReportDetailDTO>> GetReportByIdAsync(int reportId)
        {
            try
            {
                var report = (await _reportSqlGenericRepository.GetAsync(
                    r => r.Id == reportId,
                    r => r.Client,
                    r => r.Battery,
                    r => r.Battery.Measurements
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
                    BatteryGDA = report.BatteryGDA,
                    ReportState = report.ReportState,
                    ReportDate = report.ReportDate,

                    ClientId = report.ClientId,
                    ClientName = report.Client.Name,
                    ClientEmail = report.Client.Email,

                    BatteryType = report.Battery.Type,
                    BatteryOt = report.Battery.Ot,
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
