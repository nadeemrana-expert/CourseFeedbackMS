using System.Threading.Tasks;
using CourseFeedbackMS.Configuration.Dto;

namespace CourseFeedbackMS.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
