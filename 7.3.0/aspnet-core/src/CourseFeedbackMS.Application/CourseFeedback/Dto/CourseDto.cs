using Abp.Application.Services.Dto;
using System;

namespace CourseFeedbackMS.CourseFeedback.Dto
{
    public class CourseDto : EntityDto
    {
        public string CourseName { get; set; }
        public string InstructorName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreationTime { get; set; }
        public int FeedbackCount { get; set; }
        public double? AverageRating { get; set; }
    }
}
