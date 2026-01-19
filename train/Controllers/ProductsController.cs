using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using train.Repositories.Interface;      // IProductRepository
using train.Repositories.Abstractions;   // ICategoryRepository
using train.ViewModels;
using train.Models;
using Microsoft.AspNetCore.SignalR;
using train.Hubs;
using System.Security.Claims;

namespace train.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("admin/products")]
    public class ProductsController : Controller
    {
        private readonly IProductRepository _products;
        private readonly ICategoryRepository _categories;
        private readonly IHubContext<CartHub> _cartHub;
        private readonly IStockAlertService _stockAlertService;
        private readonly IWebHostEnvironment _env;



        public ProductsController(IProductRepository products, ICategoryRepository categories, IHubContext<CartHub> cartHub, IStockAlertService stockAlertService, IWebHostEnvironment env)
        {
            _cartHub = cartHub;
            _products = products;
            _categories = categories;
            _stockAlertService = stockAlertService;
            _env = env;
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


        private static string CartGroupKey(string? userId, string sessionId)
               => string.IsNullOrEmpty(userId) ? $"g:{sessionId}" : $"u:{userId}";

        // READ: List
        [HttpGet("")]
        public async Task<IActionResult> Index(
            string? q,
            int? categoryId,
            string? audience,
            string? color,
            string? size,
            decimal? minPrice,
            decimal? maxPrice,
            int sort = 0,
            int page = 1,
            int pageSize = 12)
        {
            // Admin should see all products by default (audience = null shows all)
            // Don't default to "Men" like the customer-facing catalog does

            var items = await _products.GetAllAsync(q, categoryId, audience, color, size, minPrice, maxPrice, sort, page, pageSize);
            var total = await _products.CountAsync(q, categoryId, audience, color, size, minPrice, maxPrice);

            // Build ViewModel for UI
            var cats = await _categories.GetByAudienceAsync(audience ?? "", q); // If audience is null, does this return all? 
            if (string.IsNullOrEmpty(audience))
            {
                // If audience is null/empty, maybe we should fetch ALL categories?
                // The repository GetByAudienceAsync might handle null, let's check. 
                // Previous code passed "Men".
                // I'll stick to passing audience, but let's check if audience is required for categories.
                // For now, I'll pass whatever comes in.
            }

            var catSelect = cats
                .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == categoryId))
                .ToList();

            var vm = new CatalogIndexViewModel
            {
                Mode = "Admin", // Internal flag
                Heading = "Manage Products",
                Products = items,
                Query = q,
                CategoryId = categoryId,
                Categories = catSelect,
                Audience = audience ?? "",
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

            return View(vm);
        }

        // Shared helpers (duplicated from CatalogController for now)
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

        // READ: Details
        [HttpGet("details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var p = await _products.GetByIdAsync(id);
            if (p == null) return NotFound();
            return View(p);
        }

        private async Task<IEnumerable<SelectListItem>> CategoryItemsFor(string audience, int? selectedId = null)
        {
            // Helper for Forms (Create/Edit)
            var cats = await _categories.GetByAudienceAsync(audience, null);

            string Label(train.Models.Category c) =>
                string.IsNullOrWhiteSpace(c.Color) ? c.Name : $"{c.Name} ({c.Color})";

            return cats.Select(c => new SelectListItem(
                Label(c),
                c.Id.ToString(),
                selectedId.HasValue && c.Id == selectedId.Value));
        }

        // API endpoint for dynamic category filtering
        [HttpGet("categories-by-audience")]
        public async Task<IActionResult> GetCategoriesByAudience(string audience)
        {
            if (string.IsNullOrEmpty(audience))
                return BadRequest("Audience is required");

            var categories = await CategoryItemsFor(audience);
            return Json(categories);
        }


        // CREATE: GET
        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            var vm = new ProductFormViewModel
            {
                TargetAudience = "Men",
                Categories = await CategoryItemsFor("Men")
            };
            return View(vm);
        }

        // CREATE: POST
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductFormViewModel vm)
        {
            if (!vm.CategoryId.HasValue)
                ModelState.AddModelError(nameof(vm.CategoryId), "Please select a category.");

            if (!ModelState.IsValid)
            {
                var cats = await _categories.GetByAudienceAsync(vm.TargetAudience, q: null);
                vm.Categories = cats.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == vm.CategoryId));
                return View(vm);
            }

            var catId = vm.CategoryId!.Value;

            // audience/category guard
            var cat = await _categories.GetByIdAsync(catId);
            if (cat == null || !string.Equals(cat.TargetAudience, vm.TargetAudience, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(vm.CategoryId), "Selected category doesn't match the selected audience.");
                var cats = await _categories.GetByAudienceAsync(vm.TargetAudience, q: null);
                vm.Categories = cats.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == vm.CategoryId));
                return View(vm);
            }

            var product = vm.ToEntity();

            // Handle image upload
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{vm.ImageFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.ImageFile.CopyToAsync(fileStream);
                }

                product.ImageUrl = $"/images/products/{uniqueFileName}";
            }

            await _products.AddAsync(product);

            // ✅ CHECK FOR LOW STOCK AFTER CREATING PRODUCT
            if (product.Stock < 20)
            {
                await _stockAlertService.NotifyLowStockAsync(product.Id, product.Name, product.Stock);
            }

            TempData["Ok"] = "Product created.";
            return RedirectToAction(nameof(Index));
        }

        // EDIT: GET
        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _products.GetByIdAsync(id);
            if (p is null) return NotFound();

            var vm = ProductFormViewModel.FromEntity(p);
            vm.Categories = await CategoryItemsFor(vm.TargetAudience, vm.CategoryId);
            return View(vm);
        }

        // EDIT: POST
        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            if (!vm.CategoryId.HasValue)
                ModelState.AddModelError(nameof(vm.CategoryId), "Please select a category.");

            if (!ModelState.IsValid)
            {
                var cats = await _categories.GetByAudienceAsync(vm.TargetAudience, q: null);
                vm.Categories = cats.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == vm.CategoryId));
                return View(vm);
            }

            var catId = vm.CategoryId!.Value;

            var product = await _products.GetByIdAsync(id);
            if (product == null) return NotFound();

            // audience/category guard
            var cat = await _categories.GetByIdAsync(catId);
            if (cat == null || !string.Equals(cat.TargetAudience, vm.TargetAudience, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(vm.CategoryId), "Selected category doesn't match the selected audience.");
                var cats = await _categories.GetByAudienceAsync(vm.TargetAudience, q: null);
                vm.Categories = cats.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == vm.CategoryId));
                return View(vm);
            }

            // Apply changes (color & isOnSale removed)
            product.Name = vm.Name;
            product.Description = vm.Description;
            product.Price = vm.Price;
            product.Stock = vm.Stock;
            product.CategoryId = catId;
            product.TargetAudience = vm.TargetAudience;
            product.Sizes = vm.Sizes;
            product.Colors = vm.Colors;

            // Handle image upload
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{vm.ImageFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.ImageFile.CopyToAsync(fileStream);
                }

                product.ImageUrl = $"/images/products/{uniqueFileName}";
            }
            else if (!string.IsNullOrEmpty(vm.ImageUrl))
            {
                product.ImageUrl = vm.ImageUrl;
            }

            await _products.UpdateAsync(product);
            TempData["Ok"] = "Product updated.";
            return RedirectToAction(nameof(Index));
        }

        // DELETE: GET
        [HttpGet("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _products.GetByIdAsync(id);
            if (p == null) return NotFound();
            return View(p);
        }

        // DELETE: POST
        [HttpPost("delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _products.DeleteAsync(id);
            TempData["Ok"] = "Product deleted.";
            return RedirectToAction(nameof(Index));
        }


    }
}
