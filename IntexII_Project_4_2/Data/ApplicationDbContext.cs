﻿using IntexII_Project_4_2.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IntexII_Project_4_2.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<LineItem> LineItems { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ItemRecommendation> ItemRecommendations { get; set; }
        public DbSet<TopRecommendation> TopRecommendations { get; set; } // Make sure this is correctly defined
        public virtual DbSet<CustomerRecommendation> CustomerRecommendations { get; set; }
        
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // If you have a custom table name for ItemRecommendations or need to set schema, configure it here
            modelBuilder.Entity<ItemRecommendation>().ToTable("ItemRecommendations"); // Use your actual table name as in your database

            // Ensure the TopRecommendations table is configured if needed
            modelBuilder.Entity<TopRecommendation>().ToTable("TopRecommendations"); // Use your actual table name as in your database

            // Add configuration for other entities if needed
        }
    }
}
