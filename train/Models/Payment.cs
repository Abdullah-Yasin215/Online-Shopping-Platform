// Models/Payment.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace train.Models
{
    public class Payment
    {
        public int Id { get; set; }

        // Link to your existing Order
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public string PaymentMethod { get; set; } = string.Empty; // "Card", "COD", "Wallet", "BankTransfer"
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded

        // For online payments
        public string? TransactionId { get; set; }
        public string? PaymentGateway { get; set; } // "Stripe", "PayPal", "JazzCash", etc.
        public decimal Amount { get; set; }

        // Card details (encrypted in real scenario)
        public string? MaskedCardNumber { get; set; }
        public string? CardHolderName { get; set; }

        // Wallet/Bank transfer details
        public string? WalletProvider { get; set; } // "Easypaisa", "JazzCash"
        public string? AccountNumber { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedDate { get; set; }

        // Additional tracking
        public string? FailureReason { get; set; }
        public string? GatewayResponse { get; set; }
    }

    public class ShippingMethod
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // "Standard", "Express", "Next-Day"
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public int DeliveryDays { get; set; }
        public bool IsActive { get; set; } = true;
    }
}