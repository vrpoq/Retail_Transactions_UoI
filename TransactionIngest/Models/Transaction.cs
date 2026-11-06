using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionIngest.Models
{
    public class Transaction
    {   
        [Display(Name = "Source System")]
        public string SourceSystem { get; set; } // TODO set max length < 20
        [Display(Name = "Transaction ID")]
        public int TransactionId { get; set; }
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; } // TODO set max length < 20
        [Display(Name = "Location Code")]
        public string LocationCode { get; set; } // TODO set max length < 20
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } // TODO set max length < 20
        public decimal Amount { get; set; } // TODO set to 18,2
        [Display(Name = "Transaction Time")]
        public DateTime TransactionTime { get; set; } // TODO make sure UTC
        public string AuditHistory { get; set; } // TODO set max length < 20

    }
}