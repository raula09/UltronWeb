using MongoDB.Driver;
using ProductWeb.Data;
using ProductWeb.Models;

namespace ProductWeb.Repositories
{
    public class ProductRepository
    {
        private readonly IMongoCollection<Product> _products;

        public ProductRepository(MongoDbContext context)
        {
            _products = context.Collection<Product>("products");
        }

        public void Create(Product product)
        {
            _products.InsertOne(product);
        }

        public List<Product> GetAll()
        {
            return _products.Find(Builders<Product>.Filter.Empty).ToList();
        }

        public Product GetById(string id)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            return _products.Find(filter).FirstOrDefault();
        }

        public void Update(Product product)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, product.Id);
            _products.ReplaceOne(filter, product);
        }

        public void Delete(string id)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            _products.DeleteOne(filter);
        }
    }
}
