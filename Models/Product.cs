using System.ComponentModel.DataAnnotations;

namespace POS.Models
{
    public class Product
    {
        /*        Product {
            ProductId integer
            ProductName string
            BuyPrice decimal
            SellPrice decimal
            Stock int
            ShopId int
            Shop integer
            }*/

        [Key]
        public int Product_Id { get; set; }

        [StringLength(150, ErrorMessage ="Product Name required")]
        public required string Product_Name { get; set; }

        
        public required decimal BuyPrice { get; set; }
        public required decimal SellPrice { get; set; }
        public required int Stock { get; set; }
        public required shop ShopId { get; set; }
        
}

}
