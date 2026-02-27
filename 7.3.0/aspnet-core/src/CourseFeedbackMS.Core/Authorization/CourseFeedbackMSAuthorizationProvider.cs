using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace CourseFeedbackMS.Authorization
{
    public class CourseFeedbackMSAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
            context.CreatePermission(PermissionNames.Pages_Users_Activation, L("UsersActivation"));
            context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
            context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);

            // Dashboard
            context.CreatePermission(PermissionNames.Pages_Dashboard, L("Dashboard"), multiTenancySides: MultiTenancySides.Tenant);

            // Courses
            var courses = context.CreatePermission(PermissionNames.Pages_Courses, L("Courses"), multiTenancySides: MultiTenancySides.Tenant);
            courses.CreateChildPermission(PermissionNames.Pages_Courses_Create, L("CoursesCreate"), multiTenancySides: MultiTenancySides.Tenant);
            courses.CreateChildPermission(PermissionNames.Pages_Courses_Edit, L("CoursesEdit"), multiTenancySides: MultiTenancySides.Tenant);
            courses.CreateChildPermission(PermissionNames.Pages_Courses_Delete, L("CoursesDelete"), multiTenancySides: MultiTenancySides.Tenant);

            // Feedbacks
            var feedbacks = context.CreatePermission(PermissionNames.Pages_Feedbacks, L("Feedbacks"), multiTenancySides: MultiTenancySides.Tenant);
            feedbacks.CreateChildPermission(PermissionNames.Pages_Feedbacks_Create, L("FeedbacksCreate"), multiTenancySides: MultiTenancySides.Tenant);
            feedbacks.CreateChildPermission(PermissionNames.Pages_Feedbacks_Edit, L("FeedbacksEdit"), multiTenancySides: MultiTenancySides.Tenant);
            feedbacks.CreateChildPermission(PermissionNames.Pages_Feedbacks_Delete, L("FeedbacksDelete"), multiTenancySides: MultiTenancySides.Tenant);
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, CourseFeedbackMSConsts.LocalizationSourceName);
        }
    }
}
