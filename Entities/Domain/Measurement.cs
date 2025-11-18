namespace Entities.Domain
{
    public class Measurement
    {
        public int Id { get; set; }
        public string Magnitude { get; set; }
        public DateTime MeasurementDate { get; set; }
        public int BatteryId { get; set; }
        public Battery Battery { get; set; }
    }
}
