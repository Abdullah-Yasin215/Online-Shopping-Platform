namespace train.Models
{

    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }
        public Cart? Cart { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; } = 1;

        // Unit price captured at the time of adding to cart
        public decimal UnitPrice { get; set; }

        // Selected size and color (if applicable)
        public string? SelectedSize { get; set; }
        public string? SelectedColor { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
    }
}
