using Abp.Authorization;
using CourseFeedbackMS.Authorization.Roles;
using CourseFeedbackMS.Authorization.Users;

namespace CourseFeedbackMS.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
