using Microsoft.Extensions.Configuration;
using Castle.MicroKernel.Registration;
using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using CourseFeedbackMS.Configuration;
using CourseFeedbackMS.EntityFrameworkCore;
using CourseFeedbackMS.Migrator.DependencyInjection;

namespace CourseFeedbackMS.Migrator
{
    [DependsOn(typeof(CourseFeedbackMSEntityFrameworkModule))]
    public class CourseFeedbackMSMigratorModule : AbpModule
    {
        private readonly IConfigurationRoot _appConfiguration;

        public CourseFeedbackMSMigratorModule(CourseFeedbackMSEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbSeed = true;

            _appConfiguration = AppConfigurations.Get(
                typeof(CourseFeedbackMSMigratorModule).GetAssembly().GetDirectoryPathOrNull()
            );
        }

        public override void PreInitialize()
        {
            Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
                CourseFeedbackMSConsts.ConnectionStringName
            );

            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
            Configuration.ReplaceService(
                typeof(IEventBus), 
                () => IocManager.IocContainer.Register(
                    Component.For<IEventBus>().Instance(NullEventBus.Instance)
                )
            );
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(CourseFeedbackMSMigratorModule).GetAssembly());
            ServiceCollectionRegistrar.Register(IocManager);
        }
    }
}
