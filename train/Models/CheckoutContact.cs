namespace train.Repositories.Interface
{
    public record CheckoutContact(
        string? Email,
        string? Name,
        string? Phone,
        string? City,
        string? PostalCode,
        string? Address
    );
}
