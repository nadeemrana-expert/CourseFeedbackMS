using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using CourseFeedbackMS.EntityFrameworkCore;
using CourseFeedbackMS.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace CourseFeedbackMS.Web.Tests
{
    [DependsOn(
        typeof(CourseFeedbackMSWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class CourseFeedbackMSWebTestModule : AbpModule
    {
        public CourseFeedbackMSWebTestModule(CourseFeedbackMSEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(CourseFeedbackMSWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(CourseFeedbackMSWebMvcModule).Assembly);
        }
    }
}