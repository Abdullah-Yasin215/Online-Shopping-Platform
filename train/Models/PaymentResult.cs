namespace train.Models
{
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string GatewayResponse { get; set; } = string.Empty;
    }
}
