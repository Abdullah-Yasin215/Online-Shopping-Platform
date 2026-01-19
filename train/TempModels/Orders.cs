using System;
using System.Collections.Generic;

namespace train.TempModels;

public partial class Orders
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactName { get; set; }

    public string? ShippingAddress { get; set; }

    public string? City { get; set; }

    public string? ContactPhone { get; set; }

    public decimal Discount { get; set; }

    public string? PostalCode { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal Subtotal { get; set; }

    public virtual ICollection<OrderItems> OrderItems { get; set; } = new List<OrderItems>();

    public virtual ICollection<Payments> Payments { get; set; } = new List<Payments>();

    public virtual AspNetUsers? User { get; set; }
}
