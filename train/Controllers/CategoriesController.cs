using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using train.Repositories.Abstractions; // ICategoryRepository
using train.Models;
using train.Data;
using System.Security.Claims; // for CategoryPresets (preset-options endpoint)

namespace train.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("admin/categories")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categories;

        public CategoriesController(ICategoryRepository categories)
        {
            _categories = categories;
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


        // ===== LIST =====
        // GET /admin/categories?q=...&audience=Men
        [HttpGet("")]
        public async Task<IActionResult> Index(string? q, string? audience)
        {
            var items = await _categories.GetAllAsync(
                q: q,
                audience: string.IsNullOrWhiteSpace(audience) ? null : audience,
                parentCategoryId: null,           // only top-level in admin
                page: 1,
                pageSize: int.MaxValue);

            ViewBag.Audience = audience;
            ViewBag.Query = q;
            return View(items); // Views/Categories/Index.cshtml -> @model IEnumerable<Category>
        }

        // ===== CREATE =====
        // GET /admin/categories/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            // default audience to Men so the preset list has something to show
            return View(new Category { TargetAudience = "Men" });
        }

        // POST /admin/categories/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid) return View(model);

            // Set color to null - categories should not have colors
            model.Color = null;

            // Uniqueness by (audience + name) only - ignore color
            if (await _categories.ExistsAsync(model.Name, model.TargetAudience, null))
            {
                ModelState.AddModelError(nameof(model.Name),
                    "A category with this name already exists for this audience.");
                return View(model);
            }

            // If you want admin to manage only top-level categories:
            // model.ParentCategoryId = null;

            await _categories.AddAsync(model);
            TempData["Ok"] = "Category created.";
            return RedirectToAction(nameof(Index), new { audience = model.TargetAudience });
        }

        // ===== EDIT =====
        // GET /admin/categories/edit/5
        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var c = await _categories.GetByIdAsync(id);
            if (c == null) return NotFound();
            return View(c); // Views/Categories/Edit.cshtml -> @model Category
        }

        // POST /admin/categories/edit/5
        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            // Set color to null - categories should not have colors
            model.Color = null;

            // Uniqueness by (audience + name) only - ignore color, excluding current record
            if (await _categories.ExistsAsync(model.Name, model.TargetAudience, null, excludingId: id))
            {
                ModelState.AddModelError(nameof(model.Name),
                    "A category with this name already exists for this audience.");
                return View(model);
            }

            // model.ParentCategoryId = null; // keep top-level (optional)
            await _categories.UpdateAsync(model);
            TempData["Ok"] = "Category updated.";
            return RedirectToAction(nameof(Index), new { audience = model.TargetAudience });
        }

        // ===== DELETE =====
        // GET /admin/categories/delete/5
        [HttpGet("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _categories.GetByIdAsync(id);
            if (c == null) return NotFound();

            ViewBag.HasProducts = await _categories.HasProductsAsync(id);
            return View(c); // Views/Categories/Delete.cshtml -> @model Category
        }

        // POST /admin/categories/delete/5
        [HttpPost("delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await _categories.HasProductsAsync(id))
            {
                TempData["Error"] = "Cannot delete: Category has products.";
                return RedirectToAction(nameof(Index));
            }

            await _categories.DeleteAsync(id);
            TempData["Ok"] = "Category deleted.";
            return RedirectToAction(nameof(Index));
        }

        // ===== JSON: categories for audience (used by product form) =====
        // GET /admin/categories/for-audience?audience=Men
        [HttpGet("for-audience")]
        public async Task<IActionResult> ForAudience([FromQuery] string audience)
        {
            if (string.IsNullOrWhiteSpace(audience)) return BadRequest();

            var cats = await _categories.GetAllAsync(
                q: null,
                audience: audience,
                parentCategoryId: null, // top-level only
                page: 1,
                pageSize: int.MaxValue);

            // include color so the UI can label options clearly
            return Json(cats.Select(c => new { id = c.Id, name = c.Name, color = c.Color }));
        }

        // ===== JSON: preset names for audience (used by category create/edit views) =====
        // GET /admin/categories/preset-options?audience=Men
        [HttpGet("preset-options")]
        public IActionResult PresetOptions([FromQuery] string audience)
        {
            if (string.IsNullOrWhiteSpace(audience)) return BadRequest();

            if (!CategoryPresets.Map.TryGetValue(audience, out var list))
                return Json(Array.Empty<object>());

            var grouped = list
                .GroupBy(x => x.Group)
                .Select(g => new { group = g.Key, items = g.Select(x => x.Name).ToArray() });

            return Json(grouped);
        }
    }
}
