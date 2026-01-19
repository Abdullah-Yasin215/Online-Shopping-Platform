using System;
using System.Collections.Generic;

namespace train.TempModels;

public partial class Categories
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? ParentCategoryId { get; set; }

    public string TargetAudience { get; set; } = null!;

    public string? Color { get; set; }

    public virtual ICollection<Categories> InverseParentCategory { get; set; } = new List<Categories>();

    public virtual Categories? ParentCategory { get; set; }

    public virtual ICollection<Products> Products { get; set; } = new List<Products>();
}
