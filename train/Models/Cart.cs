using train.Models;

public class Cart
{
    public int Id { get; set; }

    // If user is logged in we bind cart to UserId, otherwise to SessionId
    public string? UserId { get; set; }
    public string? SessionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}