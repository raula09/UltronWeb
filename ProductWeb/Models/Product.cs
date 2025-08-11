using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace ProductWeb.Models
{
    public class Product
    {
        [BsonId] 
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string Name { get; set; }
        public string Description { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public string Material { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}