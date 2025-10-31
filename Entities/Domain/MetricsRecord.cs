namespace Entities.Domain
{
    public class MetricsRecord
    {
        public string Id { get; set; }
        public Dictionary<string, float> Metrics { get; set; }
    }
}
