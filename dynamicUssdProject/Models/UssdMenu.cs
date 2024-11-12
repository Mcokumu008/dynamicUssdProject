using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dynamicUssdProject.Models
{
    public class UssdMenu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int MenuLevel { get; set; }

        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public UssdMenu ParentMenu { get; set; }  // Reference to parent menu item

        public string DisplayText { get; set; }

        public string ResponseType { get; set; }

        public string ActionUrl { get; set; }

        public ICollection<UssdMenu> SubMenus { get; set; } = new List<UssdMenu>();  // Collection of child menu items
    }
}
