namespace POS.Models
{
    public class ProductDetailsViewModel
    {
        public int ProductId { get; set; } // The ID of the product
        public string ProductName { get; set; } // The name of the product
        public int Quantity { get; set; } // The quantity of the product
        public decimal TotalPrice { get; set; } // Total price for the specific product
    }
}
