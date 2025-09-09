using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO
{
    public class BatteryDTO
    {
        public string ID_Chip { get; set; }
        public string OT { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime SaleDate { get; set; } 
        public int ClientID { get; set; }
    }
}
