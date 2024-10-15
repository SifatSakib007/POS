namespace POS.Models
{
    public class ProductClientReportViewModel
    {
        public string ProductName { get; set; }
        public decimal BuyPrice { get; set; }
        public int Stock { get; set; }
        public string ClientName { get; set; }
        public string ClientAddress { get; set; }
        public string ClientPhoneNo { get; set; }
    }
}
