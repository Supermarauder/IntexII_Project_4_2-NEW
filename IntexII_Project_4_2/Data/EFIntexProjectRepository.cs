
using IntexII_Project_4_2.Data;

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
    }
}
