namespace train.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Parent category (for subcategories)
        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }
        public ICollection<Category>? SubCategories { get; set; }

        // Audience filter
        public string TargetAudience { get; set; } = "Men";

        // Optional Color
        public string? Color { get; set; }

        public ICollection<Product>? Products { get; set; }
    }
}
