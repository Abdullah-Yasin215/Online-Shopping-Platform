using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using train.Hubs;
using train.Repositories.Interface;     // IProductRepository
using train.ViewModels;


namespace train.Controllers
{
    [AllowAnonymous]
    [Route("cart")]
    public class CartController : Controller
    {
        private readonly ICartRepository _carts;
        private readonly IProductRepository _products;
        private readonly IOrderRepository _order;

        public CartController(ICartRepository carts, IProductRepository products, IOrderRepository order)
        {
            _carts = carts;
            _products = products;
            _order = order;
        }




        // ===== helpers =====
        protected (string? userId, string sessionId) Auth()
        {
            string? userId = User.Identity?.IsAuthenticated == true
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            const string cookieName = "sf_sid";
            if (!Request.Cookies.TryGetValue(cookieName, out var sid) || string.IsNullOrWhiteSpace(sid))
            {
                sid = Guid.NewGuid().ToString("N");
                Response.Cookies.Append(cookieName, sid, new CookieOptions
                {
                    HttpOnly = true,
                    IsEssential = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });
            }
            return (userId, sid);
        }

        private static CartIndexViewModel BuildVm(Cart cart)
        {
            var vm = new CartIndexViewModel
            {
                Items = cart.Items.Select(i => new CartLineViewModel
                {
                    ProductId = i.ProductId,
                    Name = i.Product?.Name ?? "Product",
                    ImageUrl = i.Product?.ImageUrl,
                    Category = i.Product?.Category?.Name,
                    Color = i.Product?.Category?.Color,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList()
            };
            vm.TotalQuantity = vm.Items.Sum(x => x.Quantity);
            vm.Subtotal = vm.Items.Sum(x => x.LineTotal);
            return vm;
        }

        // ===== browse cart =====
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var (userId, sid) = Auth();
            var cart = await _carts.GetOrCreateAsync(userId, sid);
            var vm = BuildVm(cart);
            ViewBag.CartCount = vm.TotalQuantity;
            return View(vm); // Views/Cart/Index.cshtml
        }

        [HttpPost("add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(
    int productId,
    [FromServices] IHubContext<CartHub> cartHub,
    int qty = 1,
    string? selectedSize = null,
    string? selectedColor = null,
    string? returnUrl = null,
    string? connectionId = null)
        {
            Console.WriteLine($"=== CART ADD REQUEST ===");
            Console.WriteLine($"ProductId: {productId}, Quantity: {qty}, ConnectionId: {connectionId}");

            var (userId, sid) = Auth();
            var cart = await _carts.GetOrCreateAsync(userId, sid);

            var product = await _products.GetByIdAsync(productId);
            if (product == null) return NotFound();

            await _carts.AddItemAsync(cart, productId, qty, product.Price, selectedSize, selectedColor);
            // await _carts.ReloadAsync(cart); // REMOVED: causes in-memory duplication since AddItemAsync already adds to collection

            var totalQty = cart.Items.Sum(i => i.Quantity);
            var groupKey = CartHub.GroupKey(userId, sid);

            // Fix: Use Convert.ToInt32 instead of Math.Round
            var payload = new
            {
                productId,
                name = product.Name,
                quantity = Convert.ToInt32(qty),
                totalQuantity = Convert.ToInt32(totalQty)
            };

            Console.WriteLine($"📤 Sending cart update - Total: {totalQty}");

            // Send to group (for user's sessions)
            if (!string.IsNullOrEmpty(groupKey))
            {
                if (!string.IsNullOrEmpty(connectionId))
                {
                    // Exclude the current connection so they don't get double notifications (AJAX + SignalR)
                    await cartHub.Clients.GroupExcept(groupKey, new[] { connectionId })
                        .SendAsync("ItemAddedToCart", payload);
                }
                else
                {
                    await cartHub.Clients.Group(groupKey).SendAsync("ItemAddedToCart", payload);
                }
            }
            // Fallback: no group (shouldn't happen), but if so, maybe send to connection? 
            // Actually, if groupKey is missing, we can't do much. 
            // But we already return JSON for the caller. So no fallback needed.
            else if (!string.IsNullOrEmpty(connectionId))
            {
                await cartHub.Clients.Client(connectionId).SendAsync("ItemAddedToCart", payload);
            }

            // Return the payload so the frontend can show a toast immediately (fallback if SignalR fails)
            return Ok(new { success = true, cartCount = totalQty, payload });
        }

        // Add this debugging method to your CartController
        [HttpGet("debug")]
        public async Task<IActionResult> DebugCart()
        {
            var (userId, sid) = Auth();
            var cart = await _carts.GetOrCreateAsync(userId, sid);

            Console.WriteLine($"=== CART DEBUG ===");
            Console.WriteLine($"User: {userId}, Session: {sid}");
            Console.WriteLine($"Total Items: {cart.Items.Sum(i => i.Quantity)}");
            foreach (var item in cart.Items)
            {
                Console.WriteLine($"- {item.Product?.Name}: {item.Quantity} x {item.UnitPrice}");
            }
            Console.WriteLine($"==================");

            return Json(new
            {
                totalItems = cart.Items.Sum(i => i.Quantity),
                items = cart.Items.Select(i => new { i.ProductId, Name = i.Product?.Name ?? "Unknown", i.Quantity })
            });
        }
        [HttpGet("getcount")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var (userId, sid) = Auth();
                var cart = await _carts.GetOrCreateAsync(userId, sid);
                var totalQuantity = cart.Items.Sum(i => i.Quantity);

                Console.WriteLine($"📊 GetCount: User={userId}, Session={sid}, Count={totalQuantity}");

                return Json(totalQuantity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetCount error: {ex.Message}");
                return Json(0);
            }
        }
        // ===== remove line =====
        [HttpPost("remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId)
        {
            var (userId, sid) = Auth();
            var cart = await _carts.GetOrCreateAsync(userId, sid);

            await _carts.RemoveItemAsync(cart, productId);
            await _carts.ReloadAsync(cart);

            TempData["Ok"] = "Item removed.";
            return RedirectToAction(nameof(Index));
        }

        // ===== clear cart =====
        [HttpPost("clear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var (userId, sid) = Auth();
            var cart = await _carts.GetOrCreateAsync(userId, sid);
            await _carts.ClearAsync(cart);
            TempData["Ok"] = "Cart cleared.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
    int productId,
    int qty,
    [FromServices] IHubContext<CartHub> cartHub)
        {
            var (userId, sid) = Auth();
            var cart = await _carts.GetOrCreateAsync(userId, sid);

            var currentItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (currentItem == null)
            {
                return Json(new { success = false, message = "Item not found in cart" });
            }

            await _carts.UpdateQuantityAsync(cart, productId, qty);
            await _carts.ReloadAsync(cart);

            var updatedCart = await _carts.GetOrCreateAsync(userId, sid);
            var totalQuantity = updatedCart.Items.Sum(i => i.Quantity);
            var subtotal = updatedCart.Items.Sum(i => i.LineTotal);

            var updatedItem = updatedCart.Items.FirstOrDefault(i => i.ProductId == productId);
            var itemSubtotal = updatedItem?.LineTotal ?? 0;

            var groupKey = CartHub.GroupKey(userId, sid);
            var payload = new
            {
                productId,
                quantity = qty,
                itemSubtotal = itemSubtotal,
                subtotal = subtotal,
                totalQuantity = totalQuantity,
                success = true
            };

            if (!string.IsNullOrEmpty(groupKey))
            {
                await cartHub.Clients.Group(groupKey).SendAsync("CartQuantityUpdated", payload);
            }

            return Json(payload);
        }

        // ===== proceed to checkout (placeholder) =====
        [HttpPost("checkout")]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout()
        {
            // Here you would redirect to your payment/checkout flow.
            // We keep it simple and just show the cart again as the "review" step is the cart page itself.
            TempData["Ok"] = "Review your order below, then continue to payment.";
            return RedirectToAction(nameof(Index));
        }
    }
}
