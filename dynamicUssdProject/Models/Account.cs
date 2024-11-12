namespace dynamicUssdProject.Models
{
    public class Account
    {
        public int Id { get; set; } // Primary key
        public string PhoneNumber { get; set; } // Phone number as a unique identifier
        public string Pin { get; set; } // PIN for account verification
        public decimal Balance { get; set; } // Account balance

       
        public int UserId { get; set; }
        public User User { get; set; }  // Assuming each Account belongs to one User
        public ICollection<Transaction> Transactions { get; set; }
    }
}
