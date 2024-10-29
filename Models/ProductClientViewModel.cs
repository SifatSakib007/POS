namespace POS.Models
{
    public class ProductClientViewModel
    {
        public int BuyId { get; set; }
        public string? Invoice { get; set; }
        public string? ProductIds { get; set; }
        public string ProductNames { get; set; }
        public string? PreviousBuyPrices { get; set; }
        public string? BuyPricePerProduct { get; set; }
        public string? Quantities { get; set; }
        public string? Unit { get; set; }

        public int? ClientId { get; set; }
        public string? ClientName { get; set; }
        public string? ClientAddress { get; set; }
        public string? ClientPhoneNo { get; set; }
        public string? ClientShopName { get; set; }
        public decimal? ShabekDue { get; set; } = 0;
        public decimal? Deposit { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? UserId { get; set; }
    }
}
