namespace train.ViewModels
{
    public class HomePageViewModel
    {
        public IEnumerable<train.Models.Product>? NewArrivals { get; set; }
        public IEnumerable<train.Models.Product>? Essentials { get; set; }
        // If you add a repo method later, you can include:
        public IEnumerable<train.Models.Product>? TopSellers { get; set; }

        public string Title { get; set; } = "Welcome";
        public string Subtitle { get; set; } = "Discover the latest arrivals and essentials.";
    }
}
