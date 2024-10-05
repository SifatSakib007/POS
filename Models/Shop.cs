using System;

namespace POS.Models
{
    public class Shop
    {
        /*ClientName string
   Shop integer
   ShopAddress string
   CreatedAt datetime
   ShopId integer
   ShopName string
   ClientId int
   Products integer*/
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string ShopName { get; set; }
        public string ShopAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        

    }
}
