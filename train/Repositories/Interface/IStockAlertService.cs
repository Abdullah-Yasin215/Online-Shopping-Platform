using train.ViewModels;

namespace train.Repositories.Interface
{
    public interface IStockAlertService
    {
        Task CheckAndNotifyLowStockAsync(int productId);
        Task NotifyLowStockAsync(int productId, string productName, int currentStock);
        Task<List<LowStockProduct>> GetCurrentLowStockProductsAsync();
    }
}
