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
        Task<ResultService<ReportDetailDTO>> ReportGetByIdAsync(int id);
        Task<ResultService<IEnumerable<ReportSearchDTO>>> ReportsSearchAsync(ReportSearchFilter filter);
        Task<ResultService<ReportViewDTO>> ReportCreate(BatteryReviewRequest reportRequest);
        Task<ResultService<ReportViewDTO>> UpdateMeasurementReportAsync(ReportUpdateDTO update);
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

        public async Task<ResultService<ReportViewDTO>> ReportCreate(BatteryReviewRequest reportRequest)
        {
            try
            {
                var batteryExist = (await _batterySqlGenericRepository.GetAsync(b => b.ChipId == reportRequest.ChipId)).FirstOrDefault();

                if (batteryExist == null)
                    return ResultService<ReportViewDTO>.Fail(404, Activator.CreateInstance<ReportViewDTO>(), "La batería no existe.");

                if (batteryExist.Report != null)
                    return ResultService<ReportViewDTO>.Fail(409, Activator.CreateInstance<ReportViewDTO>(), "Ya se hizo un reporte de la batería.");

                var reportModel = new Report
                {
                    ReportState = "Pendiente",
                    ReportDate = DateTime.UtcNow,
                    BatteryId = batteryExist.Id
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
                    ReportState = reportModel.ReportState
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
                    r => r.Battery.Client
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
                    reports = reports.Where(r => r.ReportState == filter.State);
                }

                if (filter.ReportDate.HasValue)
                {
                    reports = reports.Where(r => DateOnly.FromDateTime(r.ReportDate) == filter.ReportDate.Value);
                }

                var reportsView = reports.Select(r => new ReportSearchDTO
                {
                    Id = r.Id,
                    ChipId = r.Battery.ChipId,
                    ClientName = r.Battery.Client.Name,
                    ReportState = r.ReportState,
                    ReportDate = DateOnly.FromDateTime(r.ReportDate)
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

                report.ReportState = update.ReportState;

                await _batterySqlGenericRepository.UpdateByEntityAsync(report.Battery);

                var dto = new ReportViewDTO
                {
                    Id = report.Id,
                    ChipId = report.Battery.ChipId,
                    ReportState = report.ReportState,
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
                    ChipId = report.Battery.ChipId,
                    ReportState = report.ReportState,
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
