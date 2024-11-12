namespace dynamicUssdProject.Models.Request
{
    public class TransferFundsToBankRequest : UssdRequest
    {
        public string BankAccountNumber { get; set; }
        public decimal Amount { get; set; }
    }

}
