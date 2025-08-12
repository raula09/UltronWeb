using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductWeb.Interfaces;
using ProductWeb.Models;
using ProductWeb.Models.DTO;
using ProductWeb.Repositories;
using ProductWeb.Services;

namespace ProductWeb.Controllers
{
    [Route("products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductRepository _products;
        private readonly IPhotoService _photoService;

        public ProductsController(ProductRepository productRepo, IPhotoService photoService)
        {
            _products = productRepo;
            _photoService = photoService;
        }

        [HttpGet("all")]
        public IActionResult All()
        {
            try
            {
                var list = _products.GetAll();
                if (list == null) return NotFound("No products found");
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateProductDto dto)
        {
            if (dto == null)
                return BadRequest("Product data is required.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required.");

            if (dto.Quantity < 0)
                return BadRequest("Quantity cannot be negative.");

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Size = dto.Size,
                Color = dto.Color,
                Category = dto.Category,
                Material = dto.Material,
                Price = dto.Price,
                Quantity = dto.Quantity
            };

            if (dto.ImageUrl != null)
            {
                var uploadResult = await _photoService.AddPhotoAsync(dto.ImageUrl);
                if (uploadResult.Error != null)
                    return BadRequest(uploadResult.Error.Message);

                product.ImageUrl = uploadResult.SecureUrl.ToString();
            }

            _products.Create(product);

            return Ok(product);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("update/{id}")]
        public IActionResult Update(string id, [FromBody] ProductUpdateDto dto)
        {
            if (dto == null)
                return BadRequest("Update data is required.");

            var product = _products.GetById(id);
            if (product == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Name)) product.Name = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Description)) product.Description = dto.Description;
            if (!string.IsNullOrWhiteSpace(dto.Size)) product.Size = dto.Size;
            if (!string.IsNullOrWhiteSpace(dto.Color)) product.Color = dto.Color;
            if (!string.IsNullOrWhiteSpace(dto.Category)) product.Category = dto.Category;
            if (!string.IsNullOrWhiteSpace(dto.Material)) product.Material = dto.Material;
            if (dto.Price.HasValue && dto.Price.Value > 0) product.Price = dto.Price.Value;
            if (dto.Quantity.HasValue)
            {
                if (dto.Quantity.Value < 0)
                    return BadRequest("Quantity cannot be negative.");
                product.Quantity = dto.Quantity.Value;
            }

            _products.Update(product);

            return Ok(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("delete/{id}")]
        public IActionResult Delete(string id)
        {
            var product = _products.GetById(id);
            if (product == null) return NotFound();

            _products.Delete(id);
            return Ok(new { message = "Deleted" });
        }
    }
}
