using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    public class Sell
    {
        /*        Sell {
            SaleId integer
            ProductId int
            ProductName integer
            Quantity int
            TotalPrice decimal
            DuePrice decimal
            Deposit decimal
            ShopId int
            ShopName string
            ClientId int
            ClientName string
            CustomerId int
            CustomerName string
            }*/

        [Key]
        public int Sell_Id { get; set; }
        
        public int Product_Id { get; set; }

        public string Product_Name { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice {  get; set; }
        public decimal DuePrice { get; set; }
        public decimal Deposit {  get; set; }


}
}
