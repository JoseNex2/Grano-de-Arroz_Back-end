namespace Entities.Domain
{
    public class MetricsRecord
    {
        public int Id { get; set; }
        public Dictionary<string, float> Metrics { get; set; }
    }
}
