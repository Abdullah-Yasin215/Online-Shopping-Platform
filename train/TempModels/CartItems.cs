using System;
using System.Collections.Generic;

namespace train.TempModels;

public partial class CartItems
{
    public int Id { get; set; }

    public int CartId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public string? SelectedColor { get; set; }

    public string? SelectedSize { get; set; }

    public virtual Carts Cart { get; set; } = null!;

    public virtual Products Product { get; set; } = null!;
}
