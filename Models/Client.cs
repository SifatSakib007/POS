using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }
        public int? Invoice { get; set; }
        public string? Name { get; set; }
               
        [StringLength(200, ErrorMessage = "ঠিকানা ২০০ অক্ষরের বেশি হতে পারবে না।")]
        public string? Address { get; set; }

        
        [Phone(ErrorMessage = "ফোন নম্বর সঠিক নয়।")]
        public string? PhoneNo { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Debt { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? ClearDebt { get; set; }
        // New properties to store payment dates and amounts as comma-separated strings
        public string? AddDebtDates { get; set; }  // Stores the dates in "yyyy-MM-dd" format
        public string? AddDebtAmounts { get; set; } // Stores the amounts paid

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        // Navigation property for related products
        public ICollection<Product> Products { get; set; }
        public int? UserId { get; set; }  // References the user who added the product
        public Users? User { get; set; }  // Navigation property to reference the user
    }
}
