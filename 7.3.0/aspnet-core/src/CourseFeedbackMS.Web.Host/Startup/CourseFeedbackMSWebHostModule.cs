using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using CourseFeedbackMS.Configuration;

namespace CourseFeedbackMS.Web.Host.Startup
{
    [DependsOn(
       typeof(CourseFeedbackMSWebCoreModule))]
    public class CourseFeedbackMSWebHostModule: AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public CourseFeedbackMSWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(CourseFeedbackMSWebHostModule).GetAssembly());
        }
    }
}
