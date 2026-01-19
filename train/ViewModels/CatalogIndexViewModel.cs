using Microsoft.AspNetCore.Mvc.Rendering;
using train.Models;

namespace train.ViewModels
{
    public class CatalogIndexViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();

        public string? Query { get; set; }
        public int? CategoryId { get; set; }
        public IEnumerable<SelectListItem>? Categories { get; set; }

        // Audience
        public string Audience { get; set; } = "Men";

        // Color (Attributes)
        public string? SelectedColor { get; set; }
        public IEnumerable<SelectListItem>? Colors { get; set; }

        // Size (Attributes)
        public string? SelectedSize { get; set; }
        public IEnumerable<SelectListItem>? Sizes { get; set; }

        // Price Range
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public int Sort { get; set; } // 0=Newest, 1=LowHigh, 2=HighLow

        // Paging
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public PaginationViewModel Pagination { get; set; } = new();

        // NEW FIELDS (used by controller/actions like NewIn / Essentials)
        // "Browse" | "NewIn" | "Essentials" (or anything you set)
        public string Mode { get; set; } = "Browse";

        // When Mode == "NewIn", this is how many days back we show
        public int Days { get; set; } = 90; // 3 months default

        // Optional convenience heading you can use in the view
        public string Heading { get; set; } = "Browse";
    }

    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}
