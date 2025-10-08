using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace Entities.Domain.DTO
{
    public class BatteryViewDTO
    {
        public int Id { get; set; }
        public string ChipId { get; set; }
        public string? WorkOrder { get; set; }
        public string Type { get; set; }
        public DateTime? SaleDate { get; set; }
        //public ClientViewDTO? Client { get; set; }
    }
}
