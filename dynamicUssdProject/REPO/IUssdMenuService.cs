using dynamicUssdProject.Models;

namespace dynamicUssdProject.REPO
{
    public interface IUssdMenuService
    {
        Task<IEnumerable<UssdMenu>> GetMenuOptionsAsync(int menuLevel, int? parentId);
        Task<Menu> GetMenuOptionByIdAsync(int id);
    }

}
