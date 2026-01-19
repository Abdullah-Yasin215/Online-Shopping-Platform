namespace train.ViewModels
{
    public class CartIndexViewModel
    {
        public List<CartLineViewModel> Items { get; set; } = new();
        public int TotalQuantity { get; set; }
        public decimal Subtotal { get; set; }
    }
}
