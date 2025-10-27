namespace Entities.Domain
{
    public class PointsRecord
    {
        public int Id { get; set; }
        public Dictionary<TimeOnly, float> Points { get; set; }
    }
}
