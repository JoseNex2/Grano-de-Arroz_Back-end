using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO
{
    public class MeasurementReportDTO
    {
        public int Id { get; set; }
        public string Magnitude { get; set; }
        public string Status { get; set; }
        public string Coment { get; set; }
        public DateTime MeasurementDate { get; set; }
    }
}
