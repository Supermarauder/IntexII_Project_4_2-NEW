using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntexII_Project_4_2.Data
{
    public class TopRecommendation
    {
        [ForeignKey("Product")]
        [Key]
        public int ProductID { get; set; }

        public double Rating { get; set; }
    }
}
