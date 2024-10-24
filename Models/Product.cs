﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        public int? Invoice { get; set; }
        [Required(ErrorMessage = "পণ্যের নাম আবশ্যক।")]
        [StringLength(150, ErrorMessage = "পণ্যের নাম ১৫০ অক্ষরের বেশি হতে পারবে না।")]
        public required string ProductName { get; set; }
        [StringLength(20, ErrorMessage = "পণ্যের একক 2০ অক্ষরের বেশি হতে পারবে না।")]
        public string? Unit { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PreviousBuyPrice { get; set; }
        [Required(ErrorMessage = "ক্রয়মূল্য আবশ্যক।")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "ক্রয়মূল্য ০.০১ এর বেশি হতে হবে।")]
        public required decimal BuyPrice { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TotalPrice { get; set; }
        [Required(ErrorMessage = "স্টক সংখ্যা আবশ্যক।")]
        [Range(0, int.MaxValue, ErrorMessage = "স্টক সংখ্যা অবশ্যই শূন্য বা তার বেশি হতে হবে।")]
        public required int Stock { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? AddedBy { get; set; }
        public int? AddStock { get; set; }
        // New properties to store payment dates and amounts as comma-separated strings
        public string? AddStockDates { get; set; }  // Stores the dates in "yyyy-MM-dd" format
        public string? AddStockAmounts { get; set; } // Stores the amounts paid
        public int? ClientId { get; set; } // Foreign key
        // Navigation property
        public Client? Client { get; set; }
        // Foreign key relationship with Shop
        [ForeignKey("Shop")]
        public int? EmployeeId { get; set; }
        public Shop? Shop { get; set; }   // Navigation property
        // Foreign key relationship with Users
        [ForeignKey("Users")]
        public int? UserId { get; set; }  // References the user who added the product
        public Users? User { get; set; }  // Navigation property to reference the user
    }
}