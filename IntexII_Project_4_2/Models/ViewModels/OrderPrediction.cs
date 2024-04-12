using IntexII_Project_4_2.Data;

namespace IntexII_Project_4_2.Models.ViewModels
{
    public class OrderPrediction
    {
        public Order Order { get; set; }
        public string Prediction { get; set; }

        public Customer Customer { get; set; }

        public Cart Cart { get; set; } // Add this

        // Constructor ensures Cart is never null
        public OrderPrediction()
        {
            Cart = new Cart();
        }
    }
}
