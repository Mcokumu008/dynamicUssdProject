namespace dynamicUssdProject.Models.Request
{
    public class TillNumberRequest : UssdRequest
    {
        public string TillNumber { get; set; }
        public decimal Amount { get; set; }
    }
}
