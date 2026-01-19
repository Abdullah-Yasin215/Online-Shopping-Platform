using Microsoft.EntityFrameworkCore;
using train.Areas.Identity.Data;
using train.Models;
using train.Repositories.Abstractions;

namespace train.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly appdbcontext _db;
        public CategoryRepository(appdbcontext db) => _db = db;

        public async Task<List<Category>> GetAllAsync(
            string? q, string? audience, int? parentCategoryId, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            IQueryable<Category> query = _db.Set<Category>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(c => EF.Functions.Like(c.Name, $"%{term}%"));
            }

            if (!string.IsNullOrWhiteSpace(audience))
            {
                var aud = audience.Trim();
                query = query.Where(c => c.TargetAudience == aud);
            }

            if (parentCategoryId.HasValue)
                query = query.Where(c => c.ParentCategoryId == parentCategoryId);
            else
                query = query.Where(c => c.ParentCategoryId == null);

            return await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Category>> GetByAudienceAsync(string audience, string? q)
        {
            var query = _db.Set<Category>()
                .AsNoTracking()
                .Where(c => c.TargetAudience == audience && c.ParentCategoryId == null);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(c => EF.Functions.Like(c.Name, $"%{term}%"));
            }

            return await query.OrderBy(c => c.Name).ToListAsync();
        }

        public Task<List<Category>> GetByAudienceAsync(string audience)
            => GetByAudienceAsync(audience, null);

        public async Task<Category?> GetByIdAsync(int id)
            => await _db.Set<Category>().FirstOrDefaultAsync(c => c.Id == id);

        // ===== NEW preferred existence check (audience + name + color) =====
        public async Task<bool> ExistsAsync(string name, string? audience, string? color, int? excludingId = null)
        {
            var trimmed = name.Trim();
            var q = _db.Set<Category>().AsQueryable().Where(c => c.Name == trimmed);

            if (!string.IsNullOrWhiteSpace(audience))
                q = q.Where(c => c.TargetAudience == audience);

            // Treat null/empty color as "no color" variant
            if (string.IsNullOrWhiteSpace(color))
                q = q.Where(c => c.Color == null || c.Color == "");
            else
                q = q.Where(c => c.Color == color);

            if (excludingId.HasValue)
                q = q.Where(c => c.Id != excludingId.Value);

            return await q.AnyAsync();
        }

        // Backward-compat methods now delegate (no color) to the new method
        public Task<bool> NameExistsAsync(string name, string? audience, int? excludingId = null)
            => ExistsAsync(name, audience, color: null, excludingId: excludingId);

        public Task<bool> NameExistsAsync(string name, int? excludingId = null)
            => ExistsAsync(name, audience: null, color: null, excludingId: excludingId);

        public async Task AddAsync(Category category)
        {
            _db.Set<Category>().Add(category);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _db.Set<Category>().Update(category);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _db.Set<Category>().FindAsync(id);
            if (entity is null) return;
            _db.Set<Category>().Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> HasProductsAsync(int categoryId)
            => await _db.Set<Product>().AnyAsync(p => p.CategoryId == categoryId);
    }
}
