namespace train.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }

        // Audience: Men, Women, Boys, Girls  (back-compat: "Junior" -> Boys+Girls)
        public string TargetAudience { get; set; } = "Men";

        // Color moved to Category; do NOT keep it here

        // Category relationship
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Removed IsOnSale

        // When the product became available (used for "New In")
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Attributes for filtering (comma-separated, e.g. "S, M, L" or "Red, Blue")
        public string? Sizes { get; set; }
        public string? Colors { get; set; }
    }
}
