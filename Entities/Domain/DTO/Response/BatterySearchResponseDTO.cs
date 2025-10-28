namespace Entities.Domain.DTO.Response
{
    public class BatterySearchResponseDTO
    {
        public BatteryViewDTO battery {  get; set; }
        public List<MeasurementDTO> measurements { get; set; }
    }
}
