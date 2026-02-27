namespace CourseFeedbackMS.Authorization.Roles
{
    public static class StaticRoleNames
    {
        public static class Host
        {
            public const string Admin = "Admin";
        }

        public static class Tenants
        {
            public const string Admin = "Admin";
            public const string Teacher = "Teacher";
            public const string Student = "Student";
        }
    }
}
