using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductWeb.Data;
using ProductWeb.Models;


namespace ProductWeb.Controllers
{
    [Route("products")]
    public class ProductsController : Controller
    {
        private readonly ProductRepository _products;

        public ProductsController(ProductRepository productRepo)
        {
            _products = productRepo;
        }

        [HttpGet("all")]
        public IActionResult All()
        {
            var list = _products.GetAll();
            return Ok(list);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("products/create")]
        public IActionResult Create([FromForm] string name, [FromForm] string description, [FromForm] decimal price)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Name required.");
            }

            var product = new Product
            {
                Name = name,
                Description = description,
                Price = price
            };

            _products.Create(product);
            return Ok(product);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var p = _products.GetById(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        [HttpPost("update/{id}")]
        public IActionResult Update(string id, [FromForm] string name, [FromForm] string description, [FromForm] decimal price)
        {
            var product = _products.GetById(id);
            if (product == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(name)) product.Name = name;
            if (!string.IsNullOrWhiteSpace(description)) product.Description = description;
            if (price > 0) product.Price = price;

            _products.Update(product);
            return Ok(product);
        }

        [HttpPost("delete/{id}")]
        public IActionResult Delete(string id)
        {
            _products.Delete(id);
            return Ok(new { message = "Deleted" });
        }
    }
}
