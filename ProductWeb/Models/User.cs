using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ProductWeb.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } = "User";

        public string VerificationCode { get; set; }
        public bool IsVerified { get; set; } = false;

        public string PasswordHash { get; set; } // <-- add this!

        // Optional properties only for login input, ignored by MongoDB
        [BsonIgnore]
        public string Password { get; set; }

        [BsonIgnore]
        public string LoginVerificationCode { get; set; }
    }
}
