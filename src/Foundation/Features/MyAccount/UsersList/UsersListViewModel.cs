using Foundation.Features.Login;
using Foundation.Features.Users;

namespace Foundation.Features.MyAccount.UsersList
{
    public class UsersListViewModel : ContentViewModel<UsersListPage>
    {
        public UsersListViewModel(UsersListPage currentPage) : base(currentPage)
        {
        }

        public List<UsersViewModel> User { get; set; }
    }
}
