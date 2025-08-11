using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductWeb.Models;
using ProductWeb.Repositories;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CartController : ControllerBase
{
    private readonly CartRepository _cartRepo;
    private readonly ProductRepository _productRepo;
    private readonly CheckoutLogRepository _checkoutLogRepo;
    private readonly UserRepository _userRepo;
    private readonly EmailService _emailService;

    public CartController(
        CartRepository cartRepo,
        ProductRepository productRepo,
        CheckoutLogRepository checkoutLogRepo,
         EmailService emailService,
        UserRepository userRepo)   
    {
        _cartRepo = cartRepo;
        _productRepo = productRepo;
        _emailService = emailService;
        _checkoutLogRepo = checkoutLogRepo;
        _userRepo = userRepo;  
    }


    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpPost("add")]
    public IActionResult AddToCart([FromBody] CartItem item)
    {
        if (item == null || string.IsNullOrEmpty(item.ProductId) || item.Quantity <= 0)
            return BadRequest("Invalid product or quantity");

        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var product = _productRepo.GetById(item.ProductId);
        if (product == null)
            return NotFound("Product not found");

        var cart = _cartRepo.GetCartByUserId(userId) ?? new Cart { UserId = userId, Items = new List<CartItem>() };

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existingItem != null)
            existingItem.Quantity += item.Quantity;
        else
            cart.Items.Add(item);

        _cartRepo.SaveCart(cart);

        return Ok(new { message = "Item added to cart", cart });
    }

    [HttpGet]
    public IActionResult ViewCart()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var cart = _cartRepo.GetCartByUserId(userId) ?? new Cart { UserId = userId, Items = new List<CartItem>() };
        return Ok(cart);
    }
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var userId = GetUserId();
        var cart = _cartRepo.GetCartByUserId(userId);

        if (cart == null || !cart.Items.Any())
            return BadRequest("Cart is empty.");

        var checkoutLogItems = new List<CheckoutLogItem>();
        decimal totalCost = 0m;

        foreach (var item in cart.Items)
        {
            var product = _productRepo.GetById(item.ProductId);
            if (product == null)
                return BadRequest($"Product {item.ProductId} not found.");

            if (product.Quantity < item.Quantity)
                return BadRequest($"Insufficient stock for product {product.Name}.");

            _productRepo.UpdateQuantity(product.Id, product.Quantity - item.Quantity);

            checkoutLogItems.Add(new CheckoutLogItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                Price = product.Price
            });

            totalCost += product.Price * item.Quantity;
        }

        var user = _userRepo.GetById(userId);

        var logEntry = new CheckoutLog
        {
            UserId = userId,
            UserName = user?.Username ?? "Unknown",
            UserEmail = user?.Email ?? "Unknown",
            PhoneNumber = user?.PhoneNumber ?? "Unknown",
            Items = checkoutLogItems,
            Timestamp = DateTime.UtcNow
        };

        await _checkoutLogRepo.InsertAsync(logEntry);

        _cartRepo.DeleteCart(userId);

        var emailBody = new System.Text.StringBuilder();
        emailBody.AppendLine($"Hello {user?.Username ?? "Customer"},");
        emailBody.AppendLine("Thank you for your purchase! Here are the details of your order:\n");

        foreach (var item in checkoutLogItems)
        {
            emailBody.AppendLine($"- {item.ProductName}: Quantity {item.Quantity}, Price per item ${item.Price:F2}, Total ${item.Price * item.Quantity:F2}");
        }
        emailBody.AppendLine($"\nTotal Amount: ${totalCost:F2}");
        emailBody.AppendLine($"\nWe will call you shortly at {user?.PhoneNumber ?? "your phone number"} to confirm your order.");
        emailBody.AppendLine("\nThank you for shopping with us!");

        await _emailService.SendEmailAsync(
            user?.Email ?? "",
            "Purchase Confirmation",
            emailBody.ToString()
        );

        return Ok(new { message = "Checkout successful, confirmation email sent.", purchasedItems = checkoutLogItems });
    }


    [HttpDelete]
        public IActionResult ClearCart()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            _cartRepo.DeleteCart(userId);
            return Ok(new { message = "Cart cleared." });
        }
    }
