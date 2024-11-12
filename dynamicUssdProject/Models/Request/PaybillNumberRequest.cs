namespace dynamicUssdProject.Models.Request
{
    public class PaybillNumberRequest : UssdRequest
    {
        public string PaybillNumber { get; set; }
        public decimal Amount { get; set; }
    }
}
