using System;
using System.Collections.Generic;

namespace train.TempModels;

public partial class AspNetUsers
{
    public string Id { get; set; } = null!;

    public string City { get; set; } = null!;

    public int Age { get; set; }

    public string? UserName { get; set; }

    public string? NormalizedUserName { get; set; }

    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public virtual ICollection<AspNetUserClaims> AspNetUserClaims { get; set; } = new List<AspNetUserClaims>();

    public virtual ICollection<AspNetUserLogins> AspNetUserLogins { get; set; } = new List<AspNetUserLogins>();

    public virtual ICollection<AspNetUserTokens> AspNetUserTokens { get; set; } = new List<AspNetUserTokens>();

    public virtual ICollection<Carts> CartsAppusercontext { get; set; } = new List<Carts>();

    public virtual ICollection<Carts> CartsUser { get; set; } = new List<Carts>();

    public virtual ICollection<Orders> Orders { get; set; } = new List<Orders>();

    public virtual ICollection<AspNetRoles> Role { get; set; } = new List<AspNetRoles>();
}
