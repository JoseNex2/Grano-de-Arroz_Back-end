using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO
{
    public class BatterySearchOptionalDTO
    {
        public int Id { get; set; }
        public string BatteryGDA { get; set; }
        public DateOnly DateSolicitud { get; set; }
        public string ClientName { get; set; }
    }
}
