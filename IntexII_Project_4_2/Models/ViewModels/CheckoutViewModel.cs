using IntexII_Project_4_2.Data;

namespace IntexII_Project_4_2.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public List<CheckoutItemViewModel> Items { get; set; } = new List<CheckoutItemViewModel>();
        public decimal GrandTotal { get; set; }
    }
}