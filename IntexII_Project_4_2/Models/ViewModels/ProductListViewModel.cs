using IntexII_Project_4_2.Data;
using IntexII_Project_4_2.Models.ViewModels;

public class ProductListViewModel
{
    public IEnumerable<Product> Products { get; set; }
    public PaginationInfo PaginationInfo { get; set; } = new PaginationInfo();
    public string[] SelectedCategories { get; set; } // Array to hold selected categories
    public string[] SelectedColors { get; set; } // Array to hold selected colors
}