using IntexII_Project_4_2.Data;
using System.ComponentModel.DataAnnotations;

namespace IntexII_Project_4_2.Models.ViewModels
{
    public class EditOrderViewModel
    {
        // Order information
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
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CountryOfResidence { get; set; }

        // Additional fields as needed for your view
        public bool IsFraudulent { get; set; }
        public bool IsFullfilled { get; set; }
    }
}