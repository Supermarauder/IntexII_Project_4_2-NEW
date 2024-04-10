using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntexII_Project_4_2.Data
{
    public class CustomerRecommendation
    {
        [ForeignKey("Customer")]
        [Key]
        public int CustomerID { get; set; }

        public int Recommendation1 { get; set; }
        public int Recommendation2 { get; set; }
        public int Recommendation3 { get; set; }
        public int Recommendation4 { get; set; }
        public int Recommendation5 { get; set; }
    }
}
