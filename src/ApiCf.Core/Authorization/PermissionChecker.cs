using Abp.Authorization;
using ApiCf.Authorization.Roles;
using ApiCf.Authorization.Users;

namespace ApiCf.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}




