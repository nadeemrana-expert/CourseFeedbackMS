using Abp.Application.Services.Dto;
using System;

namespace CourseFeedbackMS.CourseFeedback.Dto
{
    public class FeedbackDto : EntityDto
    {
        public string StudentName { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedDate { get; set; }
        public string AttachmentPath { get; set; }
        public string AttachmentFileName { get; set; }
    }
}
