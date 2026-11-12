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
        [Display(Name = "24-Hour Range")]
        public DateTime FullDayRange { get; set; } // already in UTC
        [StringLength(19)]
        [Display(Name = "Audit History")]
        public string AuditHistory { get; set; }
        [Display(Name = "Revision Number")]
        public int RevisionNumber { get; set; }

        public static Transaction DeepCopy(Transaction inTransaction)
        {
            Transaction deepCopyTrans = new Transaction(); 

            deepCopyTrans.SourceSystem = inTransaction.SourceSystem;
            deepCopyTrans.TransactionId = inTransaction.TransactionId;
            deepCopyTrans.CardNumber = inTransaction.CardNumber;
            deepCopyTrans.LocationCode = inTransaction.LocationCode;
            deepCopyTrans.ProductName = inTransaction.ProductName;
            deepCopyTrans.Amount = inTransaction.Amount;
            deepCopyTrans.TransactionTime = inTransaction.TransactionTime;
            deepCopyTrans.FullDayRange = inTransaction.FullDayRange;
            deepCopyTrans.AuditHistory = inTransaction.AuditHistory;
            deepCopyTrans.RevisionNumber = inTransaction.RevisionNumber;

            return deepCopyTrans;
        }

    }
}