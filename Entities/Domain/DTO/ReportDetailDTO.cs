using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO
{
    public class ReportDetailDTO
    {
        public int Id { get; set; }
        public string ChipId { get; set; }
        public string ReportState { get; set; }
        public DateOnly ReportDate { get; set; }


        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }


        public string BatteryType { get; set; }
        public string? BatteryWorkOrder { get; set; }
        public DateTime? SaleDate { get; set; }
        public DateTime DateRegistered { get; set; }


        public List<MeasurementDTO> Measurements { get; set; } = new List<MeasurementDTO>();
    }
}
