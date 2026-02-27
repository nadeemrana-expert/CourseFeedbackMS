using System.ComponentModel.DataAnnotations;

namespace CourseFeedbackMS.CourseFeedback.Dto
{
    public class CreateFeedbackDto
    {
        [MaxLength(200)]
        public string StudentName { get; set; }

        [Required]
        public int CourseId { get; set; }

        [MaxLength(2000)]
        public string Comment { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string AttachmentPath { get; set; }
        public string AttachmentFileName { get; set; }
    }
}
