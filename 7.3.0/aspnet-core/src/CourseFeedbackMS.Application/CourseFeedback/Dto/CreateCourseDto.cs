using System.ComponentModel.DataAnnotations;

namespace CourseFeedbackMS.CourseFeedback.Dto
{
    public class CreateCourseDto
    {
        [Required]
        [MaxLength(200)]
        public string CourseName { get; set; }

        [Required]
        [MaxLength(200)]
        public string InstructorName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
