using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductWeb.Models;
using ProductWeb.Repositories;

namespace ProductWeb.Controllers
{
    [Route("products")]
    [ApiController]
    public class ProductsController : ControllerBase
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
        [HttpPost("create")]
        public IActionResult Create(
            [FromForm] string name,
            [FromForm] string description,
            [FromForm] string size,
            [FromForm] string color,
            [FromForm] string category,
            [FromForm] string material,
            [FromForm] decimal price,
            [FromForm] int quantity)  
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Name required.");
            }

            if (quantity < 0)
            {
                return BadRequest("Quantity cannot be negative.");
            }

            var product = new Product
            {
                Name = name,
                Description = description,
                Size = size,
                Color = color,
                Category = category,
                Material = material,
                Price = price,
                Quantity = quantity 
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

        [Authorize(Roles = "Admin")]
        [HttpPost("update/{id}")]
        public IActionResult Update(
            string id,
            [FromForm] string name,
            [FromForm] string description,
            [FromForm] string size,
            [FromForm] string color,
            [FromForm] string category,
            [FromForm] string material,
            [FromForm] decimal price,
            [FromForm] int? quantity)  
        {
            var product = _products.GetById(id);
            if (product == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(name)) product.Name = name;
            if (!string.IsNullOrWhiteSpace(description)) product.Description = description;
            if (!string.IsNullOrWhiteSpace(size)) product.Size = size;
            if (!string.IsNullOrWhiteSpace(color)) product.Color = color;
            if (!string.IsNullOrWhiteSpace(category)) product.Category = category;
            if (!string.IsNullOrWhiteSpace(material)) product.Material = material;
            if (price > 0) product.Price = price;

            if (quantity.HasValue)
            {
                if (quantity.Value < 0)
                    return BadRequest("Quantity cannot be negative.");
                product.Quantity = quantity.Value;
            }

            _products.Update(product);
            return Ok(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("delete/{id}")]
        public IActionResult Delete(string id)
        {
            _products.Delete(id);
            return Ok(new { message = "Deleted" });
        }
    }
}
