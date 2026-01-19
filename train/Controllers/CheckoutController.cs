using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;          // ✅ SignalR
using train.Repositories.Abstractions;
using train.ViewModels;
using train.Hubs;
using train.Repositories.Interface;
using train.Areas.Identity.Data;
using train.Models;
using train.Services;


namespace train.Controllers
{
    [AllowAnonymous]
    [Route("checkout")]
    public class CheckoutController : Controller
    {
        private readonly ICartRepository _carts;
        private readonly IOrderRepository _orders;
        private readonly appdbcontext _db;
        private readonly IPaymentService _paymentService;
        public CheckoutController(ICartRepository carts, IOrderRepository orders,appdbcontext db,IPaymentService paymentService)
        {
            _paymentService = paymentService;
            _db = db;
            _carts = carts;
            _orders = orders;
        }

        private (string? userId, string sessionId) Auth()
        {
            var userId = User.Identity?.IsAuthenticated == true
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

            return (userId, sid);  // Return userId or sessionId
        }


        private string? CurrentUserId =>
            User.Identity?.IsAuthenticated == true
              ? User.FindFirstValue(ClaimTypes.NameIdentifier)
              : null;

        private string GetOrSetSessionId()
        {
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
            return sid;
        }

        // GET /checkout
        [HttpGet("")]
        public async Task<IActionResult> Review()
        {
            var sid = GetOrSetSessionId();
            if (CurrentUserId != null)
                await _carts.AttachCartToUserAsync(CurrentUserId, sid);

            var cart = await _carts.GetOrCreateAsync(CurrentUserId, sid);

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
            return View(vm);
        }



        // POST /checkout/place  (must be logged in)
        [Authorize]
        [HttpPost("place")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Place(
            [FromServices] IHubContext<AdminHub> adminHub, // ✅ keep
            [FromServices] IHubContext<StockHub> hub,      // ✅ keep
            [FromServices] IHubContext<CartHub> cartHub,   // ✅ NEW
            [FromServices] IHubContext<OrderHub> orderHub) // ✅ NEW
        {
            var sid = GetOrSetSessionId();

            await _carts.AttachCartToUserAsync(CurrentUserId!, sid);

            var (ok, error, order) = await _orders.PlaceOrderAsync(CurrentUserId!, sid);
            if (!ok || order == null)
            {
                TempData["Error"] = error ?? "Could not place order.";
                return RedirectToAction(nameof(Review));
            }

            // notify hubs (optional)
            await adminHub.Clients.Group("Admins").SendAsync("OrderCreated", new
            {
                id = order.Id,
                email = order.ContactEmail, // Add this line
                name = order.ContactName ?? "No name provided",  // ✅ ADD THIS
                date = order.OrderDate,
                status = order.Status,
                total = order.TotalAmount
            });

            await orderHub.Clients.Group(OrderHub.GroupName).SendAsync("OrderCreated", new
            {
                id = order.Id,
                email = order.ContactEmail, // Add this line
                name = order.ContactName ?? "No name provided",  // ✅ ADD THIS
                date = order.OrderDate,
                status = order.Status,
                total = order.TotalAmount
            });
        
            // ✅ (optional) notify the current user on CartHub (e.g., toast on the site)
            if (CurrentUserId is not null)
            {
                await cartHub.Clients.User(CurrentUserId).SendAsync("OrderPlaced", new
                {
                    id = order.Id,
                    total = order.TotalAmount,
                    items = order.Items.Sum(i => i.Quantity)
                });
            }

            TempData["Ok"] = $"Order #{order.Id} placed successfully.";
            return RedirectToAction("Details", "MyOrders", new { id = order.Id });
        }

        // GET /checkout/info (collect contact/shipping)
        [HttpGet("info")]
        public async Task<IActionResult> Info()
        {
            var userId = CurrentUserId;
            var sessionId = GetOrSetSessionId();

            var cart = await _carts.GetAsync(userId, sessionId);
            if (cart == null || cart.Items.Count == 0)
                return RedirectToAction("Index", "Cart");

            var vm = new CheckoutInfoVm
            {
                Items = cart.Items.Select(i => new CheckoutItemVm
                {
                    ProductName = i.Product?.Name ?? "",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Color = i.Product?.Category?.Color,
                    CategoryName = i.Product?.Category?.Name
                }).ToList(),
                Subtotal = _carts.Total(cart),
                ShippingFee = 0,
                Discount = 0,
                TotalAmount = _carts.Total(cart)
            };

            return View(vm);
        }

        //    [HttpPost("info")]
        //    [ValidateAntiForgeryToken]
        //    public async Task<IActionResult> Info(CheckoutInfoVm model,
        //[FromServices] IHubContext<AdminHub> adminHub,
        //[FromServices] IHubContext<StockHub> stockHub,
        //[FromServices] IHubContext<CartHub> cartHub,
        //[FromServices] IHubContext<OrderHub> orderHub)
        //    {
        //        if (!ModelState.IsValid)
        //            return View(model);

        //        var userId = CurrentUserId;
        //        var sessionId = GetOrSetSessionId();

        //        // merge session cart
        //        await _carts.AttachCartToUserAsync(userId, sessionId);

        //        // pass contact + shipping info into PlaceOrderAsync
        //        var (ok, error, order) = await _orders.PlaceOrderAsync(
        //            userId,
        //            sessionId,
        //            model.ContactName,
        //            model.ContactEmail,
        //            model.ContactPhone,
        //            model.City,
        //            model.PostalCode,
        //            model.ShippingAddress
        //        );

        //        if (!ok || order == null)
        //        {
        //            TempData["Error"] = error ?? "Could not place order.";
        //            return View(model);
        //        }

        //        // ✅ notify all connected admins in real time (existing AdminHub path)
        //        await adminHub.Clients.Group("Admins").SendAsync("OrderCreated", new
        //        {
        //            id = order.Id,
        //            email = order.ContactEmail ?? "No email", // ✅ Add email
        //            name = order.ContactName ?? "No name provided",  // ✅ ADD THIS
        //            date = order.OrderDate,
        //            status = order.Status,
        //            total = order.TotalAmount
        //        });

        //        // ✅ also notify via OrderHub
        //        await orderHub.Clients.Group(OrderHub.GroupName).SendAsync("OrderCreated", new
        //        {
        //            id = order.Id,
        //            email = order.ContactEmail ?? "No email", // ✅ Add email
        //            name = order.ContactName ?? "No name provided",  // ✅ ADD THIS
        //            date = order.OrderDate,
        //            status = order.Status,
        //            total = order.TotalAmount
        //        });

        //        if (userId is not null)
        //        {
        //            await cartHub.Clients.User(userId).SendAsync("OrderPlaced", new
        //            {
        //                id = order.Id,
        //                total = order.TotalAmount,
        //                items = order.Items.Sum(i => i.Quantity)
        //            });
        //        }

        //        TempData["Ok"] = $"Order #{order.Id} placed successfully.";
        //        return RedirectToAction("Details", "MyOrders", new { id = order.Id });
        //    }
        [HttpPost("info")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Info(CheckoutInfoVm model,
    [FromServices] IHubContext<AdminHub> adminHub,
    [FromServices] IHubContext<StockHub> stockHub,
    [FromServices] IHubContext<CartHub> cartHub,
    [FromServices] IHubContext<OrderHub> orderHub,
    [FromServices] ICartRepository cartRepository,
    [FromServices] IStockAlertService stockAlertService) // ✅ ADD THIS
        {
            Console.WriteLine("=== CHECKOUT INFO START ===");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ Model validation failed");
                foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation error: {modelError.ErrorMessage}");
                }
                return View(model);
            }

            var userId = CurrentUserId;
            var sessionId = GetOrSetSessionId();

            Console.WriteLine($"📝 Form data - Email: '{model.ContactEmail}', Name: '{model.ContactName}'");

            Console.WriteLine("🔍 Validating cart stock...");
            var (isValid, stockErrors) = await cartRepository.ValidateCartStockAsync(userId, sessionId);

            if (!isValid)
            {
                Console.WriteLine("❌ Stock validation failed");

                // Build error message for user
                var errorMessage = "Some items in your cart are no longer available in the requested quantities:";
                foreach (var stockError in stockErrors)
                {
                    if (stockError.AvailableQuantity == 0)
                    {
                        errorMessage += $"<br/>• {stockError.ProductName} is out of stock";
                    }
                    else
                    {
                        errorMessage += $"<br/>• {stockError.ProductName}: only {stockError.AvailableQuantity} available (you requested {stockError.RequestedQuantity})";
                    }
                }
                errorMessage += "<br/><br/>Please update your cart and try again.";

                TempData["Error"] = errorMessage;
                return View(model);
            }

            // merge session cart
            await _carts.AttachCartToUserAsync(userId!, sessionId);

            // pass contact + shipping info into PlaceOrderAsync
            Console.WriteLine("🛒 Calling PlaceOrderAsync with contact info...");
            var (ok, error, order) = await _orders.PlaceOrderAsync(
                userId!,
                sessionId,
                model.ContactEmail,
                model.ContactName,
                model.ContactPhone,
                model.City,
                model.PostalCode,
                model.ShippingAddress,
                stockAlertService // ✅ PASS THE SERVICE HERE
            );

            if (!ok || order == null)
            {
                Console.WriteLine($"❌ Order placement failed: {error}");
                TempData["Error"] = error ?? "Could not place order.";
                return View(model);
            }

            Console.WriteLine($"✅ Order #{order.Id} placed successfully!");
            Console.WriteLine($"📦 Order details - Email: '{order.ContactEmail}', Name: '{order.ContactName}'");

            // ✅ Check for low stock items after successful order
            Console.WriteLine("🔔 Checking for low stock items after order...");
            await CheckAndNotifyLowStockItemsAsync(order, stockAlertService, stockHub);

            // ✅ notify all connected admins in real time
            Console.WriteLine("📢 Sending AdminHub notification...");
            try
            {
                var adminPayload = new
                {
                    id = order.Id,
                    email = order.ContactEmail ?? "No email",
                    name = order.ContactName ?? "No name provided",
                    date = order.OrderDate,
                    status = order.Status,
                    total = order.TotalAmount
                };

                Console.WriteLine($"📤 AdminHub payload: {System.Text.Json.JsonSerializer.Serialize(adminPayload)}");

                await adminHub.Clients.Group("Admins").SendAsync("OrderCreated", adminPayload);
                Console.WriteLine("✅ AdminHub notification sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ AdminHub notification failed: {ex.Message}");
            }

            // ✅ also notify via OrderHub
            Console.WriteLine("📢 Sending OrderHub notification...");
            try
            {
                await orderHub.Clients.Group(OrderHub.GroupName).SendAsync("OrderCreated", new
                {
                    id = order.Id,
                    email = order.ContactEmail ?? "No email",
                    name = order.ContactName ?? "No name provided",
                    date = order.OrderDate,
                    status = order.Status,
                    total = order.TotalAmount
                });
                Console.WriteLine("✅ OrderHub notification sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ OrderHub notification failed: {ex.Message}");
            }

            if (userId is not null)
            {
                await cartHub.Clients.User(userId).SendAsync("OrderPlaced", new
                {
                    id = order.Id,
                    total = order.TotalAmount,
                    items = order.Items.Sum(i => i.Quantity)
                });
            }

            Console.WriteLine("=== CHECKOUT INFO COMPLETE ===");
            TempData["Ok"] = $"Order #{order.Id} placed. Please complete payment.";
            return RedirectToAction("Process", "Payment", new { orderId = order.Id });
        }

        // ✅ ADD THIS HELPER METHOD
        private async Task CheckAndNotifyLowStockItemsAsync(Order order, IStockAlertService stockAlertService, IHubContext<StockHub> stockHub)
        {
            try
            {
                Console.WriteLine("🔄 Checking for low stock items...");

                foreach (var item in order.Items)
                {
                    Console.WriteLine($"🔍 Checking product {item.ProductId} for low stock...");

                    // Check if this product is now low in stock
                    await stockAlertService.CheckAndNotifyLowStockAsync(item.ProductId);
                }

                Console.WriteLine("✅ Low stock check completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error checking low stock after order: {ex.Message}");
            }
        }



        // Optional confirmation page
        [HttpGet("confirmation/{id:int}")]
        public async Task<IActionResult> Confirmation(int id)
        {
            var o = await _orders.GetByIdAsync(id);
            if (o == null) return NotFound();

            var vm = new OrderConfirmationViewModel
            {
                OrderId = o.Id,
                PlacedAtUtc = o.OrderDate,
                Status = o.Status,
                Total = o.TotalAmount,
                Items = o.Items.Select(i => new OrderLineVM
                {
                    Name = i.ProductName,
                    Category = i.CategoryName,
                    Color = i.Color,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    LineTotal = i.LineTotal
                }).ToList()
            };

            return View(vm);
        }
    }
}
