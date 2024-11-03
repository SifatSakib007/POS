using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    public class Expense
    {
        [Key]
        public int Id { get; set; }

        public string? ExpenseName { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TotalAmount { get; set; }

    }
}
