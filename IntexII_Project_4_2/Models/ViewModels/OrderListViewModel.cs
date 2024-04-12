using IntexII_Project_4_2.Data;
using IntexII_Project_4_2.Models.ViewModels;

public class OrderListViewModel
{
    public IEnumerable<Order> Orders { get; set; }
    public PaginationInfo PaginationInfo { get; set; }
    public string CurrentFilter { get; set; }
}