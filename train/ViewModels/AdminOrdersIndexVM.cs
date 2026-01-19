using train.Models;

namespace train.ViewModels
{
    public class AdminOrdersIndexVM
    {
        public List<Order> Orders { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public string? Status { get; set; }
        public string? Q { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)Total / Math.Max(1, PageSize));
    }
}
