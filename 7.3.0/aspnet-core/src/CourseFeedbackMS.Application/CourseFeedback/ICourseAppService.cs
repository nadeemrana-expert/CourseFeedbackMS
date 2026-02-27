using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CourseFeedbackMS.CourseFeedback.Dto;
using System.Threading.Tasks;

namespace CourseFeedbackMS.CourseFeedback
{
    public interface ICourseAppService : IApplicationService
    {
        Task<PagedResultDto<CourseDto>> GetAllAsync(GetCoursesInput input);
        Task<CourseDto> GetAsync(EntityDto input);
        Task<CourseDto> CreateAsync(CreateCourseDto input);
        Task<CourseDto> UpdateAsync(UpdateCourseDto input);
        Task DeleteAsync(EntityDto input);
        Task<ListResultDto<CourseDto>> GetActiveCoursesAsync();
    }
}
