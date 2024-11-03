using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace POS.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Users> Users { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Shop> Shop { get; set; }
        public DbSet<Sell> Sell { get; set; }
        public DbSet<Customer> Customer{ get; set; }
        public DbSet<Client> Client { get; set; }
        public DbSet<Buy> Buy { get; set; }
        public DbSet<Expense> Expense { get; set; }
    }
}
