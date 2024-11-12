namespace dynamicUssdProject.Models
{
    public class RegisterAccountRequest
    {
        public string PhoneNumber { get; set; }
        public string Pin { get; set; }
        public decimal Balance { get; set; }

        public int UserId { get; set; }
    }
}
