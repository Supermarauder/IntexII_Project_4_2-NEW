using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntexII_Project_4_2.Data;

public partial class Order
{
    [Key]
    public int TransactionId { get; set; }
    [ForeignKey("Customer")]
    public int? CustomerId { get; set; }
    [Required(ErrorMessage = "Please select a date.")]
    public string Date { get; set; }
    [Required]
    public string DayOfWeek { get; set; }
    [Required(ErrorMessage = "Please pick a time.")]
    public int Time { get; set; }
    [Required(ErrorMessage = "Please indicate an entry mode")]
    public string EntryMode { get; set; }
    [Required(ErrorMessage = "Please enter an amount")]
    public int Amount { get; set; }
    [Required(ErrorMessage = "Please indicate transaction type.")]
    public string TypeOfTransaction { get; set; }
    [Required(ErrorMessage = "Please select a country of Transaction")]
    public string CountryOfTransaction { get; set; }
    [Required(ErrorMessage = "Please enter a shipping address.")]
    public string ShippingAddress { get; set; }
    [Required(ErrorMessage = "Please select a bank.")]
    public string Bank { get; set; }
    [Required(ErrorMessage = "Please specify card type")]
    public string TypeOfCard { get; set; }
    [Required]
    public int Fraud { get; set; }
    [Required(ErrorMessage = "Please indicate if the order is fulfilled or not.")]
    public bool Fullfilled { get; set; } = true;

}
