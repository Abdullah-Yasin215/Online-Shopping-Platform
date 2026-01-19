using System;
using System.Collections.Generic;

namespace train.TempModels;

public partial class ShippingMethods
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Cost { get; set; }

    public int DeliveryDays { get; set; }

    public bool IsActive { get; set; }
}
