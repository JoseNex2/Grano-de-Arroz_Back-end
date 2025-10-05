using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class ReportSearchFilter
    {
        public string? ChipId { get; set; }
        public string? ClientName { get; set; }
        public string? State { get; set; }
        public DateOnly? ReportDate { get; set; }
    }
}
