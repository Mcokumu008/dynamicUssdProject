using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dynamicUssdProject.Models
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }
        public int AccountId { get; set; } // Foreign key to Account
        public Account Account { get; set; } // Navigation property to Account
        public decimal TransactionCharge { get; set; }
        public decimal Amount { get; set; } // The amount of the transaction
        public string? TransactionType { get; set; } // E.g., "Deposit", "Withdrawal"
        public string? BankAccountNumber { get; set; }
        public string? PesalinkId { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string? PaybillNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? OtherPhoneNumber { get; set; }
        public string? TillNumber { get; set; }
        public string? UtilityAccountNumber { get; set; }

    }
}
