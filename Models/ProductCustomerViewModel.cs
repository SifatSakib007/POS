using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    public class ProductCustomerViewModel
    {
        // List of products to select from
        public List<Product>? Products { get; set; }

        // List of customers to select from
        public List<Customer>? Customers { get; set; }

        // Sell properties to store after submission
        public Sell? Sells { get; set; }

        // Additional fields that may be relevant for submission
        public int SelectedProductId { get; set; } // Stores selected product
        public int SelectedCustomerId { get; set; } // Stores selected customer
        public string ProductName { get; set; }
        public int? Invoice { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public int Stock { get; set; } // For stock details
        public int Quantity { get; set; } // For stock details

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BuyPrice { get; set; } // For selling price

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SellingPrice { get; set; } // For selling price

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; } // For calculation of total price

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalTotalPrice { get; set; } // For total price

        public string? CustomerPhoneNo { get; set; } // For customer phone number
        public string? CustomerAddress { get; set; } // For customer address

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Deposit { get; set; } // For deposit

        [Column(TypeName = "decimal(18, 2)")]

        public decimal ShabekDue { get; set; } // For previous due amount
    }
}