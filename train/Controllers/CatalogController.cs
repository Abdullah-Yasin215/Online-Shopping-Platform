using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using train.Repositories.Abstractions;
using train.Repositories.Interface;
using train.ViewModels;

namespace train.Controllers
{
    [AllowAnonymous]
    [Route("catalog")]
    public class CatalogController : Controller
    {
        private readonly IProductRepository _products;
        private readonly ICategoryRepository _categories;
        private readonly ICartRepository _carts;

        public CatalogController(IProductRepository products, ICategoryRepository categories, ICartRepository carts)
        {
            _products = products;
            _categories = categories;
            _carts = carts;
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

            return (userId, sid);
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(
            string? q,
            int? categoryId,
            string? audience = null,
            string? color = null,
            string? size = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int sort = 0,
            int page = 1,
            int pageSize = 12)
        {
            var (userId, sessionId) = Auth();
            audience ??= "Men";

            var items = await _products.GetAllAsync(q, categoryId, audience, color, size, minPrice, maxPrice, sort, page, pageSize);
            var total = await _products.CountAsync(q, categoryId, audience, color, size, minPrice, maxPrice);

            var vm = await BuildCatalogVm(
                mode: "Browse",
                audience: audience,
                items: items,
                total: total,
                q: q,
                categoryId: categoryId,
                color: color,
                size: size,
                minPrice: minPrice,
                maxPrice: maxPrice,
                sort: sort,
                page: page,
                pageSize: pageSize
            );

            var cart = await _carts.GetOrCreateAsync(userId, sessionId);
            ViewBag.CartCount = cart.Items.Sum(i => i.Quantity);

            return View(vm);
        }

        [HttpGet("new")]
        public async Task<IActionResult> NewIn(
            string? audience = "Men",
            int days = 3,
            int page = 1,
            int pageSize = 12)
        {
            var (userId, sessionId) = Auth();
            audience ??= "Men";

            var items = await _products.GetNewInAsync(days, audience, page, pageSize);
            var total = await _products.CountNewInAsync(days, audience);

            var vm = await BuildCatalogVm(
                mode: "NewIn",
                audience: audience,
                items: items,
                total: total,
                q: null,
                categoryId: null,
                color: null,
                size: null,
                minPrice: null,
                maxPrice: null,
                sort: 0,
                page: page,
                pageSize: pageSize,
                days: days
            );

            var cart = await _carts.GetOrCreateAsync(userId, sessionId);
            ViewBag.CartCount = cart.Items.Sum(i => i.Quantity);

            return View("Index", vm);
        }

        [HttpGet("essentials")]
        public async Task<IActionResult> Essentials(
            string? audience = "Men",
            int page = 1,
            int pageSize = 12)
        {
            var (userId, sessionId) = Auth();
            audience ??= "Men";

            var items = await _products.GetEssentialsAsync(audience, page, pageSize);
            var total = await _products.CountEssentialsAsync(audience);

            var vm = await BuildCatalogVm(
                mode: "Essentials",
                audience: audience,
                items: items,
                total: total,
                q: null,
                categoryId: null,
                color: null,
                size: null,
                minPrice: null,
                maxPrice: null,
                sort: 0,
                page: page,
                pageSize: pageSize
            );

            var cart = await _carts.GetOrCreateAsync(userId, sessionId);
            ViewBag.CartCount = cart.Items.Sum(i => i.Quantity);

            return View("Index", vm);
        }

        [HttpGet("men")] public IActionResult Men() => RedirectToAction(nameof(Index), new { audience = "Men" });
        [HttpGet("women")] public IActionResult Women() => RedirectToAction(nameof(Index), new { audience = "Women" });
        [HttpGet("boys")] public IActionResult Boys() => RedirectToAction(nameof(Index), new { audience = "Boys" });
        [HttpGet("girls")] public IActionResult Girls() => RedirectToAction(nameof(Index), new { audience = "Girls" });

        [HttpGet("men/new")] public IActionResult MenNew() => RedirectToAction(nameof(NewIn), new { audience = "Men", days = 3 });
        [HttpGet("women/new")] public IActionResult WomenNew() => RedirectToAction(nameof(NewIn), new { audience = "Women", days = 3 });
        [HttpGet("boys/new")] public IActionResult BoysNew() => RedirectToAction(nameof(NewIn), new { audience = "Boys", days = 3 });
        [HttpGet("girls/new")] public IActionResult GirlsNew() => RedirectToAction(nameof(NewIn), new { audience = "Girls", days = 3 });

        [HttpGet("men/essentials")] public IActionResult MenEssentials() => RedirectToAction(nameof(Essentials), new { audience = "Men" });
        [HttpGet("women/essentials")] public IActionResult WomenEssentials() => RedirectToAction(nameof(Essentials), new { audience = "Women" });
        [HttpGet("boys/essentials")] public IActionResult BoysEssentials() => RedirectToAction(nameof(Essentials), new { audience = "Boys" });
        [HttpGet("girls/essentials")] public IActionResult GirlsEssentials() => RedirectToAction(nameof(Essentials), new { audience = "Girls" });

        // Temporary debug endpoint
        [HttpGet("debug/products")]
        public async Task<IActionResult> DebugProducts()
        {
            var products = await _products.GetAllAsync(null, null, null, null, null, null, null, 0, 1, 100);
            var result = products.Select(p => new
            {
                p.Id,
                p.Name,
                p.TargetAudience,
                p.CategoryId,
                CategoryName = p.Category?.Name,
                CategoryColor = p.Category?.Color
            });
            return Json(result);
        }

        // Debug endpoint for categories
        [HttpGet("debug/categories")]
        public async Task<IActionResult> DebugCategories(string? audience = null)
        {
            var categories = await _categories.GetByAudienceAsync(audience ?? "Women", null);
            var result = categories.Select(c => new
            {
                c.Id,
                c.Name,
                c.Color,
                c.ParentCategoryId
            });
            return Json(result);
        }

        [HttpGet("details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var (userId, sessionId) = Auth();
            var product = await _products.GetByIdAsync(id);
            if (product == null) return NotFound();

            var cart = await _carts.GetOrCreateAsync(userId, sessionId);
            ViewBag.CartCount = cart.Items.Sum(i => i.Quantity);

            return View(product);
        }

        private async Task<CatalogIndexViewModel> BuildCatalogVm(
            string mode,
            string audience,
            IEnumerable<train.Models.Product> items,
            int total,
            string? q,
            int? categoryId,
            string? color,
            string? size,
            decimal? minPrice,
            decimal? maxPrice,
            int sort,
            int page,
            int pageSize,
            int days = 3)
        {
            var cats = await _categories.GetByAudienceAsync(audience, q);
            var catSelect = cats
                .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == categoryId))
                .ToList();

            // Heading logic
            string heading = "Shop";
            if (!string.IsNullOrEmpty(audience)) heading = $"{audience}";
            if (categoryId.HasValue)
            {
                var c = cats.FirstOrDefault(x => x.Id == categoryId);
                if (c != null) heading += $" / {c.Name}";
            }
            if (mode == "NewIn") heading += " / New In";
            if (mode == "Essentials") heading += " / Essentials";


            return new CatalogIndexViewModel
            {
                Mode = mode,
                Days = days,
                Heading = heading,
                Products = items,
                Query = q,
                CategoryId = categoryId,
                Categories = catSelect,
                Audience = audience,
                SelectedColor = color,
                Colors = GetColorOptions(color),
                SelectedSize = size,
                Sizes = GetSizeOptions(size),
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Sort = sort,
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Pagination = new PaginationViewModel { CurrentPage = page, PageSize = pageSize, TotalItems = total }
            };
        }

        private static List<SelectListItem> GetColorOptions(string? selected)
        {
            var colors = new[]
            {
                "Black", "White", "Gray", "Navy", "Blue", "Red",
                "Green", "Yellow", "Purple", "Pink", "Brown", "Beige"
            };

            var list = new List<SelectListItem> { new("All Colors", "") };
            foreach (var c in colors)
                list.Add(new SelectListItem(c, c, !string.IsNullOrEmpty(selected) &&
                    string.Equals(selected, c, StringComparison.OrdinalIgnoreCase)));
            return list;
        }

        private static List<SelectListItem> GetSizeOptions(string? selected)
        {
            var sizes = new[] { "XS", "S", "M", "L", "XL", "XXL" };

            var list = new List<SelectListItem> { new("All Sizes", "") };
            foreach (var s in sizes)
                list.Add(new SelectListItem(s, s, !string.IsNullOrEmpty(selected) &&
                    string.Equals(selected, s, StringComparison.OrdinalIgnoreCase)));
            return list;
        }
    }
}
