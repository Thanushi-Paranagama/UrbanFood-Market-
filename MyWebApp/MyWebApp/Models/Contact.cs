using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MyWebApp.Models
{
    public class Contact
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null;

        [Required(ErrorMessage = "First name is required.")]
        [BsonElement("FirstName")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [BsonElement("LastName")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Contact number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Please enter a valid 10-digit contact number.")]
        [BsonElement("ContactNumber")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Message is required.")]
        [BsonElement("Message")]
        public string Message { get; set; }
    }
}