namespace ProductWeb.Repositories
{
    using ProductWeb.Models;
    using System.Collections.Concurrent;

    public class CartRepository
    {
        private readonly ConcurrentDictionary<string, Cart> _carts = new();

        public Cart GetCartByUserId(string userId)
        {
            _carts.TryGetValue(userId, out var cart);
            return cart ?? new Cart { UserId = userId };
        }

        public void SaveCart(Cart cart)
        {
            _carts[cart.UserId] = cart;
        }

        public void DeleteCart(string userId)
        {
            _carts.TryRemove(userId, out _);
        }
    }

}
