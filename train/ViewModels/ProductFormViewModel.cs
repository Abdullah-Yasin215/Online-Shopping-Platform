using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using train.Models;

namespace train.ViewModels
{
    public class ProductFormViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }

        [Required]
        public int? CategoryId { get; set; }
        public IEnumerable<SelectListItem>? Categories { get; set; }

        [Required]
        public string TargetAudience { get; set; } = "Men";

        // Color and IsOnSale removed (color comes from Category)
        // But we added simple attributes for filtering
        public string? Sizes { get; set; }
        public string? Colors { get; set; }

        public Product ToEntity()
        {
            if (!CategoryId.HasValue)
                throw new InvalidOperationException("CategoryId is required.");

            return new Product
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Price = Price,
                Stock = Stock,
                ImageUrl = ImageUrl,
                CategoryId = CategoryId.Value,
                TargetAudience = TargetAudience,
                Sizes = Sizes,
                Colors = Colors
            };
        }

        public static ProductFormViewModel FromEntity(Product p) => new ProductFormViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            ImageUrl = p.ImageUrl,
            CategoryId = p.CategoryId,
            TargetAudience = p.TargetAudience,
            Sizes = p.Sizes,
            Colors = p.Colors
        };
    }
}
