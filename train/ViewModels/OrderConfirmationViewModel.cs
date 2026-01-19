namespace train.ViewModels
{
    public class OrderConfirmationViewModel
    {
        public int OrderId { get; set; }
        public DateTime PlacedAtUtc { get; set; }
        public string Status { get; set; } = "Pending";
        public decimal Total { get; set; }
        public List<OrderLineVM> Items { get; set; } = new();
    }

}
