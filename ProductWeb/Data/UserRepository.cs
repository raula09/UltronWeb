using MongoDB.Driver;
using ProductWeb.Models;


namespace ProductWeb.Data
{
    public class UserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(MongoDbContext context)
        {
            _users = context.Collection<User>("users");
        }

        public void Create(User user)
        {
            _users.InsertOne(user);
        }
        public void Update(User user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            _users.ReplaceOne(filter, user);
        }
        public User GetByVerificationCode(string code)
        {
            var filter = Builders<User>.Filter.Eq(u => u.VerificationCode, code);
            return _users.Find(filter).FirstOrDefault();
        }

        public User GetByEmail(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            return _users.Find(filter).FirstOrDefault();
        }

        public User GetByUsername(string username)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Username, username);
            return _users.Find(filter).FirstOrDefault();
        }
        public List<User> GetAllUsername()
        {
            return _users.Find(Builders<User>.Filter.Empty).ToList();
        }
        public User GetById(string id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            return _users.Find(filter).FirstOrDefault();
        }

    }
}
