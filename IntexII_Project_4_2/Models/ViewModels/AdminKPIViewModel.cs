using IntexII_Project_4_2.Data;

namespace IntexII_Project_4_2.Models.ViewModels
{
    public class AdminKPIViewModel
    {
        public int TotalSales2023 { get; set; }
        public int TotalSalesPast7Days { get; set; }
        public int UnfulfilledOrders { get; set; }
        public int OrdersFulfilledPast7Days { get; set; }
    }
}