using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using CourseFeedbackMS.Authorization.Roles;
using CourseFeedbackMS.Authorization.Users;
using CourseFeedbackMS.CourseFeedback;
using CourseFeedbackMS.MultiTenancy;

namespace CourseFeedbackMS.EntityFrameworkCore
{
    public class CourseFeedbackMSDbContext : AbpZeroDbContext<Tenant, Role, User, CourseFeedbackMSDbContext>
    {
        /* Define a DbSet for each entity of the application */
        public DbSet<Course> Courses { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        public CourseFeedbackMSDbContext(DbContextOptions<CourseFeedbackMSDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Course>(b =>
            {
                b.ToTable("Courses");
                b.HasIndex(e => e.TenantId);
                b.Property(e => e.CourseName).IsRequired().HasMaxLength(200);
                b.Property(e => e.InstructorName).IsRequired().HasMaxLength(200);
            });

            modelBuilder.Entity<Feedback>(b =>
            {
                b.ToTable("Feedbacks");
                b.HasIndex(e => e.TenantId);
                b.HasIndex(e => e.CourseId);
                b.HasOne(e => e.Course)
                 .WithMany(c => c.Feedbacks)
                 .HasForeignKey(e => e.CourseId)
                 .OnDelete(DeleteBehavior.Restrict);
                b.Property(e => e.StudentName).IsRequired().HasMaxLength(200);
                b.Property(e => e.Comment).HasMaxLength(2000);
            });
        }
    }
}
