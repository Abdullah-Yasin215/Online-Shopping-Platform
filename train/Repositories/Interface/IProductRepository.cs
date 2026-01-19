using train.Models;

namespace train.Repositories.Interface
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(
            string? q,
            int? categoryId,
            string? audience,
            string? color,
            string? size,
            decimal? minPrice,
            decimal? maxPrice,
            int sort,
            int page,
            int pageSize);

        Task<int> CountAsync(
             string? q,
             int? categoryId,
             string? audience,
             string? color,
             string? size,
             decimal? minPrice,
             decimal? maxPrice);

        Task<IEnumerable<Product>> GetNewInAsync(int days, string audience, int page, int pageSize);
        Task<int> CountNewInAsync(int days, string audience);

        Task<IEnumerable<Product>> GetEssentialsAsync(string audience, int page, int pageSize);
        Task<int> CountEssentialsAsync(string audience);

        Task<Product?> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        Task<List<Product>> GetLowStockProductsAsync(int threshold = 20);
    }
}
