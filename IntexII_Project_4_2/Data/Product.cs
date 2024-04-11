using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Add this to use DatabaseGenerated attribute

namespace IntexII_Project_4_2.Data
{
    public partial class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // This specifies that ProductId should be auto-generated
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Please enter a name.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Please enter a valid year.")]
        [Range(1900, 2100)] // Assuming years are between 1900 and 2100
        public int? Year { get; set; }

        [Required(ErrorMessage = "Please enter the Number of parts.")]
        public int? NumParts { get; set; }

        [Required(ErrorMessage = "Please enter a price.")]
        public int? Price { get; set; }

        public string? ImgLink { get; set; }

        public string? PrimaryColor { get; set; }

        public string? SecondaryColor { get; set; }

        [Required(ErrorMessage = "Please enter a description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        public string? Category { get; set; }
    }
}