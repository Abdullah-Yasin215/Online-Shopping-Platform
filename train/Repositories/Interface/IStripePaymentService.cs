// Services/StripePaymentService.cs
using Stripe;
using Stripe.Checkout;
using train.Models;
using train.ViewModels;

namespace train.Services
{
    public interface IStripePaymentService
    {
        Task<PaymentResult> ProcessCardPaymentAsync(Payment payment, PaymentInfoVm model);
        Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency = "usd");
        Task<bool> VerifyPaymentAsync(string paymentIntentId);
        Task<PaymentMethod> CreatePaymentMethodAsync(string cardNumber, int expMonth, int expYear, string cvc);
    }
}