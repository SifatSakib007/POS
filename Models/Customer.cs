using System.ComponentModel.DataAnnotations;

namespace POS.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "গ্রাহকের নাম আবশ্যক।")]
        [StringLength(100, ErrorMessage = "গ্রাহকের নাম ১০০ অক্ষরের বেশি হতে পারবে না।")]
        public required string Name { get; set; }

        [StringLength(200, ErrorMessage = "ঠিকানা ২০০ অক্ষরের বেশি হতে পারবে না।")]
        public string? Address { get; set; }


        [Phone(ErrorMessage = "ফোন নম্বর সঠিক নয়।")]
        public string? PhoneNo { get; set; }

        public decimal? FirstDue { get; set; } = 0;
        public decimal? Due { get; set; }
        public decimal? DueClose { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        // New properties to store payment dates and amounts as comma-separated strings
        public string? PaymentDates { get; set; }  // Stores the dates in "yyyy-MM-dd" format
        public string? PaymentAmounts { get; set; } // Stores the amounts paid

        // Navigation property for the purchases made by this customer
        public ICollection<Sell> Purchases { get; set; } = new List<Sell>();

        public int? UserId { get; set; }  // References the user who added the product
        public Users? User { get; set; }  // Navigation property to reference the user
    }
}
