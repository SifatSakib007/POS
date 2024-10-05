﻿using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace POS.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Users> Users { get; set; }
    }
}
