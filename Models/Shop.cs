using System.ComponentModel.DataAnnotations;

namespace POS.Models
{
    public class Shop
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "অংশীদারের নাম আবশ্যক।")]
        [StringLength(100, ErrorMessage = "অংশীদারের নাম ১০০ অক্ষরের বেশি হতে পারবে না।")]
        public required string EmployeeName { get; set; }
        
        [Phone(ErrorMessage = "ফোন নম্বর সঠিক নয়।")]
        public string? PhoneNo { get; set; }

        [Required(ErrorMessage = "দোকানের নাম আবশ্যক।")]
        [StringLength(200, ErrorMessage = "দোকানের নাম ২০০ অক্ষরের বেশি হতে পারবে না।")]
        public required string ShopName { get; set; }

        [StringLength(300, ErrorMessage = "ঠিকানা ৩০০ অক্ষরের বেশি হতে পারবে না।")]
        public string? ShopAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? UserId { get; set; }  // References the user who added the product
        public Users? User { get; set; }  // Navigation property to reference the user
        // Navigation property to track the products in this shop
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
