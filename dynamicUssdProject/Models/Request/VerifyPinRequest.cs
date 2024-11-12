namespace dynamicUssdProject.Models.Request
{
    public class VerifyPinRequest
    {
        public string PhoneNumber { get; set; }
        public string Pin { get; set; }
    }
}
