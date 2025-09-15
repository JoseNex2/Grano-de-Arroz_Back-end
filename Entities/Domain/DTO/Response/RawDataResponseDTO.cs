using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO.Response
{
    public class RawDataResponseDTO
    {
        public string MacAddress { get; set; }
        public string Magnitude { get; set; }
        public DateOnly MeasurementDate { get; set; }
    }
}
