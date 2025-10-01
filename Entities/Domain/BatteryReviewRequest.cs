using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace Entities.Domain
{
    public class BatteryReviewRequest
    {
        public string BatteryGDA { get; set; }
        public string ClientName { get; set; }
        public int ClientId { get; set; }
    }
}
