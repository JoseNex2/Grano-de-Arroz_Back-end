using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO
{
    public class BatteriesSearchResponseDTO
    {
        public int TotalBatteries { get; set; }
        public IEnumerable<BatteryViewDTO> Batteries { get; set; }
    }
}
