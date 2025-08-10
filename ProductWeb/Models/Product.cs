using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace ProductWeb.Models
{
    public class Product
    {
        [BsonId]  // Marks this as the MongoDB _id field
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}