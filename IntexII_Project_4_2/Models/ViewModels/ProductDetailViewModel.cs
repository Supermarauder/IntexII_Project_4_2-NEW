using IntexII_Project_4_2.Data;

namespace IntexII_Project_4_2.Models.ViewModels
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; }
        public List<Product> Recommendations { get; set; }
    }
}