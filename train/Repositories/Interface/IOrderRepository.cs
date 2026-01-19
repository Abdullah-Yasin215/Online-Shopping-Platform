// Repositories/Interface/IOrderRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using train.Models;

namespace train.Repositories.Interface
{
    public interface IOrderRepository
    {
        Task<(bool ok, string? error, Order? order)> PlaceOrderAsync(string? userId, string sessionId);

        // ADD THIS:
        Task<(bool ok, string? error, Order? order)> PlaceOrderAsync(
            string? userId, string sessionId,
            string? contactEmail, string? contactName, string? contactPhone,
            string? city, string? postalCode, string? address,
            IStockAlertService stockAlertService);
        Task<Order?> GetByIdAsync(int id);
        Task<List<Order>> GetUserOrdersAsync(string userId, int take = 50);
        Task<Order?> GetUserOrderAsync(string userId, int orderId);
    }
}
