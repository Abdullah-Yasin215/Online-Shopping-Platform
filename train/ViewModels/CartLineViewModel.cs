namespace train.ViewModels
{
    public class CartLineViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }     // e.g., "Tees"
        public string? Color { get; set; }        // from Category.Color
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}