using System;
using System.Collections.Generic;

namespace train.TempModels;

public partial class Products
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string? ImageUrl { get; set; }

    public string TargetAudience { get; set; } = null!;

    public int CategoryId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Colors { get; set; }

    public string? Sizes { get; set; }

    public virtual ICollection<CartItems> CartItems { get; set; } = new List<CartItems>();

    public virtual Categories Category { get; set; } = null!;
}
