using Abp.Application.Services.Dto;

namespace CourseFeedbackMS.CourseFeedback.Dto
{
    public class GetCoursesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public bool? IsActive { get; set; }
    }
}
