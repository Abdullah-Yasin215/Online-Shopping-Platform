namespace train.Models
{
    public class ApplicationUser
    {
        // Extra fields for user profile
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Cart> Orders { get; set; } = new List<Cart>();

    }

}
