using System.Threading.Tasks;
using Abp.Application.Services;
using CourseFeedbackMS.Authorization.Accounts.Dto;

namespace CourseFeedbackMS.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
