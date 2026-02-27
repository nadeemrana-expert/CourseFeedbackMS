using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace CourseFeedbackMS.EntityFrameworkCore
{
    public static class CourseFeedbackMSDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<CourseFeedbackMSDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<CourseFeedbackMSDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);
        }
    }
}
