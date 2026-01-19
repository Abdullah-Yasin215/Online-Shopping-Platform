// Services/StripePaymentService.cs
using Stripe;
using Stripe.Checkout;
using train.Areas.Identity.Data;
using train.Models;
using train.ViewModels;

namespace train.Services
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly appdbcontext _context;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(IConfiguration configuration, appdbcontext context, ILogger<StripePaymentService> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<PaymentResult> ProcessCardPaymentAsync(Payment payment, PaymentInfoVm model)
        {
            try
            {
                // Validate card details
                if (!IsValidCard(model.CardNumber ?? "", model.ExpiryMonth ?? "", model.ExpiryYear ?? "", model.CVV ?? ""))
                {
                    return new PaymentResult
                    {
                        Success = false,
                        Message = "Invalid card details"
                    };
                }

                // Create payment method
                var paymentMethodOptions = new PaymentMethodCreateOptions
                {
                    Type = "card",
                    Card = new PaymentMethodCardOptions
                    {
                        Number = (model.CardNumber ?? "").Replace(" ", "").Replace("-", ""),
                        ExpMonth = long.Parse(model.ExpiryMonth ?? "1"),
                        ExpYear = long.Parse(model.ExpiryYear ?? "2025"),
                        Cvc = model.CVV ?? "",
                    },
                    BillingDetails = new PaymentMethodBillingDetailsOptions
                    {
                        Name = model.CardHolderName,
                        Email = model.ContactEmail,
                        Phone = model.ContactPhone,
                        Address = new AddressOptions
                        {
                            Line1 = model.ShippingAddress,
                            City = model.City,
                            PostalCode = model.PostalCode
                        }
                    }
                };

                var paymentMethodService = new PaymentMethodService();
                var paymentMethod = await paymentMethodService.CreateAsync(paymentMethodOptions);

                // Create payment intent
                var paymentIntentOptions = new PaymentIntentCreateOptions
                {
                    Amount = (long)(payment.Amount * 100), // Convert to cents
                    Currency = "usd",
                    PaymentMethod = paymentMethod.Id,
                    Confirm = true,
                    ConfirmationMethod = "automatic",
                    CaptureMethod = "automatic",
                    ReturnUrl = $"{_configuration["BaseUrl"]}/Checkout/Confirmation?orderId={payment.OrderId}",
                    Metadata = new Dictionary<string, string>
                    {
                        { "order_id", payment.OrderId.ToString() },
                        { "customer_email", model.ContactEmail ?? "" }
                    },
                    Description = $"Payment for order #{payment.OrderId}",
                    Shipping = new ChargeShippingOptions
                    {
                        Name = model.ContactName,
                        Phone = model.ContactPhone,
                        Address = new AddressOptions
                        {
                            Line1 = model.ShippingAddress,
                            City = model.City,
                            PostalCode = model.PostalCode
                        }
                    }
                };

                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = await paymentIntentService.CreateAsync(paymentIntentOptions);

                // Handle payment intent status
                if (paymentIntent.Status == "succeeded")
                {
                    // Payment successful
                    payment.PaymentStatus = "Completed";
                    payment.TransactionId = paymentIntent.Id;
                    payment.GatewayResponse = $"Card payment successful. Network: {paymentMethod.Card?.Brand}";
                    payment.CompletedDate = DateTime.UtcNow;

                    // Store card network information
                    if (paymentMethod.Card != null)
                    {
                        payment.MaskedCardNumber = $"****-****-****-{paymentMethod.Card.Last4}";
                        payment.CardHolderName = model.CardHolderName;
                    }

                    await _context.SaveChangesAsync();

                    return new PaymentResult
                    {
                        Success = true,
                        TransactionId = paymentIntent.Id,
                        Message = $"Payment processed successfully via {paymentMethod.Card?.Brand?.ToUpper()}"
                    };
                }
                else if (paymentIntent.Status == "requires_action" || paymentIntent.Status == "requires_confirmation")
                {
                    // 3D Secure authentication required
                    payment.PaymentStatus = "Requires3DSecure";
                    payment.TransactionId = paymentIntent.Id;
                    await _context.SaveChangesAsync();

                    return new PaymentResult
                    {
                        Success = false,
                        TransactionId = paymentIntent.Id,
                        Message = "3D Secure authentication required",
                        GatewayResponse = paymentIntent.ClientSecret
                    };
                }
                else
                {
                    // Payment failed
                    payment.PaymentStatus = "Failed";
                    payment.FailureReason = paymentIntent.LastPaymentError?.Message ?? "Payment failed";
                    payment.TransactionId = paymentIntent.Id;
                    await _context.SaveChangesAsync();

                    return new PaymentResult
                    {
                        Success = false,
                        TransactionId = paymentIntent.Id,
                        Message = paymentIntent.LastPaymentError?.Message ?? "Payment failed"
                    };
                }
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe payment failed for order {OrderId}", payment.OrderId);

                payment.PaymentStatus = "Failed";
                payment.FailureReason = ex.StripeError?.Message ?? ex.Message;
                await _context.SaveChangesAsync();

                return new PaymentResult
                {
                    Success = false,
                    Message = ex.StripeError?.Message ?? "Payment processing error"
                };
            }
        }

        public async Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency = "usd")
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100),
                Currency = currency,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };

            var service = new PaymentIntentService();
            return await service.CreateAsync(options);
        }

        public async Task<bool> VerifyPaymentAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);
                return paymentIntent.Status == "succeeded";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify payment {PaymentIntentId}", paymentIntentId);
                return false;
            }
        }

        public async Task<PaymentMethod> CreatePaymentMethodAsync(string cardNumber, int expMonth, int expYear, string cvc)
        {
            var options = new PaymentMethodCreateOptions
            {
                Type = "card",
                Card = new PaymentMethodCardOptions
                {
                    Number = cardNumber,
                    ExpMonth = expMonth,
                    ExpYear = expYear,
                    Cvc = cvc,
                },
            };

            var service = new PaymentMethodService();
            return await service.CreateAsync(options);
        }

        private bool IsValidCard(string cardNumber, string expiryMonth, string expiryYear, string cvv)
        {
            // Basic validation
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Replace(" ", "").Length < 13)
                return false;

            if (!int.TryParse(expiryMonth, out int month) || month < 1 || month > 12)
                return false;

            if (!int.TryParse(expiryYear, out int year) || year < DateTime.Now.Year)
                return false;

            if (string.IsNullOrEmpty(cvv) || (cvv.Length != 3 && cvv.Length != 4))
                return false;

            // Check if card is expired
            if (year == DateTime.Now.Year && month < DateTime.Now.Month)
                return false;

            return true;
        }
    }
}