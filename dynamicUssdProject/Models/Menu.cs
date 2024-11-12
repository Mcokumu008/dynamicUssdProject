namespace dynamicUssdProject.Models
{
    public class Menu
    {
        public int Id { get; set; }
        public string DisplayText { get; set; }
        public string ActionUrl { get; set; }
        public int? ParentId { get; set; } // Nullable for top-level menus

        // Navigation property for child items (submenus)
        public virtual ICollection<SubMenu> SubMenus { get; set; }
    }
}
