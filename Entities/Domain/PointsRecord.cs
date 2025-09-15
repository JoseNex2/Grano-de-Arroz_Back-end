using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain
{
    public class PointsRecord
    {
        public int Id { get; set; }
        public Dictionary<TimeOnly, float> Points { get; set; }
    }
}
