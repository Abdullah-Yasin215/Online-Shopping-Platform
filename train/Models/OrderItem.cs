namespace train.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // Snapshot of product info
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string? CategoryName { get; set; }
        public string? Color { get; set; }

        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
