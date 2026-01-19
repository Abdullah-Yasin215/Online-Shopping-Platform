using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using train.Areas.Identity.Data;
using train.Hubs;
using train.Models;
using train.Repositories.Interface;

namespace train.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly appdbcontext _db;
        private readonly IHubContext<StockHub> _stockHub;
        private readonly IHubContext<AdminHub> _adminHub;   // ✅ for order events

        private const int LOW_STOCK_THRESHOLD = 20;

        public OrderRepository(
            appdbcontext db,
            IHubContext<StockHub> stockHub,
            IHubContext<AdminHub> adminHub)               // ✅ inject both hubs
        {
            _db = db;
            _stockHub = stockHub;
            _adminHub = adminHub;
        }

        public async Task<(bool ok, string? error, Order? order)> PlaceOrderAsync(string? userId, string sessionId)
        {
            var cart = await _db.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c =>
                    (!string.IsNullOrEmpty(userId) && c.UserId == userId) ||
                    (string.IsNullOrEmpty(userId) && c.SessionId == sessionId));

            if (cart == null || cart.Items.Count == 0)
                return (false, "Your cart is empty.", null);

            var effectiveUserId = userId ?? cart.UserId;
            if (string.IsNullOrEmpty(effectiveUserId))
                return (false, "Please sign in to place your order.", null);

            using var tx = await _db.Database.BeginTransactionAsync();

            // 1) Validate stock
            foreach (var ci in cart.Items)
            {
                if (ci.Product == null)
                    return (false, "A product in your cart no longer exists.", null);

                if (ci.Product.Stock < ci.Quantity)
                    return (false, $"Not enough stock for '{ci.Product.Name}'. Available: {ci.Product.Stock}.", null);
            }

            // 2) Decrement stock and collect just-changed products (for alerts later)
            var touchedProducts = new List<Product>();
            foreach (var ci in cart.Items)
            {
                ci.Product!.Stock -= ci.Quantity;
                touchedProducts.Add(ci.Product);
                // no need to call Update; tracked entities from Include are already tracked
            }

            // 3) Create order + snapshot lines
            var order = new Order
            {
                UserId = effectiveUserId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                Items = new List<OrderItem>() // ensure initialized
            };

            decimal total = 0m;
            foreach (var ci in cart.Items)
            {
                var oi = new OrderItem
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product!.Name,
                    CategoryName = ci.Product.Category?.Name,
                    Color = ci.Product.Category?.Color,
                    UnitPrice = ci.UnitPrice,
                    Quantity = ci.Quantity
                };
                total += oi.UnitPrice * oi.Quantity;
                order.Items.Add(oi);
            }
            order.TotalAmount = total;

            _db.Orders.Add(order);

            // 4) Clear cart
            _db.CartItems.RemoveRange(cart.Items);
            cart.Items.Clear();

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            // 5a) Notify Admins: a new order was created
            await _adminHub.Clients.Group(AdminHub.GroupName).SendAsync("OrderCreated", new
            {
                id = order.Id,
                date = order.OrderDate,
                status = order.Status,
                total = order.TotalAmount,
                // customerEmail = ... (add if you store it on Order)
            });

            // 5b) Low stock alerts only for products that crossed the threshold
            foreach (var p in touchedProducts.Where(p => p.Stock < LOW_STOCK_THRESHOLD))
            {
                await _stockHub.Clients.Group(StockHub.GroupName).SendAsync("LowStockAlert", new
                {
                    productId = p.Id,
                    name = p.Name,
                    stock = p.Stock,
                    threshold = LOW_STOCK_THRESHOLD
                });
            }

            return (true, null, order);
        }

        public Task<Order?> GetByIdAsync(int id) =>
            _db.Orders
               .Include(o => o.Items)
               .FirstOrDefaultAsync(o => o.Id == id);

        public Task<List<Order>> GetUserOrdersAsync(string userId, int take = 50) =>
            _db.Orders
               .Where(o => o.UserId == userId)
               .OrderByDescending(o => o.OrderDate)
               .Include(o => o.Items)
               .AsNoTracking()
               .Take(take)
               .ToListAsync();

        public Task<Order?> GetUserOrderAsync(string userId, int orderId) =>
            _db.Orders
               .Include(o => o.Items)
               .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        public async Task<(bool ok, string? error, Order? order)> PlaceOrderAsync(
    string userId, string sessionId,
    string? contactEmail, string? contactName, string? contactPhone,
    string? city, string? postalCode, string? address,
    IStockAlertService stockAlertService) // ✅ ADD THIS PARAMETER
        {
            // 0) Load cart with products
            var cart = await _db.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => (!string.IsNullOrEmpty(userId) && c.UserId == userId)
                                       || (string.IsNullOrEmpty(userId) && c.SessionId == sessionId));

            if (cart == null || cart.Items.Count == 0)
                return (false, "Your cart is empty.", null);

            // 1) Validate stock
            foreach (var ci in cart.Items)
            {
                if (ci.Product == null) return (false, "A product no longer exists.", null);
                if (ci.Product.Stock < ci.Quantity)
                    return (false, $"Not enough stock for '{ci.Product.Name}'. Available: {ci.Product.Stock}.", null);
            }

            using var tx = await _db.Database.BeginTransactionAsync();

            try
            {
                // 2) Decrement stock AND check for low stock alerts
                foreach (var ci in cart.Items)
                {
                    ci.Product!.Stock -= ci.Quantity;
                    _db.Products.Update(ci.Product);

                    // ✅ CHECK FOR LOW STOCK AFTER UPDATING
                    if (ci.Product.Stock < 20)
                    {
                        await stockAlertService.NotifyLowStockAsync(ci.Product.Id, ci.Product.Name, ci.Product.Stock);
                        Console.WriteLine($"📢 Low stock alert sent for {ci.Product.Name} (Stock: {ci.Product.Stock})");
                    }
                }

                // 3) Build order + snapshot items
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    ContactEmail = contactEmail,
                    ContactName = contactName,
                    ContactPhone = contactPhone,
                    City = city,
                    PostalCode = postalCode,
                    ShippingAddress = address
                };

                decimal subtotal = 0m;
                foreach (var ci in cart.Items)
                {
                    // SECURITY: Use the live price from the Product, NOT the price in the CartItem
                    var unit = ci.Product!.Price;
                    var line = unit * ci.Quantity;
                    subtotal += line;

                    order.Items.Add(new OrderItem
                    {
                        ProductId = ci.ProductId,
                        ProductName = ci.Product!.Name,
                        CategoryName = ci.Product.Category?.Name,
                        Color = ci.Product.Category?.Color,
                        UnitPrice = unit,
                        Quantity = ci.Quantity
                    });
                }

                order.Subtotal = subtotal;
                order.ShippingFee = 0;   // set if you calculate shipping
                order.Discount = 0;      // set if you have coupon logic
                order.TotalAmount = order.Subtotal + order.ShippingFee - order.Discount;

                // 4) Save order
                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                // 5) Clear cart
                _db.CartItems.RemoveRange(cart.Items);
                await _db.SaveChangesAsync();

                await tx.CommitAsync();

                Console.WriteLine($"✅ Order #{order.Id} placed successfully with low stock checks!");
                return (true, null, order);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                Console.WriteLine($"❌ Order placement failed: {ex.Message}");
                return (false, "Failed to place order.", null);
            }
        }

    }


}
