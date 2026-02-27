using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Abp.Authorization.Users;
using CourseFeedbackMS.Authorization.Roles;
using CourseFeedbackMS.Authorization.Users;
using CourseFeedbackMS.CourseFeedback;

namespace CourseFeedbackMS.EntityFrameworkCore.Seed
{
    public class CourseFeedbackDataSeeder
    {
        private readonly CourseFeedbackMSDbContext _context;

        public CourseFeedbackDataSeeder(CourseFeedbackMSDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            CreateCourses();
            CreateSampleUsers();
            CreateSampleFeedbacks();
        }

        private void CreateCourses()
        {
            if (!_context.Courses.Any())
            {
                _context.Courses.AddRange(
                    new Course
                    {
                        TenantId = 1,
                        CourseName = "Angular Fundamentals",
                        InstructorName = "John Doe",
                        IsActive = true
                    },
                    new Course
                    {
                        TenantId = 1,
                        CourseName = "ASP.NET Core Deep Dive",
                        InstructorName = "Jane Smith",
                        IsActive = true
                    },
                    new Course
                    {
                        TenantId = 1,
                        CourseName = "SQL Server Essentials",
                        InstructorName = "Bob Lee",
                        IsActive = true
                    }
                );
                _context.SaveChanges();
            }
        }

        private void CreateSampleUsers()
        {
            var passwordHasher = new PasswordHasher<User>(
                new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions()));

            // ── Teacher users ──
            CreateUserIfNotExists("teacher1", "John", "Doe", "john.doe@school.com",
                StaticRoleNames.Tenants.Teacher, passwordHasher);
            CreateUserIfNotExists("teacher2", "Jane", "Smith", "jane.smith@school.com",
                StaticRoleNames.Tenants.Teacher, passwordHasher);

            // ── Student users ──
            CreateUserIfNotExists("student1", "Alice", "Johnson", "alice.johnson@student.com",
                StaticRoleNames.Tenants.Student, passwordHasher);
            CreateUserIfNotExists("student2", "Bob", "Williams", "bob.williams@student.com",
                StaticRoleNames.Tenants.Student, passwordHasher);
            CreateUserIfNotExists("student3", "Charlie", "Brown", "charlie.brown@student.com",
                StaticRoleNames.Tenants.Student, passwordHasher);
        }

        private void CreateSampleFeedbacks()
        {
            // Only seed if no feedbacks exist yet
            if (_context.Feedbacks.IgnoreQueryFilters().Any()) return;

            var courses = _context.Courses.IgnoreQueryFilters()
                .Where(c => c.TenantId == 1).ToList();
            var students = _context.Users.IgnoreQueryFilters()
                .Where(u => u.TenantId == 1 &&
                    (u.UserName == "student1" || u.UserName == "student2" || u.UserName == "student3"))
                .ToList();

            if (!courses.Any() || !students.Any()) return;

            var angularCourse = courses.FirstOrDefault(c => c.CourseName.Contains("Angular"));
            var aspnetCourse = courses.FirstOrDefault(c => c.CourseName.Contains("ASP.NET"));
            var sqlCourse = courses.FirstOrDefault(c => c.CourseName.Contains("SQL"));

            var alice = students.FirstOrDefault(s => s.UserName == "student1");
            var bob = students.FirstOrDefault(s => s.UserName == "student2");
            var charlie = students.FirstOrDefault(s => s.UserName == "student3");

            var feedbacks = new[]
            {
                // Alice's feedbacks
                new Feedback
                {
                    TenantId = 1,
                    StudentName = $"{alice.Name} {alice.Surname}",
                    CourseId = angularCourse.Id,
                    Rating = 5,
                    Comment = "Excellent course! The hands-on exercises with components and services were very helpful. I feel confident building Angular apps now.",
                    CreatedDate = DateTime.UtcNow.AddDays(-10),
                    CreatorUserId = alice.Id
                },
                new Feedback
                {
                    TenantId = 1,
                    StudentName = $"{alice.Name} {alice.Surname}",
                    CourseId = aspnetCourse.Id,
                    Rating = 4,
                    Comment = "Great content on dependency injection and middleware pipeline. Would love more examples on authentication patterns.",
                    CreatedDate = DateTime.UtcNow.AddDays(-8),
                    CreatorUserId = alice.Id
                },
                // Bob's feedbacks
                new Feedback
                {
                    TenantId = 1,
                    StudentName = $"{bob.Name} {bob.Surname}",
                    CourseId = angularCourse.Id,
                    Rating = 4,
                    Comment = "Very well structured course. The RxJS section could use more real-world examples, but overall a solid learning experience.",
                    CreatedDate = DateTime.UtcNow.AddDays(-7),
                    CreatorUserId = bob.Id
                },
                new Feedback
                {
                    TenantId = 1,
                    StudentName = $"{bob.Name} {bob.Surname}",
                    CourseId = sqlCourse.Id,
                    Rating = 5,
                    Comment = "Bob Lee is an amazing instructor! Learned so much about query optimization and indexing strategies.",
                    CreatedDate = DateTime.UtcNow.AddDays(-5),
                    CreatorUserId = bob.Id
                },
                new Feedback
                {
                    TenantId = 1,
                    StudentName = $"{bob.Name} {bob.Surname}",
                    CourseId = aspnetCourse.Id,
                    Rating = 3,
                    Comment = "Decent course but the pace was a bit fast for beginners. The Entity Framework Core section was particularly useful though.",
                    CreatedDate = DateTime.UtcNow.AddDays(-3),
                    CreatorUserId = bob.Id
                },
                // Charlie's feedbacks
                new Feedback
                {
                    TenantId = 1,
                    StudentName = $"{charlie.Name} {charlie.Surname}",
                    CourseId = sqlCourse.Id,
                    Rating = 4,
                    Comment = "Good coverage of SQL fundamentals. The stored procedures section was very practical and applicable to real projects.",
                    CreatedDate = DateTime.UtcNow.AddDays(-6),
                    CreatorUserId = charlie.Id
                },
                new Feedback
                {
                    TenantId = 1,
                    StudentName = $"{charlie.Name} {charlie.Surname}",
                    CourseId = angularCourse.Id,
                    Rating = 3,
                    Comment = "Good introduction to Angular but I struggled with the routing module. More step-by-step walkthroughs would be helpful.",
                    CreatedDate = DateTime.UtcNow.AddDays(-4),
                    CreatorUserId = charlie.Id
                },
                new Feedback
                {
                    TenantId = 1,
                    StudentName = $"{charlie.Name} {charlie.Surname}",
                    CourseId = aspnetCourse.Id,
                    Rating = 5,
                    Comment = "One of the best .NET courses I have taken! The Web API section with Swagger integration was fantastic.",
                    CreatedDate = DateTime.UtcNow.AddDays(-1),
                    CreatorUserId = charlie.Id
                }
            };

            _context.Feedbacks.AddRange(feedbacks);
            _context.SaveChanges();
        }

        private void CreateUserIfNotExists(
            string userName, string firstName, string lastName,
            string email, string roleName, PasswordHasher<User> passwordHasher)
        {
            var existingUser = _context.Users.IgnoreQueryFilters()
                .FirstOrDefault(u => u.TenantId == 1 && u.UserName == userName);

            if (existingUser != null) return;

            var user = new User
            {
                TenantId = 1,
                UserName = userName,
                Name = firstName,
                Surname = lastName,
                EmailAddress = email,
                IsEmailConfirmed = true,
                IsActive = true
            };
            user.SetNormalizedNames();
            user.Password = passwordHasher.HashPassword(user, "123qwe");

            _context.Users.Add(user);
            _context.SaveChanges();

            // Find the role and assign it
            var role = _context.Roles.IgnoreQueryFilters()
                .FirstOrDefault(r => r.TenantId == 1 && r.Name == roleName);

            if (role != null)
            {
                _context.UserRoles.Add(new UserRole(1, user.Id, role.Id));
                _context.SaveChanges();
            }
        }
    }
}