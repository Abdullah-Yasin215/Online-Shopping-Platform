using Microsoft.EntityFrameworkCore;
using train.Areas.Identity.Data;
using train.Repositories.Interface;   // IAdminStatsRepository
using train.ViewModels;
using train.Models;
using Microsoft.AspNetCore.Http.HttpResults;                   // Order

namespace train.Repositories
{
    public class AdminStatsRepository : IAdminStatsRepository
    {
        private readonly appdbcontext _db;

        public AdminStatsRepository(appdbcontext db)
        {
            _db = db;
        }

        public Task<int> GetProductCountAsync()
            => _db.Products.CountAsync();

        public Task<int> GetCategoryCountAsync()
            => _db.Categories.CountAsync();

        // ✅ Use Orders table, not Carts
        public Task<int> GetOrdersTotalAsync()
            => _db.Orders.CountAsync();

        // ✅ Pending/Open orders only (tweak statuses to your exact strings)
        public Task<int> GetOrdersPendingAsync()
        {
            string[] pending = new[] { "Pending", "Open" };
            return _db.Orders.CountAsync(o => pending.Contains(o.Status));
        }

        public Task<int> GetLowStockCountAsync(int threshold)
            => _db.Products.CountAsync(p => p.Stock < threshold);

        // ✅ Recent orders for the dashboard list
        public async Task<List<AdminDashboardViewModel.OrderRow>> GetRecentOrdersAsync(int take)
        {
             return await _db.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.OrderDate)
                .Take(take)
                .Select(o => new AdminDashboardViewModel.OrderRow
                {
                    Id = o.Id,
                    Email = o.ContactEmail ?? "No email provided", // ✅ ADD THIS LINE
                    Date = o.OrderDate,
                    Status = o.Status,
                    Total = o.TotalAmount
                })
                .ToListAsync();
        }
    }
}
