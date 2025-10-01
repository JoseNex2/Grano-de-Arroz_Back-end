using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO
{
    public class ReportViewDTO
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public ClientViewDTO Client { get; set; }
        public string BatteryGDA { get; set; }
        public string ReportState { get; set; }
        public DateOnly ReportDate { get; set; }
    }
}
