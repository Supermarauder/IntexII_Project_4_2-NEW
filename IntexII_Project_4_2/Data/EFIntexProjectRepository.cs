using IntexII_Project_4_2.Data;
using System.Linq;

namespace IntexII_Project_4_2.Models
{
    public class EFIntexProjectRepository : IIntexProjectRepository
    {
        private ApplicationDbContext _context;

        public EFIntexProjectRepository(ApplicationDbContext temp)
        {
            _context = temp;
        }

        public IQueryable<Product> Products => _context.Products;
        public IQueryable<ItemRecommendation> ItemRecommendations => _context.ItemRecommendations;

        // Implement the TopRecommendations property to satisfy the interface contract
        public IQueryable<TopRecommendation> TopRecommendations => _context.TopRecommendations;

        public void AddOrder(Order order)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
        }
    }
}
