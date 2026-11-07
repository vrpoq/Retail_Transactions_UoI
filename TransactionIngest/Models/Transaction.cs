using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace TransactionIngest.Models
{
    public class Transaction
    {   
        [StringLength(19)]
        [Display(Name = "Source System")]
        [Key, Column(Order = 0)]
        public string SourceSystem { get; set; }
        [Display(Name = "Transaction ID")]
        [Key, Column(Order = 1)]
        public int TransactionId { get; set; }
        [StringLength(19)]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }
        [StringLength(19)]
        [Display(Name = "Location Code")]
        public string LocationCode { get; set; }
        [StringLength(19)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }
        public decimal Amount { get; set; }
        [StringLength(21)] // 18 places before decimal + 1 decimal + 2 places after decimal
        [Display(Name = "Amount")]
        public string AmountDisplay{ get { return Math.Round(Amount, 2).ToString();} } 
        [Display(Name = "Transaction Time")]
        public DateTime TransactionTime { get; set; } // already in UTC
        [StringLength(19)]
        public string AuditHistory { get; set; }

    }
}