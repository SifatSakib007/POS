using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    public class Sell
    {
        [Key]
        public int SellId { get; set; }

        [ForeignKey("Product")]
        public int? ProductId { get; set; }

        [Required(ErrorMessage = "প্রোডাক্ট নাম আবশ্যক")]
        public required string ProductName { get; set; }
        public Product? Product { get; set; }  // Navigation property

        public decimal? BuyPrice { get; set; }
        public int? Stock { get; set; }

        [Required(ErrorMessage = "পরিমাণ আবশ্যক।")]
        public required int Quantity { get; set; }

        [Required(ErrorMessage = "বিক্রয়মূল্য আবশ্যক।")]
        public decimal SellingPrice { get; set; }

        [Required(ErrorMessage = "মোট মূল্য আবশ্যক।")]
        public decimal TotalPrice { get; set; }
        public required decimal TotalTotalPrice { get; set; }

        public decimal? DuePrice { get; set; }

        [Required(ErrorMessage = "ডিপোজিট আবশ্যক।")]
        public decimal Deposit { get; set; }

        public decimal? TotalDuePrice { get; set; }

        [StringLength(100, ErrorMessage = "গ্রাহকের নাম ১০০ অক্ষরের বেশি হতে পারবে না।")]
        public string? CustomerName { get; set; }

        [Phone(ErrorMessage = "ফোন নম্বর সঠিক নয়।")]
        public string? CustomerPhoneNo { get; set; }

        [StringLength(300, ErrorMessage = "ঠিকানা ৩০০ অক্ষরের বেশি হতে পারবে না।")]
        public string? CustomerAddress { get; set; }

        [ForeignKey("Customer")]
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }  // Navigation property

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(50, ErrorMessage = "যোগ করা ব্যক্তির নাম ৫০ অক্ষরের বেশি হতে পারবে না।")]
        public string? AddedBy { get; set; }
        public required decimal ShabekDue { get; set; } = 0; 

        public int? UserId { get; set; }  // References the user who added the product
        public Users? User { get; set; }  // Navigation property to reference the user


    }
}
