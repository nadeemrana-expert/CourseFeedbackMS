using Abp.Application.Services.Dto;

namespace CourseFeedbackMS.CourseFeedback.Dto
{
    public class GetFeedbacksInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public int? CourseId { get; set; }
        public int? Rating { get; set; }
    }
}
