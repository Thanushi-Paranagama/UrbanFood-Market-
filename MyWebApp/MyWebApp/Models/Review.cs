using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace MyWebApp.Models
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null;

        [BsonElement("CustomerName")]
        public string CustomerName { get; set; }

        [BsonElement("ReviewText")]
        public string ReviewText { get; set; }

        [BsonElement("Rating")]
        public int? Rating { get; set; }

        [BsonElement("Date")]
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}