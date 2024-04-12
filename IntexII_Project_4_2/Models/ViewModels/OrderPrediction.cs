using IntexII_Project_4_2.Data;
using System.ComponentModel.DataAnnotations;
namespace IntexII_Project_4_2.Models.ViewModels
{
    public class OrderPrediction
    {
        public Order Order { get; set; }
        public string Prediction { get; set; }
        public Customer Customer { get; set; }
        public Cart Cart { get; set; } // Add this
        public int CustomerId { get; set; }
        // Constructor ensures Cart is never null
        public OrderPrediction()
        {
            Cart = new Cart();
        }

//indent
        public int TransactionId { get; set; }

        [DataType(DataType.Date)]
        public string Date { get; set; }

        public int Time { get; set; }
        public int Amount { get; set; }
        public string CountryOfTransaction { get; set; }
        public string ShippingAddress { get; set; }
        public string Bank { get; set; }
        public string TypeOfCard { get; set; }

        // Customer information
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CountryOfResidence { get; set; }

        // Additional fields as needed for your view
        public bool IsFraudulent { get; set; }
        public bool IsFullfilled { get; set; }
    }
}
