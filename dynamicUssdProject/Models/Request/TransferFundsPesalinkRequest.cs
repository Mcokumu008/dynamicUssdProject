namespace dynamicUssdProject.Models.Request
{
    public class TransferFundsPesalinkRequest : UssdRequest
    {
        public string PesalinkId { get; set; }
        public decimal Amount { get; set; }
    }
}
