using System.ComponentModel.DataAnnotations;

namespace train.ViewModels
{
    public class CheckoutInfoVm
    {
        // Contact / Shipping info
        [Required]
        public string? ContactName { get; set; }

        [Required]
        [EmailAddress]
        public string? ContactEmail { get; set; }

        [Required]
        [Phone]
        public string? ContactPhone { get; set; }

        [Required]
        public string? City { get; set; }

        [Required]
        public string? PostalCode { get; set; }

        [Required]
        public string? ShippingAddress { get; set; }

        // Totals
        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }

        // Optional: list of items to preview
        public List<CheckoutItemVm>? Items { get; set; }
    }
}