using System.ComponentModel.DataAnnotations;

namespace POS.Models
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "ব্যবহারকারীর নাম আবশ্যক।")]
        [StringLength(50, ErrorMessage = "ব্যবহারকারীর নাম ৫০ অক্ষরের বেশি হতে পারবে না।")]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "দোকানের নাম আবশ্যক।")]
        [StringLength(200, ErrorMessage = "দোকানের নাম ২০০ অক্ষরের বেশি হতে পারবে না।")]
        public required string ShopName { get; set; }

        [Required(ErrorMessage = "ঠিকানা আবশ্যক।")]
        [StringLength(200, ErrorMessage = "ঠিকানা ২০০ অক্ষরের বেশি হতে পারবে না।")]
        public required string Address { get; set; }

        public string? Role { get; set; }

        
        public string? Email { get; set; }

        [Required(ErrorMessage = "ফোন নম্বর আবশ্যক।")]
        [Phone(ErrorMessage = "ফোন নম্বর সঠিক নয়।")]
        public required string PhoneNo { get; set; }

        [Required(ErrorMessage = "পাসওয়ার্ড আবশ্যক।")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "পাসওয়ার্ড ১০০ অক্ষরের বেশি হতে পারবে না।")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "অনুগ্রহ করে আপনার পাসওয়ার্ড নিশ্চিত করুন।")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "পাসওয়ার্ডগুলি মেলেনি।")]
        public required string ConfirmPassword { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
        public string? Status { get; set; } = "Active";


        // Navigation property for the products added by the user
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Shop> Shops { get; set; } = new List<Shop>();
        public ICollection<Sell> Sells { get; set; } = new List<Sell>();
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public ICollection<Client> Clients { get; set; } = new List<Client>();
    }
}
