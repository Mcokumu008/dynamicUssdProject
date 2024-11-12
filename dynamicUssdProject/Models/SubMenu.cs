namespace dynamicUssdProject.Models
{
    public class SubMenu
    {
        public int Id { get; set; }
        public string DisplayText { get; set; }
        public string ActionUrl { get; set; }
        public int ParentMenuId { get; set; } // Foreign key to the Parent menu

        public virtual Menu ParentMenu { get; set; }
    }
}
