using train.Models;
using train.ViewModels;

namespace train.Services
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(Payment payment, PaymentInfoVm model);
        Task<Payment> CreatePaymentAsync(int orderId, decimal amount, PaymentInfoVm model);
        Task<Payment?> GetPaymentByOrderIdAsync(int orderId);
        Task<bool> VerifyPaymentAsync(string transactionId);
    }
}
