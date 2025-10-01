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
        public string ReportState { get; set; }
        public Client Client { get; set; }
        public int ClientId { get; set; }
        public string BatteryGDA { get; set; }
        public Battery Battery {  get; set; }
        public DateOnly ReportDate { get; set; }
    }
}
