using Microsoft.EntityFrameworkCore;
using train.Areas.Identity.Data;
using train.Models;
using train.Repositories.Interface;

namespace train.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly appdbcontext _db;
        public CartRepository(appdbcontext db) => _db = db;

        public async Task<Cart> GetOrCreateAsync(string? userId, string sessionId)
        {
            Cart? cart = await GetAsync(userId, sessionId);
            if (cart != null) return cart;

            cart = new Cart
            {
                UserId = userId,
                SessionId = string.IsNullOrEmpty(userId) ? sessionId : null
            };

            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();

            // ensure Items are loaded
            await ReloadAsync(cart);
            return cart;
        }

        public async Task<(bool isValid, List<StockValidationError> errors)> ValidateCartStockAsync(string? userId, string sessionId)
        {
            var errors = new List<StockValidationError>();

            // Get cart using your existing method
            var cart = await GetOrCreateAsync(userId, sessionId);
            await ReloadAsync(cart); // Ensure products are loaded

            foreach (var item in cart.Items)
            {
                var product = item.Product; // This should be loaded by ReloadAsync

                if (product == null)
                {
                    errors.Add(new StockValidationError
                    {
                        ProductId = item.ProductId,
                        ProductName = "Unknown Product",
                        RequestedQuantity = item.Quantity,
                        AvailableQuantity = 0
                    });
                }
                else if (product.Stock < item.Quantity)
                {
                    errors.Add(new StockValidationError
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        RequestedQuantity = item.Quantity,
                        AvailableQuantity = product.Stock
                    });
                }
            }

            return (!errors.Any(), errors);
        }
        public async Task<Cart?> GetAsync(string? userId, string sessionId)
        {
            IQueryable<Cart> q = _db.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category);

            if (!string.IsNullOrEmpty(userId))
                return await q.FirstOrDefaultAsync(c => c.UserId == userId);

            return await q.FirstOrDefaultAsync(c => c.SessionId == sessionId);
        }

        public async Task AddItemAsync(Cart cart, int productId, int quantity, decimal unitPrice, string? selectedSize = null, string? selectedColor = null)
        {
            // Check if there's already an item with the same product, size, and color
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.SelectedSize == selectedSize && i.SelectedColor == selectedColor);
            if (item == null)
            {
                item = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = 0,
                    UnitPrice = unitPrice,
                    SelectedSize = selectedSize,
                    SelectedColor = selectedColor
                };

                cart.Items.Add(item);
            }

            item.Quantity += Math.Max(1, quantity);
            await _db.SaveChangesAsync();
        }

        public async Task<(bool ok, string? error, Cart? updatedCart)> UpdateCartQuantitiesToAvailableAsync(string? userId, string sessionId)
        {
            try
            {
                var cart = await GetOrCreateAsync(userId, sessionId);
                await ReloadAsync(cart);

                var hasChanges = false;

                foreach (var item in cart.Items.ToList()) // ToList() to avoid modification during iteration
                {
                    var product = item.Product;
                    if (product == null)
                    {
                        // Remove items with missing products
                        await RemoveItemAsync(cart, item.ProductId);
                        hasChanges = true;
                    }
                    else if (product.Stock < item.Quantity)
                    {
                        if (product.Stock <= 0)
                        {
                            // Remove out-of-stock items
                            await RemoveItemAsync(cart, item.ProductId);
                        }
                        else
                        {
                            // Update quantity to available stock
                            await UpdateQuantityAsync(cart, item.ProductId, product.Stock);
                        }
                        hasChanges = true;
                    }
                }

                if (hasChanges)
                {
                    await ReloadAsync(cart); // Reload to get updated cart
                }

                return (true, null, cart);
            }
            catch (Exception ex)
            {
                return (false, "Failed to update cart quantities: " + ex.Message, null);
            }
        }

        public async Task UpdateQuantityAsync(Cart cart, int productId, int quantity)
        {
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null) return;

            if (quantity <= 0)
                _db.CartItems.Remove(item);
            else
                item.Quantity = quantity;

            await _db.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(Cart cart, int productId)
        {
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null) return;
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
        }

        public async Task ClearAsync(Cart cart)
        {
            _db.CartItems.RemoveRange(cart.Items);
            await _db.SaveChangesAsync();
        }

        public async Task ReloadAsync(Cart cart)
        {
            await _db.Entry(cart).Collection(c => c.Items).LoadAsync();
            foreach (var ci in cart.Items)
            {
                await _db.Entry(ci).Reference(x => x.Product).LoadAsync();
                if (ci.Product != null)
                    await _db.Entry(ci.Product).Reference(p => p.Category).LoadAsync();
            }
        }

        public int Count(Cart cart) => cart.Items.Sum(i => i.Quantity);
        public decimal Total(Cart cart) => cart.Items.Sum(i => i.UnitPrice * i.Quantity);

        // Repositories/CartRepository.cs  (add this method)
        public async Task AttachCartToUserAsync(string userId, string sessionId)
        {
            // session cart (anonymous)
            var sessionCart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (sessionCart == null) return;

            // existing user cart (if any)
            var userCart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null)
            {
                // just convert session cart -> user cart
                sessionCart.UserId = userId;
                sessionCart.SessionId = null;
            }
            else
            {
                // merge items
                foreach (var it in sessionCart.Items)
                {
                    var existing = userCart.Items.FirstOrDefault(x => x.ProductId == it.ProductId);
                    if (existing == null)
                    {
                        userCart.Items.Add(new CartItem
                        {
                            ProductId = it.ProductId,
                            Quantity = it.Quantity,
                            UnitPrice = it.UnitPrice
                        });
                    }
                    else
                    {
                        existing.Quantity += it.Quantity;
                    }
                }
                _db.CartItems.RemoveRange(sessionCart.Items);
                _db.Carts.Remove(sessionCart);
            }

            await _db.SaveChangesAsync();
        }

    }
}
