using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Entities.Domain
{
    public class MetricsRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId _id { get; set; }
        [BsonElement("Id")]
        public int Id { get; set; }
        [BsonElement("Metrics")]
        public Dictionary<string, double> Metrics { get; set; }
    }
}
