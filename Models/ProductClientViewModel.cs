namespace POS.Models
{
    public class ProductClientViewModel
    {
        public Product Product { get; set; } = new Product
        {
            ProductName = string.Empty,
            BuyPrice = 0.0m,
            Stock = 0,
            Unit = string.Empty
        };
        public Client Client { get; set; } = new Client
        {
            Products = new List<Product>() // Ensure the collection is initialized
        };
    }
}
