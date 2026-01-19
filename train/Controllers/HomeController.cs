using Microsoft.AspNetCore.Mvc;
using train.Repositories.Interface;      // IProductRepository
using train.Repositories.Abstractions;   // ICategoryRepository (optional)
using train.ViewModels;
using System.Security.Claims;

namespace train.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _products;

        public HomeController(IProductRepository products)
        {
            _products = products;
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


        public async Task<IActionResult> Index()
        {
            var vm = new HomePageViewModel
            {
                // Pull last 7 days across all audiences; adjust page/pageSize to taste
                NewArrivals = await _products.GetNewInAsync(days: 7, audience: "", page: 1, pageSize: 8),
                Essentials = await _products.GetEssentialsAsync(audience: "", page: 1, pageSize: 8),
                Title = "Welcome to our store",
                Subtitle = "Fresh drops & everyday essentials"
            };

            return View(vm);
        }

        public IActionResult Privacy() => View();
        public IActionResult Terms() => View();
        public IActionResult Support() => View();
    }
}
