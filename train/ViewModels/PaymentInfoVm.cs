// ViewModels/PaymentViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace train.ViewModels
{
    public class PaymentInfoVm
    {
        [Required]
        public string PaymentMethod { get; set; } = "COD";

        // Order reference
        public int OrderId { get; set; }
        public decimal OrderTotal { get; set; }

        // Contact & Shipping Info (Required for Payment Gateways)
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactName { get; set; }
        public string? ShippingAddress { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }

        // Card details
        [RequiredIf("PaymentMethod", "Card", ErrorMessage = "Card holder name is required for card payments")]
        public string? CardHolderName { get; set; }

        [RequiredIf("PaymentMethod", "Card", ErrorMessage = "Card number is required for card payments")]
        [CreditCard(ErrorMessage = "Invalid card number")]
        public string? CardNumber { get; set; }

        [RequiredIf("PaymentMethod", "Card", ErrorMessage = "Expiry month is required for card payments")]
        [Range(1, 12, ErrorMessage = "Invalid expiry month")]
        public string? ExpiryMonth { get; set; }

        [RequiredIf("PaymentMethod", "Card", ErrorMessage = "Expiry year is required for card payments")]
        public string? ExpiryYear { get; set; }

        [RequiredIf("PaymentMethod", "Card", ErrorMessage = "CVV is required for card payments")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "Invalid CVV")]
        public string? CVV { get; set; }

        // Wallet details
        [RequiredIf("PaymentMethod", "Wallet", ErrorMessage = "Wallet provider is required")]
        public string? WalletProvider { get; set; }

        [RequiredIf("PaymentMethod", "Wallet", ErrorMessage = "Account number is required")]
        public string? WalletAccount { get; set; }

        // Bank transfer details
        public string? BankReference { get; set; }
    }

    public class PaymentResultVm
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public int PaymentId { get; set; }
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? NextStep { get; set; }
    }

    // Custom validation attribute
    public class RequiredIfAttribute : ValidationAttribute
    {
        private string PropertyName { get; set; }
        private object DesiredValue { get; set; }

        public RequiredIfAttribute(string propertyName, object desiredValue, string errorMessage = "")
        {
            PropertyName = propertyName;
            DesiredValue = desiredValue;
            ErrorMessage = errorMessage;
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            var instance = context.ObjectInstance;
            var type = instance.GetType();
            var propertyValue = type.GetProperty(PropertyName)?.GetValue(instance, null);

            if (propertyValue?.ToString() == DesiredValue.ToString() && (value == null || string.IsNullOrWhiteSpace(value.ToString())))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}