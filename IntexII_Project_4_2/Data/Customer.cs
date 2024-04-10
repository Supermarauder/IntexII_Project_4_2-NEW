using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IntexII_Project_4_2.Data;

public partial class Customer
{
    [Key]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Please enter your First name.")]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "Please enter your last name.")]
    public string LastName { get; set; }
    [Required(ErrorMessage = "Please enter your birthdate.")]
    public string BirthDate { get; set; }
    [Required(ErrorMessage = "Please enter your country of residence.")]
    public string CountryOfResidence { get; set; }
    [Required(ErrorMessage = "Please specify a gender.")]
    public string Gender { get; set; }
    [Required(ErrorMessage = "Please enter your age.")]
    public double Age { get; set; }
}
