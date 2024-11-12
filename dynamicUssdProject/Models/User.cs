namespace dynamicUssdProject.Models
{
    public class User
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string? Name { get; set; } // User's name
        

        public Account Account { get; set; } 

    }
}
