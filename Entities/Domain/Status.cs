namespace Entities.Domain
{
    public class Status
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Report> Reports { get; set; }
        public ICollection<MeasurementStatus> MeasurementsStatus { get; set; }
    }
}
