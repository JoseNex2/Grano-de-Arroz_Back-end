using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO
{
    public class ReportUpdateDTO
    {
        public int ReportId { get; set; }
        public List<MeasurementUpdateDTO> MeasurementsState { get; set; }
        public string ReportState { get; set; }
    }
}
