using System;
using System.Collections.Generic;

namespace train.TempModels;

public partial class Carts
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public string? SessionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? AppusercontextId { get; set; }

    public virtual AspNetUsers? Appusercontext { get; set; }

    public virtual ICollection<CartItems> CartItems { get; set; } = new List<CartItems>();

    public virtual AspNetUsers? User { get; set; }
}
