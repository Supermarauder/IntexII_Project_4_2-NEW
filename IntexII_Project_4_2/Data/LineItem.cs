using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntexII_Project_4_2.Data;
[Keyless]
public partial class LineItem
{
    [ForeignKey("Order")]
    public int TransactionId { get; set; }
    [ForeignKey("Product")]
    public int ProductId { get; set; }
    [Required(ErrorMessage = "Please enter a quantity.")]
    public int Qty { get; set; }
    public int? Rating { get; set; }
}
