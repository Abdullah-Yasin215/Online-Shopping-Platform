using Microsoft.EntityFrameworkCore;
using train.Areas.Identity.Data;
using train.Models;
using train.Services;
using train.ViewModels;

using Microsoft.AspNetCore.Identity.UI.Services;

namespace train.Repositories
{
    public class PaymentService : IPaymentService
    {
        // ... existing constructor ...


        private readonly appdbcontext _context;
        private readonly ILogger<PaymentService> _logger;
        private readonly IStripePaymentService _stripeService;
        private readonly IEmailSender _emailSender;

        public PaymentService(appdbcontext context,
                             ILogger<PaymentService> logger,
                             IStripePaymentService stripeService,
                             IEmailSender emailSender)
        {
            _context = context;
            _logger = logger;
            _stripeService = stripeService;
            _emailSender = emailSender;
        }
        public async Task<PaymentResult> ProcessPaymentAsync(Payment payment, PaymentInfoVm model)
        {
            try
            {
                switch (payment.PaymentMethod)
                {
                    case "COD":
                        return await ProcessCODPayment(payment);

                    case "Card":
                        return await _stripeService.ProcessCardPaymentAsync(payment, model);

                    case "Wallet":
                        return await ProcessWalletPayment(payment, model);

                    case "BankTransfer":
                        return await ProcessBankTransferPayment(payment);

                    default:
                        return new PaymentResult
                        {
                            Success = false,
                            Message = "Unsupported payment method"
                        };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment processing failed for order {OrderId}", payment.OrderId);
                return new PaymentResult
                {
                    Success = false,
                    Message = "Payment processing failed"
                };
            }
        }

        public async Task<Payment> CreatePaymentAsync(int orderId, decimal amount, PaymentInfoVm model)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) throw new ArgumentException("Order not found");

            var payment = new Payment
            {
                OrderId = orderId,
                PaymentMethod = model.PaymentMethod,
                Amount = amount,
                PaymentStatus = "Pending"
            };

            // Store masked card details for card payments
            if (model.PaymentMethod == "Card" && !string.IsNullOrEmpty(model.CardNumber))
            {
                payment.MaskedCardNumber = $"****-****-****-{model.CardNumber.Substring(model.CardNumber.Length - 4)}";
                payment.CardHolderName = model.CardHolderName;
            }
            else if (model.PaymentMethod == "Wallet")
            {
                payment.WalletProvider = model.WalletProvider;
                payment.AccountNumber = model.WalletAccount;
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return payment;
        }


        private async Task<PaymentResult> ProcessCODPayment(Payment payment)
        {
            // COD is always successful
            payment.PaymentStatus = "Completed";
            payment.CompletedDate = DateTime.UtcNow;
            payment.TransactionId = $"COD-{DateTime.UtcNow:yyyyMMddHHmmss}";

            await _context.SaveChangesAsync();

            return new PaymentResult
            {
                Success = true,
                TransactionId = payment.TransactionId,
                Message = "COD order placed successfully"
            };
        }

        private async Task<PaymentResult> ProcessCardPayment(Payment payment, PaymentInfoVm model)
        {
            // Integrate with actual payment gateway like Stripe, PayPal, etc.
            // This is a simulation

            // Validate card details
            if (string.IsNullOrEmpty(model.CardNumber) ||
                string.IsNullOrEmpty(model.CVV) ||
                string.IsNullOrEmpty(model.ExpiryMonth) ||
                string.IsNullOrEmpty(model.ExpiryYear))
            {
                return new PaymentResult
                {
                    Success = false,
                    Message = "Invalid card details"
                };
            }

            // Simulate payment processing
            var isSuccess = SimulateGatewayResponse();

            if (isSuccess)
            {
                payment.PaymentStatus = "Completed";
                payment.TransactionId = $"CARD-{DateTime.UtcNow:yyyyMMddHHmmss}";
                payment.CompletedDate = DateTime.UtcNow;
                payment.GatewayResponse = "Approved";

                await _context.SaveChangesAsync();

                // ✅ SEND EMAIL
                await SendConfirmationEmail(payment.OrderId, payment.Amount);

                return new PaymentResult
                {
                    Success = true,
                    TransactionId = payment.TransactionId,
                    Message = "Payment processed successfully"
                };
            }
            else
            {
                payment.PaymentStatus = "Failed";
                payment.FailureReason = "Card declined";
                await _context.SaveChangesAsync();

                return new PaymentResult
                {
                    Success = false,
                    Message = "Card payment failed"
                };
            }
        }

        private async Task<PaymentResult> ProcessWalletPayment(Payment payment, PaymentInfoVm model)
        {
            // Integrate with wallet providers like Easypaisa, JazzCash
            // This is a simulation

            payment.PaymentStatus = "Completed";
            payment.TransactionId = $"{model.WalletProvider}-{DateTime.UtcNow:yyyyMMddHHmmss}";
            payment.CompletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // ✅ SEND EMAIL
            await SendConfirmationEmail(payment.OrderId, payment.Amount);

            return new PaymentResult
            {
                Success = true,
                TransactionId = payment.TransactionId,
                Message = $"{model.WalletProvider} payment successful"
            };
        }

        private async Task<PaymentResult> ProcessBankTransferPayment(Payment payment)
        {
            payment.PaymentStatus = "Pending"; // Bank transfers might need manual verification
            payment.TransactionId = $"BANK-{DateTime.UtcNow:yyyyMMddHHmmss}";

            await _context.SaveChangesAsync();

            return new PaymentResult
            {
                Success = true,
                TransactionId = payment.TransactionId,
                Message = "Bank transfer initiated. Please complete the transfer."
            };
        }

        public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task<bool> VerifyPaymentAsync(string transactionId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);

            return payment?.PaymentStatus == "Completed";
        }

        private bool SimulateGatewayResponse()
        {
            // Simulate 90% success rate for demo
            var random = new Random();
            return random.Next(0, 10) < 9;
        }

        private async Task SendConfirmationEmail(int orderId, decimal amount)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || string.IsNullOrEmpty(order.ContactEmail)) return;

            var subject = $"Order Confirmation #{orderId}";
            var itemsHtml = "<ul>" + string.Join("", order.Items.Select(i => $"<li>{i.ProductName} x {i.Quantity} - ${i.LineTotal}</li>")) + "</ul>";

            var body = $@"
                <h1>Thank you for your order!</h1>
                <p>Order #{orderId} has been confirmed.</p>
                <p><strong>Total: ${amount}</strong></p>
                <h3>Items:</h3>
                {itemsHtml}
                <p>We will notify you when your order ships.</p>
            ";

            await _emailSender.SendEmailAsync(order.ContactEmail, subject, body);
        }
    }
}
