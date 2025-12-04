using DataAccess;

namespace Entities.Domain
{
    public class Battery
    {
        public int Id { get; set; }
        public string ChipId { get; set; }
        public string? WorkOrder { get; set; }
        public string Type { get; set; }
        public DateTime? SaleDate { get; set; }
        public DateTime RegisteredDate { get; set; }
        public int? ClientId { get; set; }
        public Client? Client { get; set; }
        public Report? Report { get; set; }
        public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
    }
}
