using System.Threading.Tasks;
using Abp.Application.Services;
using CourseFeedbackMS.Sessions.Dto;

namespace CourseFeedbackMS.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
