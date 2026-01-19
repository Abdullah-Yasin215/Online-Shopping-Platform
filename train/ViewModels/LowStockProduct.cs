// ViewModels/LowStockProduct.cs
namespace train.ViewModels
{
    public class LowStockProduct
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Stock { get; set; }
    }
}