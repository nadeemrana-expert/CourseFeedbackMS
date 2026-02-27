using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using CourseFeedbackMS.Authorization;
using CourseFeedbackMS.Authorization.Roles;
using CourseFeedbackMS.Authorization.Users;

namespace CourseFeedbackMS.EntityFrameworkCore.Seed.Tenants
{
    public class TenantRoleAndUserBuilder
    {
        private readonly CourseFeedbackMSDbContext _context;
        private readonly int _tenantId;

        public TenantRoleAndUserBuilder(CourseFeedbackMSDbContext context, int tenantId)
        {
            _context = context;
            _tenantId = tenantId;
        }

        public void Create()
        {
            CreateRolesAndUsers();
        }

        private void CreateRolesAndUsers()
        {
            // Admin role
            var adminRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Admin);
            if (adminRole == null)
            {
                adminRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.Admin, StaticRoleNames.Tenants.Admin) { IsStatic = true }).Entity;
                _context.SaveChanges();
            }

            // Grant all permissions to admin role EXCEPT feedback create/edit
            // (Admins moderate the system but don't submit student feedback)
            var adminExcludedPermissions = new[]
            {
                PermissionNames.Pages_Feedbacks_Create,
                PermissionNames.Pages_Feedbacks_Edit
            };

            // Remove if previously granted
            var staleAdminPerms = _context.Permissions.IgnoreQueryFilters()
                .OfType<RolePermissionSetting>()
                .Where(p => p.TenantId == _tenantId && p.RoleId == adminRole.Id &&
                            adminExcludedPermissions.Contains(p.Name))
                .ToList();
            if (staleAdminPerms.Any())
            {
                _context.Permissions.RemoveRange(staleAdminPerms);
                _context.SaveChanges();
            }

            var grantedPermissions = _context.Permissions.IgnoreQueryFilters()
                .OfType<RolePermissionSetting>()
                .Where(p => p.TenantId == _tenantId && p.RoleId == adminRole.Id)
                .Select(p => p.Name)
                .ToList();

            var permissions = PermissionFinder
                .GetAllPermissions(new CourseFeedbackMSAuthorizationProvider())
                .Where(p => p.MultiTenancySides.HasFlag(MultiTenancySides.Tenant) &&
                            !grantedPermissions.Contains(p.Name) &&
                            !adminExcludedPermissions.Contains(p.Name))
                .ToList();

            if (permissions.Any())
            {
                _context.Permissions.AddRange(
                    permissions.Select(permission => new RolePermissionSetting
                    {
                        TenantId = _tenantId,
                        Name = permission.Name,
                        IsGranted = true,
                        RoleId = adminRole.Id
                    })
                );
                _context.SaveChanges();
            }

            // Admin user
            var adminUser = _context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.TenantId == _tenantId && u.UserName == AbpUserBase.AdminUserName);
            if (adminUser == null)
            {
                adminUser = User.CreateTenantAdminUser(_tenantId, "admin@defaulttenant.com");
                adminUser.Password = new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions())).HashPassword(adminUser, "123qwe");
                adminUser.IsEmailConfirmed = true;
                adminUser.IsActive = true;

                _context.Users.Add(adminUser);
                _context.SaveChanges();

                // Assign Admin role to admin user
                _context.UserRoles.Add(new UserRole(_tenantId, adminUser.Id, adminRole.Id));
                _context.SaveChanges();
            }

            // ── Teacher role ──
            CreateTeacherRole();

            // ── Student role ──
            CreateStudentRole();
        }

        private void CreateTeacherRole()
        {
            var teacherRole = _context.Roles.IgnoreQueryFilters()
                .FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Teacher);

            if (teacherRole == null)
            {
                teacherRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.Teacher, StaticRoleNames.Tenants.Teacher)
                {
                    IsStatic = true,
                    IsDefault = false
                }).Entity;
                _context.SaveChanges();
            }

            // Teacher permissions: view courses, view feedbacks, dashboard (NO create/edit/delete)
            var teacherPermissionNames = new[]
            {
                PermissionNames.Pages_Dashboard,
                PermissionNames.Pages_Courses,
                PermissionNames.Pages_Feedbacks
            };

            // Remove stale teacher permissions that should no longer exist
            var teacherRevokedPermissions = new[]
            {
                PermissionNames.Pages_Courses_Create,
                PermissionNames.Pages_Courses_Edit,
                PermissionNames.Pages_Courses_Delete,
                PermissionNames.Pages_Feedbacks_Create,
                PermissionNames.Pages_Feedbacks_Edit,
                PermissionNames.Pages_Feedbacks_Delete
            };
            var staleTeacherPerms = _context.Permissions.IgnoreQueryFilters()
                .OfType<RolePermissionSetting>()
                .Where(p => p.TenantId == _tenantId && p.RoleId == teacherRole.Id &&
                            teacherRevokedPermissions.Contains(p.Name))
                .ToList();
            if (staleTeacherPerms.Any())
            {
                _context.Permissions.RemoveRange(staleTeacherPerms);
                _context.SaveChanges();
            }

            var existingTeacherPerms = _context.Permissions.IgnoreQueryFilters()
                .OfType<RolePermissionSetting>()
                .Where(p => p.TenantId == _tenantId && p.RoleId == teacherRole.Id)
                .Select(p => p.Name)
                .ToList();

            var missingTeacherPerms = teacherPermissionNames
                .Where(p => !existingTeacherPerms.Contains(p))
                .ToList();

            if (missingTeacherPerms.Any())
            {
                _context.Permissions.AddRange(
                    missingTeacherPerms.Select(perm => new RolePermissionSetting
                    {
                        TenantId = _tenantId,
                        Name = perm,
                        IsGranted = true,
                        RoleId = teacherRole.Id
                    })
                );
                _context.SaveChanges();
            }
        }

        private void CreateStudentRole()
        {
            var studentRole = _context.Roles.IgnoreQueryFilters()
                .FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Student);

            if (studentRole == null)
            {
                studentRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.Student, StaticRoleNames.Tenants.Student)
                {
                    IsStatic = true,
                    IsDefault = true  // New registrations default to Student
                }).Entity;
                _context.SaveChanges();
            }

            // Student permissions: view courses, create/edit/delete own feedbacks, dashboard
            var studentPermissionNames = new[]
            {
                PermissionNames.Pages_Dashboard,
                PermissionNames.Pages_Courses,
                PermissionNames.Pages_Feedbacks,
                PermissionNames.Pages_Feedbacks_Create,
                PermissionNames.Pages_Feedbacks_Edit,
                PermissionNames.Pages_Feedbacks_Delete
            };

            var existingStudentPerms = _context.Permissions.IgnoreQueryFilters()
                .OfType<RolePermissionSetting>()
                .Where(p => p.TenantId == _tenantId && p.RoleId == studentRole.Id)
                .Select(p => p.Name)
                .ToList();

            var missingStudentPerms = studentPermissionNames
                .Where(p => !existingStudentPerms.Contains(p))
                .ToList();

            if (missingStudentPerms.Any())
            {
                _context.Permissions.AddRange(
                    missingStudentPerms.Select(perm => new RolePermissionSetting
                    {
                        TenantId = _tenantId,
                        Name = perm,
                        IsGranted = true,
                        RoleId = studentRole.Id
                    })
                );
                _context.SaveChanges();
            }
        }
    }
}
