namespace dynamicUssdProject.Models
{
    public class MenuSelectionRequest
    {
        public string PhoneNumber { get; set; }
        public string Pin { get; set; }
        public string SessionId { get; set; }
        public int SelectedOption { get; set; }
    }
}
