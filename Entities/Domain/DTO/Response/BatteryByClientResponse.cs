using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO.Response
{
    public class BatteryByClientResponse
    {
        public int Id { get; set; }
        public string ChipId { get; set; }
        public string? WorkOrder { get; set; }
        public string Status { get; set; }
        public DateTime? SaleDate { get; set; }
    }
}
