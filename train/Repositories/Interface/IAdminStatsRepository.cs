using train.ViewModels;

namespace train.Repositories.Interface
{
    public interface IAdminStatsRepository
    {
        Task<int> GetProductCountAsync();
        Task<int> GetCategoryCountAsync();
        Task<int> GetOrdersTotalAsync();
        Task<int> GetOrdersPendingAsync();
        Task<int> GetLowStockCountAsync(int threshold);
        Task<List<AdminDashboardViewModel.OrderRow>> GetRecentOrdersAsync(int take);
    }
}
