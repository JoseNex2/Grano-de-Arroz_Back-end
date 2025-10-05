using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO
{
    public class BatteryDTO
    {
        public string ChipId { get; set; }
        public string WorkOrder { get; set; }
        public DateTime SaleDate { get; set; } 
        public int ClientId { get; set; }
    }
}
