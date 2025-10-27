namespace Entities.Domain
{
    public class MeasurementStatus
    {
        public int Id {  get; set; }
        public int MeasurementId { get; set; }
        public int StatusId { get; set; }
        public Status Status { get; set; }
        public string Coment {  get; set; }
        public int ReportId { get; set; }
        public Report Report { get; set; }
    }
}
