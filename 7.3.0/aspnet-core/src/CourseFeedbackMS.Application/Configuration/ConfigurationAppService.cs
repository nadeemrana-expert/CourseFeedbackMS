using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using CourseFeedbackMS.Configuration.Dto;

namespace CourseFeedbackMS.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : CourseFeedbackMSAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
