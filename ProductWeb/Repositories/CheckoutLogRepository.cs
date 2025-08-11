namespace ProductWeb.Repositories
{
    using MongoDB.Driver;
    using ProductWeb.Data;
    using ProductWeb.Models;
    using System.Threading.Tasks;

    public class CheckoutLogRepository
    {
        private readonly IMongoCollection<CheckoutLog> _collection;

        public CheckoutLogRepository(MongoDbContext context)
        {
            _collection = context.Collection<CheckoutLog>("checkoutLogs");
        }

        public async Task InsertAsync(CheckoutLog log)
        {
            await _collection.InsertOneAsync(log);
        }
    }

}
