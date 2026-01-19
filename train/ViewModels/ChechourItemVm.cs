namespace train.ViewModels
{
    public class CheckoutItemVm
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
        public string? Color { get; set; }
        public string? CategoryName { get; set; }
    }
}

