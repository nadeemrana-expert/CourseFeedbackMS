using Abp.Application.Services;
using CourseFeedbackMS.MultiTenancy.Dto;

namespace CourseFeedbackMS.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

