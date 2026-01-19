// Models/Order.cs
using System;
using System.Collections.Generic;
using train.Areas.Identity.Data; // appusercontext

namespace train.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public appusercontext? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";

        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }     // if you need it
        public decimal Discount { get; set; }        // if you need it
        public decimal TotalAmount { get; set; }

        // Contact / shipping info (collected on /checkout/info)
        public string? ContactEmail { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? ShippingAddress { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}

