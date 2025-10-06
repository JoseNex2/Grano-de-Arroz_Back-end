using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain
{
    public class Measurement
    {
        public int Id { get; set; }
        public string Magnitude { get; set; }
        public string? Status {  get; set; } 
        public string? Coment {  get; set; }
        public DateTime MeasurementDate { get; set; }
        public int BatteryId { get; set; }
        public Battery Battery { get; set; }
    }
}
