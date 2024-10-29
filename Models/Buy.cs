using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    public class Buy
    {
        [Key]
        public int BuyId { get; set; }
        public string? Invoice { get; set; }
        public string ProductIds { get; set; }

        [Required(ErrorMessage = "পণ্যের নাম আবশ্যক।")]
        public string ProductNames { get; set; }
        public string PreviousBuyPrices { get; set; }

        [Required(ErrorMessage = "ক্রয় মূল্য আবশ্যক।")]
        public string? BuyPricePerProduct { get; set; }
        [Required(ErrorMessage = "পরিমাণ আবশ্যক।")]
        public string Quantities { get; set; }

        [StringLength(20)]
        public string Unit { get; set; }        

        // For reference to the client (optional if not every buy has an associated client)
        [ForeignKey("Client")]
        public int? ClientId { get; set; }
        public Client Client { get; set; }  // Navigation property
        public string? ClientName { get; set; }  // Navigation property
        public string? ClientAddress { get; set; }  // Navigation property
        public string? ClientPhoneNo { get; set; }  // Navigation property
        public string? ClientShopName { get; set; }  // Navigation property
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TotalSum { get; set; } = 0;
        public decimal? ShabekDue { get; set; } = 0;
        public decimal? Deposit { get; set; } = 0;

        // Additional tracking fields
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? UserId { get; set; }  // User who added the buy entry
    }
}
