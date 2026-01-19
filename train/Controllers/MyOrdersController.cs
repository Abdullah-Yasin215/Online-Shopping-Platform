using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using train.Repositories.Abstractions;
using train.Repositories.Interface;


namespace train.Controllers
{
    [Authorize]
    [Route("MyOrders")]
    public class MyOrdersController : Controller
    {
        private readonly IOrderRepository _orders;
        public MyOrdersController(IOrderRepository orders) => _orders = orders;

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


        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET /MyOrders
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            if (CurrentUserId is null) return Challenge(); // force login
            var list = await _orders.GetUserOrdersAsync(CurrentUserId, take: 100);
            return View(list); // Views/MyOrders/Index.cshtml
        }

        // GET /MyOrders/123
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            if (CurrentUserId is null) return Challenge();
            var order = await _orders.GetUserOrderAsync(CurrentUserId, id);
            if (order is null) return NotFound();
            return View(order); // Views/MyOrders/Details.cshtml
        }
    }
}
