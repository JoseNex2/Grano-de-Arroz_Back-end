using Microsoft.AspNetCore.Http;

namespace Entities.Domain.DTO
{
    public class RawDataDTO
    {
        public string ChipId { get; set; }
        public string Type { get; set; }
        public string Magnitude { get; set; }
        public DateOnly MeasurementDate { get; set; }
        public IFormFile File { get; set; }
    }
}
