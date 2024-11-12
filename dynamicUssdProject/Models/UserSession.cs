using System.ComponentModel.DataAnnotations;

namespace dynamicUssdProject.Models
{
    public class UserSession
    {
        [Key]
        public string SessionId { get; set; }  // Unique identifier for the session
        public string UserId { get; set; }      // ID of the user associated with this session
        public int? CurrentMenuId { get; set; } // ID of the current menu the user is on
        public DateTime LastAccessed { get; set; }
    }
}
