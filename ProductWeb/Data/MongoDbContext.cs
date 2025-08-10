using MongoDB.Driver;

namespace ProductWeb.Data
{

    public class MongoDbContext
    {
        private readonly IMongoDatabase _db;

        public MongoDbContext(IConfiguration config)
        {
            var conn = config.GetSection("MongoDb:ConnectionString").Value;
            var dbName = config.GetSection("MongoDb:DatabaseName").Value;
            var client = new MongoClient(conn);
            _db = client.GetDatabase(dbName);
        }

        public IMongoCollection<T> Collection<T>(string name)
        {
            return _db.GetCollection<T>(name);
        }
    }
}
