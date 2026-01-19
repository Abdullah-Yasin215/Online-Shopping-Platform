// ViewModels/CheckoutViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using train.Models;

namespace train.ViewModels
{
    public class CheckoutViewModel
    {
        // Shipping Information
        [Required]
        public string ContactName { get; set; }

        [Required]
        [EmailAddress]
        public string ContactEmail { get; set; }

        [Required]
        public string ContactPhone { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string PostalCode { get; set; }

        [Required]
        public string ShippingAddress { get; set; }

        // Shipping Method
        [Required]
        public int SelectedShippingMethodId { get; set; }
        public List<ShippingMethod> AvailableShippingMethods { get; set; } = new List<ShippingMethod>();

        // Payment Method
        [Required]
        public string PaymentMethod { get; set; } // "COD", "Card", "Wallet", "BankTransfer"

        // Card Details (only for card payments)
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string CVV { get; set; }

        // Wallet Details
        public string WalletProvider { get; set; }
        public string WalletAccount { get; set; }

        public decimal OrderTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal FinalTotal { get; set; }
    }


}