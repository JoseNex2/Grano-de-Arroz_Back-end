using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO
{
    public class MeasurementDTO
    {
        public int Id { get; set; }
        public string Magnitude { get; set; }
        public DateOnly MeasurementDate { get; set; }
        public Dictionary<TimeOnly, double> Metrics { get; set; }
    }
}
