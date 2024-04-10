using IntexII_Project_4_2.Data;

namespace IntexII_Project_4_2.Models
{
    public interface IIntexProjectRepository
    {
        public IQueryable<Product> Products { get; }

    }
}
