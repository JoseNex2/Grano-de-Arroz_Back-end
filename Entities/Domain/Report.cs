using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DataAccess;

namespace Entities.Domain
{
    public class Report
    {
        public int Id { get; set; }
        public int StatusId { get; set; }
        public Status Status { get; set; }
        public DateTime ReportDate { get; set; }
        public int BatteryId { get; set; }
        public Battery Battery { get; set; }
    }
}
