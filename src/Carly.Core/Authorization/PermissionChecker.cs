using Abp.Authorization;
using Carly.Authorization.Roles;
using Carly.Authorization.Users;

namespace Carly.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
