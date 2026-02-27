using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace CourseFeedbackMS.CourseFeedback.Dto
{
    public class UpdateCourseDto : EntityDto
    {
        [Required]
        [MaxLength(200)]
        public string CourseName { get; set; }

        [Required]
        [MaxLength(200)]
        public string InstructorName { get; set; }

        public bool IsActive { get; set; }
    }
}
