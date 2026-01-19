using System;
using System.Collections.Generic;

namespace train.TempModels;

public partial class Payments
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public string? TransactionId { get; set; }

    public string? PaymentGateway { get; set; }

    public decimal Amount { get; set; }

    public string? MaskedCardNumber { get; set; }

    public string? CardHolderName { get; set; }

    public string? WalletProvider { get; set; }

    public string? AccountNumber { get; set; }

    public DateTime PaymentDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public string? FailureReason { get; set; }

    public string? GatewayResponse { get; set; }

    public virtual Orders Order { get; set; } = null!;
}
