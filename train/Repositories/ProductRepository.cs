using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using train.Areas.Identity.Data;
using train.Hubs;
using train.Models;
using train.Repositories.Interface;

namespace train.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly appdbcontext _db;
        private readonly IHubContext<StockHub> _stockHub;

        public ProductRepository(appdbcontext db, IHubContext<StockHub> stockHub) { _db = db; _stockHub = stockHub; }

        public async Task<IEnumerable<Product>> GetAllAsync(
            string? q,
            int? categoryId,
            string? audience,
            string? color,
            string? size,
            decimal? minPrice,
            decimal? maxPrice,
            int sort,
            int page,
            int pageSize)
        {
            var query = _db.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => p.Name.Contains(q) || (p.Description ?? "").Contains(q));

            if (!string.IsNullOrWhiteSpace(audience))
            {
                if (audience.Equals("Junior", StringComparison.OrdinalIgnoreCase) ||
                    audience.Equals("Juniors", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.TargetAudience == "Boys" || p.TargetAudience == "Girls");
                else
                    query = query.Where(p => p.TargetAudience == audience);
            }

            if (categoryId.HasValue)
            {
                var categoryIds = await _db.Categories
                    .Where(c => c.Id == categoryId.Value || c.ParentCategoryId == categoryId.Value)
                    .Select(c => c.Id)
                    .ToListAsync();

                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            // Price Filter
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // Attribute Filters (Contains check)
            if (!string.IsNullOrWhiteSpace(color))
                query = query.Where(p => (p.Colors != null && p.Colors.Contains(color)) || (p.Category != null && p.Category.Color == color));

            if (!string.IsNullOrWhiteSpace(size))
                query = query.Where(p => p.Sizes != null && p.Sizes.Contains(size));

            // Sorting
            switch (sort)
            {
                case 1:
                    query = query.OrderBy(p => p.Price);
                    break;
                case 2:
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case 0:
                default:
                    query = query.OrderByDescending(p => p.Id); // Newest by ID assumption
                    break;
            }

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> CountAsync(
            string? q,
            int? categoryId,
            string? audience,
            string? color,
            string? size,
            decimal? minPrice,
            decimal? maxPrice)
        {
            var query = _db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => p.Name.Contains(q) || (p.Description ?? "").Contains(q));

            if (!string.IsNullOrWhiteSpace(audience))
            {
                if (audience.Equals("Junior", StringComparison.OrdinalIgnoreCase) ||
                    audience.Equals("Juniors", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.TargetAudience == "Boys" || p.TargetAudience == "Girls");
                else
                    query = query.Where(p => p.TargetAudience == audience);
            }

            if (categoryId.HasValue)
            {
                var categoryIds = await _db.Categories
                    .Where(c => c.Id == categoryId.Value || c.ParentCategoryId == categoryId.Value)
                    .Select(c => c.Id)
                    .ToListAsync();

                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            // Price Filter
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // Attribute Filters
            if (!string.IsNullOrWhiteSpace(color))
                query = query.Where(p => (p.Colors != null && p.Colors.Contains(color)) || (p.Category != null && p.Category.Color == color));

            if (!string.IsNullOrWhiteSpace(size))
                query = query.Where(p => p.Sizes != null && p.Sizes.Contains(size));

            return await query.CountAsync();
        }

        public async Task<IEnumerable<Product>> GetNewInAsync(int days, string audience, int page, int pageSize)
        {
            var query = _db.Products
               .Include(p => p.Category)
               .Where(p => p.CreatedAt >= DateTime.UtcNow.AddDays(-days));

            if (!string.IsNullOrWhiteSpace(audience))
            {
                if (audience.Equals("Junior", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.TargetAudience == "Boys" || p.TargetAudience == "Girls");
                else
                    query = query.Where(p => p.TargetAudience == audience);
            }

            return await query
               .OrderByDescending(p => p.CreatedAt)
               .Skip((page - 1) * pageSize)
               .Take(pageSize)
               .AsNoTracking()
               .ToListAsync();
        }

        public async Task<int> CountNewInAsync(int days, string audience)
        {
            var query = _db.Products
               .Where(p => p.CreatedAt >= DateTime.UtcNow.AddDays(-days));

            if (!string.IsNullOrWhiteSpace(audience))
            {
                if (audience.Equals("Junior", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.TargetAudience == "Boys" || p.TargetAudience == "Girls");
                else
                    query = query.Where(p => p.TargetAudience == audience);
            }

            return await query.CountAsync();
        }

        public async Task<IEnumerable<Product>> GetEssentialsAsync(string audience, int page, int pageSize)
        {
            // Essentials logic? For now same as All but maybe strictly ordered?
            // Or maybe a specific category?
            // Let's just return Everything for that audience sorted by popularity or ID
            var query = _db.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(audience))
            {
                if (audience.Equals("Junior", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.TargetAudience == "Boys" || p.TargetAudience == "Girls");
                else
                    query = query.Where(p => p.TargetAudience == audience);
            }

            return await query
               .OrderByDescending(p => p.Id)
               .Skip((page - 1) * pageSize)
               .Take(pageSize)
               .AsNoTracking()
               .ToListAsync();
        }

        public async Task<int> CountEssentialsAsync(string audience)
        {
            var query = _db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(audience))
            {
                if (audience.Equals("Junior", StringComparison.OrdinalIgnoreCase) ||
                    audience.Equals("Juniors", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.TargetAudience == "Boys" || p.TargetAudience == "Girls");
                else
                    query = query.Where(p => p.TargetAudience == audience);
            }

            return await query.CountAsync();
        }

        public Task<Product?> GetByIdAsync(int id) =>
            _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

        public async Task AddAsync(Product product)
        {
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            var original = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == product.Id);
            _db.Products.Update(product);
            await _db.SaveChangesAsync();

            if (original != null && original.Stock != product.Stock)
                await _stockHub.Clients.All.SendAsync("StockChanged", product.Id, product.Stock);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _db.Products.FindAsync(id);
            if (entity is null) return;
            _db.Products.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public Task<bool> ExistsAsync(int id) =>
            _db.Products.AnyAsync(e => e.Id == id);


        public async Task<List<Product>> GetLowStockProductsAsync(int threshold = 20)
        {
            return await _db.Products
                .Where(p => p.Stock < threshold)
                .OrderBy(p => p.Stock)
                .ToListAsync();
        }

    }
}
