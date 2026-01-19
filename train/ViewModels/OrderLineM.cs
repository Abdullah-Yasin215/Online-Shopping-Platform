namespace train.ViewModels;

public class OrderLineVM
{
    public string Name { get; set; } = "";
    public string? Category { get; set; }
    public string? Color { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}