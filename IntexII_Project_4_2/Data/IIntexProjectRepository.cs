using IntexII_Project_4_2.Data;
using System.Linq;

namespace IntexII_Project_4_2.Models
{
    public interface IIntexProjectRepository
    {
        IQueryable<Product> Products { get; }
        IQueryable<ItemRecommendation> ItemRecommendations { get; }
        IQueryable<TopRecommendation> TopRecommendations { get; } // Added line for TopRecommendations

        IQueryable<CustomerRecommendation> CustomerRecommendations { get; } // Added line for TopRecommendations

        void AddOrder(Order order);
        IQueryable<CustomerRecommendation> CustomerRecommendations { get; }
    }
}