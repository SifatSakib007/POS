using System.ComponentModel.DataAnnotations;

namespace POS.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "ক্লায়েন্টের নাম আবশ্যক।")]
        [StringLength(100, ErrorMessage = "ক্লায়েন্টের নাম ১০০ অক্ষরের বেশি হতে পারবে না।")]
        public string? Name { get; set; }
               
        [StringLength(200, ErrorMessage = "ঠিকানা ২০০ অক্ষরের বেশি হতে পারবে না।")]
        public string? Address { get; set; }

        
        [Phone(ErrorMessage = "ফোন নম্বর সঠিক নয়।")]
        public string? PhoneNo { get; set; }
        public decimal? Debt { get; set; }
        public decimal? ClearDebt { get; set; }
        // New properties to store payment dates and amounts as comma-separated strings
        public string? AddDebtDates { get; set; }  // Stores the dates in "yyyy-MM-dd" format
        public string? AddDebtAmounts { get; set; } // Stores the amounts paid

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public int? UserId { get; set; }  // References the user who added the product
        public Users? User { get; set; }  // Navigation property to reference the user
    }
}
