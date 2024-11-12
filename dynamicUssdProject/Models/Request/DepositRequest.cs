namespace dynamicUssdProject.Models.Request
{
    public class DepositRequest
    {
        public string PhoneNumber { get; set; }
        public string Pin { get; set; }
        public decimal Amount { get; set; }
    }
}
