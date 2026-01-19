using System;
using System.Collections.Generic;

namespace train.TempModels;

public partial class OrderItems
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? CategoryName { get; set; }

    public string? Color { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public virtual Orders Order { get; set; } = null!;
}
