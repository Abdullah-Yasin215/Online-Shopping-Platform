using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using train.Hubs;
using train.Data;
using train.ViewModels;
using train.Repositories.Interface;
using train.Areas.Identity.Data; // Your DbContext namespace

public class StockAlertService : IStockAlertService
{
    private readonly IHubContext<StockHub> _stockHub;
    private readonly appdbcontext _context;
    private readonly ILogger<StockAlertService> _logger;

    public StockAlertService(
        IHubContext<StockHub> stockHub,
        appdbcontext context,
        ILogger<StockAlertService> logger)
    {
        _stockHub = stockHub;
        _context = context;
        _logger = logger;
    }

    public async Task CheckAndNotifyLowStockAsync(int productId)
    {
        try
        {
            var product = await _context.Products
                .Where(p => p.Id == productId)
                .FirstOrDefaultAsync();

            if (product != null && product.Stock < 20)
            {
                await NotifyLowStockAsync(product.Id, product.Name, product.Stock);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking low stock for product {ProductId}", productId);
        }
    }

    public async Task NotifyLowStockAsync(int productId, string productName, int currentStock)
    {
        try
        {
            var alertData = new
            {
                productId,
                name = productName,
                stock = currentStock,
                timestamp = DateTime.UtcNow
            };

            await _stockHub.Clients.Group(StockHub.GroupName)
                .SendAsync("LowStockAlert", alertData);

            _logger.LogInformation("Low stock alert sent for {ProductName} (Stock: {Stock})",
                productName, currentStock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send low stock alert for product {ProductId}", productId);
        }
    }

    public async Task<List<LowStockProduct>> GetCurrentLowStockProductsAsync()
    {
        return await _context.Products
            .Where(p => p.Stock < 20)
            .Select(p => new LowStockProduct
            {
                ProductId = p.Id,
                Name = p.Name,
                Stock = p.Stock
            })
            .ToListAsync();
    }
}