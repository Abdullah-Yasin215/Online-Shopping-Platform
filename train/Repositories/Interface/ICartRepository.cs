using train.Models;

namespace train.Repositories.Interface
{
    public interface ICartRepository
    {
        Task<Cart> GetOrCreateAsync(string? userId, string sessionId);
        Task<Cart?> GetAsync(string? userId, string sessionId);
        Task AddItemAsync(Cart cart, int productId, int quantity, decimal unitPrice, string? selectedSize = null, string? selectedColor = null);
        Task UpdateQuantityAsync(Cart cart, int productId, int quantity);
        Task RemoveItemAsync(Cart cart, int productId);
        Task ClearAsync(Cart cart);
        Task ReloadAsync(Cart cart);        // <-- make sure this includes Product+Category
        int Count(Cart cart);
        decimal Total(Cart cart);
        Task AttachCartToUserAsync(string userId, string sessionId);

        Task<(bool isValid, List<StockValidationError> errors)> ValidateCartStockAsync(string? userId, string sessionId);
        Task<(bool ok, string? error, Cart? updatedCart)> UpdateCartQuantitiesToAvailableAsync(string? userId, string sessionId);
    }

    // ✅ ADD THIS CLASS FOR STOCK ERRORS
    public class StockValidationError
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int RequestedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
    }
}
