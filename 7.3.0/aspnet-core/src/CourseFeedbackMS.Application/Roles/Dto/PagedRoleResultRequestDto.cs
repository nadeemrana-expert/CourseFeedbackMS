using Abp.Application.Services.Dto;

namespace CourseFeedbackMS.Roles.Dto
{
    public class PagedRoleResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}

