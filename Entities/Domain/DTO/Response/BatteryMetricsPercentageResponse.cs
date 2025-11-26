using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO.Response
{
    public class BatteryMetricsPercentageResponse
    {
        public double SoldPercentage { get; set; }
        public double SoldWithReportPercentage { get; set; }
    }
}
