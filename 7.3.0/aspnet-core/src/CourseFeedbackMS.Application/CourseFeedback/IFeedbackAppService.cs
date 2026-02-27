using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CourseFeedbackMS.CourseFeedback.Dto;
using System.Threading.Tasks;

namespace CourseFeedbackMS.CourseFeedback
{
    public interface IFeedbackAppService : IApplicationService
    {
        Task<PagedResultDto<FeedbackDto>> GetAllAsync(GetFeedbacksInput input);
        Task<FeedbackDto> GetAsync(EntityDto input);
        Task<FeedbackDto> CreateAsync(CreateFeedbackDto input);
        Task<FeedbackDto> UpdateAsync(UpdateFeedbackDto input);
        Task DeleteAsync(EntityDto input);
        Task<DashboardDto> GetDashboardDataAsync();
    }
}
