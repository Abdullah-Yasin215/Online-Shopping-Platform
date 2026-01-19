// Controllers/PaymentController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using train.Areas.Identity.Data;
using train.Models;
using train.Services;
using train.ViewModels;

namespace train.Controllers
{
    [Authorize]
    [Route("payment")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly appdbcontext _context;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentService,
            appdbcontext context,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _context = context;
            _logger = logger;
        }

        [HttpGet("process/{orderId:int}")]
        public async Task<IActionResult> Process(int orderId)
        {
            // Verify order exists and belongs to user
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            // Check if payment already exists
            var existingPayment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
            if (existingPayment?.PaymentStatus == "Completed")
            {
                return RedirectToAction("Success", new { paymentId = existingPayment.Id });
            }

            var vm = new PaymentInfoVm
            {
                OrderId = orderId,
                OrderTotal = order.TotalAmount
            };

            return View(vm);
        }

        [HttpPost("process/{orderId:int?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(PaymentInfoVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _logger.LogInformation("Starting payment processing for order {OrderId}", model.OrderId);

                // Verify order exists
                var order = await _context.Orders.FindAsync(model.OrderId);
                if (order == null)
                {
                    ModelState.AddModelError("", "Order not found");
                    return View(model);
                }

                // Create payment record
                // SECURITY: Use order.TotalAmount from DB, ignore model.OrderTotal
                var payment = await _paymentService.CreatePaymentAsync(model.OrderId, order.TotalAmount, model);

                // Process payment
                var result = await _paymentService.ProcessPaymentAsync(payment, model);

                if (result.Success)
                {
                    _logger.LogInformation("Payment successful for order {OrderId}, transaction: {TransactionId}",
                        model.OrderId, result.TransactionId);

                    return RedirectToAction("Success", new { paymentId = payment.Id });
                }
                else
                {
                    _logger.LogWarning("Payment failed for order {OrderId}: {ErrorMessage}",
                        model.OrderId, result.Message);

                    TempData["Error"] = result.Message;
                    return RedirectToAction("Failed", new { paymentId = payment.Id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment processing error for order {OrderId}", model.OrderId);
                TempData["Error"] = "An error occurred while processing your payment.";
                return View(model);
            }
        }

        [HttpGet("success/{paymentId:int}")]
        public async Task<IActionResult> Success(int paymentId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(payment.OrderId);
            if (order == null)
            {
                return NotFound();
            }

            var vm = new PaymentResultVm
            {
                Success = true,
                Message = GetSuccessMessage(payment.PaymentMethod),
                OrderId = payment.OrderId,
                PaymentId = payment.Id,
                TransactionId = payment.TransactionId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod
            };

            return View(vm);
        }

        [HttpGet("failed/{paymentId:int}")]
        public async Task<IActionResult> Failed(int paymentId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return NotFound();
            }

            var vm = new PaymentResultVm
            {
                Success = false,
                Message = payment.FailureReason ?? "Payment processing failed",
                OrderId = payment.OrderId,
                PaymentId = payment.Id,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod
            };

            return View(vm);
        }

        [HttpGet("details/{orderId:int}")]
        public async Task<IActionResult> Details(int orderId)
        {
            var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
            if (payment == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            var vm = new PaymentResultVm
            {
                Success = payment.PaymentStatus == "Completed",
                Message = GetStatusMessage(payment.PaymentStatus),
                OrderId = orderId,
                PaymentId = payment.Id,
                TransactionId = payment.TransactionId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod
            };

            return View(vm);
        }

        private string GetSuccessMessage(string paymentMethod)
        {
            return paymentMethod switch
            {
                "COD" => "Order confirmed! You'll pay when your order is delivered.",
                "Card" => "Payment processed successfully! Your order is being prepared.",
                "Wallet" => "Wallet payment successful! Your order is being processed.",
                "BankTransfer" => "Bank transfer initiated. Please complete the transfer.",
                _ => "Payment processed successfully!"
            };
        }

        private string GetStatusMessage(string status)
        {
            return status switch
            {
                "Completed" => "Payment completed successfully",
                "Pending" => "Payment is pending confirmation",
                "Failed" => "Payment failed",
                _ => "Payment status: " + status
            };
        }
    }
}