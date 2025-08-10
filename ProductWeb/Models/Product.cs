﻿using MongoDB.Bson;
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
        public decimal Price { get; set; }
    }
}