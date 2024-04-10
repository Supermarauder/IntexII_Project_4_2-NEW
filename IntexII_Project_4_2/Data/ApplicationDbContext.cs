using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IntexII_Project_4_2.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Customer> Customers { get; set; }

        public virtual DbSet<LineItem> LineItems { get; set; }

        public virtual DbSet<Order> Orders { get; set; }

        public virtual DbSet<Product> Products { get; set; }

        public virtual DbSet<TopRecommendation> TopRecommendations { get; set; }
        public virtual DbSet<ItemRecommendation> ItemsRecommendations { get; set; }
        public virtual DbSet<CustomerRecommendation> CustomerRecommendations { get; set; }
    }
}
