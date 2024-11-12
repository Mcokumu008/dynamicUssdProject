namespace dynamicUssdProject.Models.Request
{
    public class TransferFundsToMobileRequest : UssdRequest
    {
        public string RecipientNumber { get; set; }
        public decimal Amount { get; set; }
    }
}
