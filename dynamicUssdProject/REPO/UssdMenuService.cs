using dynamicUssdProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dynamicUssdProject.REPO
{
    public class UssdMenuService : IUssdMenuService
    {
        private readonly ApplicationDbContext _context;

        public UssdMenuService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UssdMenu>> GetMenuOptionsAsync(int menuLevel, int? parentId)
        {
            return await _context.UssdMenus // Updated to use UssdMenus
                .Where(m => m.MenuLevel == menuLevel && m.ParentId == parentId)
                .ToListAsync();
        }

        public async Task<Menu> GetMenuOptionByIdAsync(int id)
        {
            // Fetch the main menu
            var menu = await _context.Menus
                .Where(m => m.Id == id && m.ParentId == null)
                .FirstOrDefaultAsync();

            if (menu == null)
            {
                return null; // Handle case when menu is not found
            }

            // Fetch the submenus related to this menu

            var subMenus = await _context.SubMenus
               .Where(s => s.ParentMenuId == menu.Id)
               .ToListAsync();
            // Set the submenus for the menu
            menu.SubMenus = subMenus;
            return menu;
        }
    }
}
