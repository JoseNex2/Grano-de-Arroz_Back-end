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
        public DateTime ReportDate { get; set; }
        public Battery Battery {  get; set; }
    }
}
