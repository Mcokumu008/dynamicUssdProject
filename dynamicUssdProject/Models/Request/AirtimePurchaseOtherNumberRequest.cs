namespace dynamicUssdProject.Models.Request
{
    public class AirtimePurchaseOtherNumberRequest : UssdRequest
    {
        public string OtherNumber { get; set; }
        public decimal Amount { get; set; }
    }
}
