using Microsoft.AspNetCore.Identity;
using train.Models;  // so it

namespace train.Areas.Identity.Data
{
    public class appusercontext : IdentityUser
    {
        public string City { get; set; }

        public int Age { get; set; }

        public ICollection<Cart> Orders { get; set; } = new List<Cart>();

    }
}
