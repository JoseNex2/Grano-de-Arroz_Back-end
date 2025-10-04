using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO
{
    public class MeasurementUpdateDTO
    {
        public int MeasurementId { get; set; }
        public string Status { get; set; }
        public string Coment { get; set; }
    }
}
