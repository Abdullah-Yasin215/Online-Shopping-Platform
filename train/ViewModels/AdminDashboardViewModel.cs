namespace train.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int ProductCount { get; set; }
        public int CategoryCount { get; set; }
        public int OrdersPending { get; set; }
        public int OrdersTotal { get; set; }
        public int LowStock { get; set; }

        public List<OrderRow> RecentOrders { get; set; } = new();

        public List<LowStockProduct> LowStockProducts { get; set; } = new();
        public class OrderRow
        {
            public int Id { get; set; }
            public string Email { get; set; } = "";
            public DateTime Date { get; set; }
            public string Status { get; set; } = "";
            public decimal Total { get; set; }
        }
    }
}
