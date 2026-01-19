using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using train.Areas.Identity.Data;
using train.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using train.ViewModels;
using System.Security.Claims;

namespace train.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly IAdminStatsRepository _stats;

        public AdminController(IAdminStatsRepository stats)
        {
            _stats = stats;
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


        public async Task<IActionResult> Index(
      [FromServices] IProductRepository productRepository) // Add this parameter
        {
            // Get current low stock products
            var lowStockProducts = await productRepository.GetLowStockProductsAsync(20);

            var vm = new AdminDashboardViewModel
            {
                ProductCount = await _stats.GetProductCountAsync(),
                CategoryCount = await _stats.GetCategoryCountAsync(),
                OrdersTotal = await _stats.GetOrdersTotalAsync(),
                OrdersPending = await _stats.GetOrdersPendingAsync(),
                LowStock = await _stats.GetLowStockCountAsync(5),
                RecentOrders = await _stats.GetRecentOrdersAsync(5)
            };

            // Pass low stock products to the view
            ViewBag.LowStockProducts = lowStockProducts.Select(p => new
            {
                ProductId = p.Id,
                Name = p.Name,
                Stock = p.Stock
            }).ToList();

            return View(vm);
        }
        public async Task<IActionResult> Orders(
     [FromServices] appdbcontext db,
     int page = 1, int pageSize = 20, string? status = null, string? q = null)
        {
            if (page < 1) page = 1;

            var query = db.Orders
                .AsNoTracking()
                .Include(o => o.User)   // ← required for o.User.Email
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(o => o.Status == status);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(o => o.Id.ToString().Contains(q));

            var total = await query.CountAsync();
            var orders = await query.Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            return View(new AdminOrdersIndexVM
            {
                Orders = orders,
                Page = page,
                PageSize = pageSize,
                Total = total,
                Status = status,
                Q = q
            });
        }

        // GET /Admin/OrderDetails/5
        [HttpGet]
        public async Task<IActionResult> OrderDetails([FromServices] appdbcontext db, int id)
        {
            var order = await db.Orders
                .AsNoTracking()
                .Include(o => o.User)   // ← required for Model.User.Email
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

    }
}

