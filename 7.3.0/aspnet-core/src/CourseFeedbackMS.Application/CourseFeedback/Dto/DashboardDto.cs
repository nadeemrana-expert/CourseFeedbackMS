using System;
using System.Collections.Generic;

namespace CourseFeedbackMS.CourseFeedback.Dto
{
    public class DashboardDto
    {
        public int TotalFeedbackCount { get; set; }
        public int TotalCourseCount { get; set; }
        public double? AverageRating { get; set; }
        public string UserRole { get; set; }
        public List<TopCourseDto> TopCoursesByRating { get; set; }
        public List<RecentFeedbackDto> RecentFeedbacks { get; set; }
    }

    public class TopCourseDto
    {
        public string CourseName { get; set; }
        public double AverageRating { get; set; }
        public int FeedbackCount { get; set; }
    }

    public class RecentFeedbackDto
    {
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
