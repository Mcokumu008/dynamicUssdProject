namespace dynamicUssdProject.Models.Request
{
    public class UpdatePinRequest
    {
        public string PhoneNumber { get; set; }
        public string NewPin { get; set; }
    }
}
