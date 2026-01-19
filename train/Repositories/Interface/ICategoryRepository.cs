using train.Models;

namespace train.Repositories.Abstractions
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync(
            string? q,
            string? audience,
            int? parentCategoryId,
            int page,
            int pageSize);

        Task<List<Category>> GetByAudienceAsync(string audience, string? q);
        Task<List<Category>> GetByAudienceAsync(string audience);

        Task<Category?> GetByIdAsync(int id);

        // NEW: existence including color (preferred)
        Task<bool> ExistsAsync(string name, string? audience, string? color, int? excludingId = null);

        // Backward-compat (kept if other code still calls it)
        Task<bool> NameExistsAsync(string name, string? audience, int? excludingId = null);
        Task<bool> NameExistsAsync(string name, int? excludingId = null);

        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);

        Task<bool> HasProductsAsync(int categoryId);
    }
}
