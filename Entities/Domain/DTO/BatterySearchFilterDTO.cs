using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO
{
    public class BatterySearchFilterDTO
    {
        public string? ChipId { get; set; }
        public DateOnly? SaleDate { get; set; }
        public string? ClientName { get; set; }
    }
}
