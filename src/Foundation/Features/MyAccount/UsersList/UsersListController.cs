using Foundation.Features.Login;
using Foundation.Features.Users;
using Foundation.Infrastructure.Cms.Settings;
using Foundation.Infrastructure.Cms.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Features.MyAccount.UsersList
{
    public class UsersListController : PageController<UsersListPage>
    {
        private readonly ISettingsService _settingsService;
        private readonly UserManager<SiteUser> _userManager;

        public UsersListController(ISettingsService settingsService, UserManager<SiteUser> userManager)
        {
            _settingsService = settingsService;
            _userManager = userManager;
        }

        public ActionResult Index(UsersListPage currentPage)
        {
            var usersList = new List<UsersViewModel>();
            var users = _userManager.Users.ToList();

            foreach (var user in users)
            {
                usersList.Add(new UsersViewModel
                {
                    UserName = user.UserName,
                    Email = user.Email,
                });
            }

            var viewModel = new UsersListViewModel(currentPage)
            {
                CurrentContent = currentPage,
                User = usersList,
            };

            return View(viewModel);
        }
    }
}
