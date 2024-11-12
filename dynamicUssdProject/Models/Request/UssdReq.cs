namespace dynamicUssdProject.Models.Request
{
    public class UssdReq
    {
        public string SessionId { get; set; }
        public string UserInput { get; set; }
        public string PhoneNumber { get; set; } // Add PhoneNumber here
    }
}
