// Controllers/WebhookController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using train.Areas.Identity.Data;
using train.Models;
using Stripe;

[ApiController]
[Route("webhook")]
public class WebhookController : ControllerBase
{
    private readonly ILogger<WebhookController> _logger;
    private readonly appdbcontext _context;
    private readonly IConfiguration _configuration;

    public WebhookController(ILogger<WebhookController> logger, appdbcontext context, IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _configuration["Stripe:WebhookSecret"]
            );

            // Handle the event
            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                await HandlePaymentIntentSucceeded(paymentIntent);
            }
            else if (stripeEvent.Type == "payment_intent.payment_failed")
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                await HandlePaymentIntentFailed(paymentIntent);
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook error");
            return BadRequest();
        }
    }

    private async Task HandlePaymentIntentSucceeded(PaymentIntent paymentIntent)
    {
        if (paymentIntent.Metadata.TryGetValue("order_id", out string orderIdStr) &&
            int.TryParse(orderIdStr, out int orderId))
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

            if (payment != null)
            {
                payment.PaymentStatus = "Completed";
                payment.TransactionId = paymentIntent.Id;
                payment.CompletedDate = DateTime.UtcNow;
                payment.GatewayResponse = "Webhook confirmed payment success";

                await _context.SaveChangesAsync();

                _logger.LogInformation("Payment confirmed via webhook for order {OrderId}", orderId);
            }
        }
    }

    private async Task HandlePaymentIntentFailed(PaymentIntent paymentIntent)
    {
        if (paymentIntent.Metadata.TryGetValue("order_id", out string orderIdStr) &&
            int.TryParse(orderIdStr, out int orderId))
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

            if (payment != null)
            {
                payment.PaymentStatus = "Failed";
                payment.FailureReason = paymentIntent.LastPaymentError?.Message ?? "Payment failed";

                await _context.SaveChangesAsync();

                _logger.LogWarning("Payment failed via webhook for order {OrderId}", orderId);
            }
        }
    }
}