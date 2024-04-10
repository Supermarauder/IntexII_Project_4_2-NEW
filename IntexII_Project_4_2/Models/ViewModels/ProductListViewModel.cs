using IntexII_Project_4_2.Data;

namespace IntexII_Project_4_2.Models.ViewModels
{
    public class ProductListViewModel
    {
        public IEnumerable<Product> Products { get; set; }  // Or use List<Product> if you prefer

        public PaginationInfo PaginationInfo { get; set; } = new PaginationInfo();
    }
}