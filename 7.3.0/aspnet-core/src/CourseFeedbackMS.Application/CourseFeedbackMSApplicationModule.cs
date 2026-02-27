using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using CourseFeedbackMS.Authorization;
using CourseFeedbackMS.CourseFeedback;
using CourseFeedbackMS.CourseFeedback.Dto;
using System.Linq;

namespace CourseFeedbackMS
{
    [DependsOn(
        typeof(CourseFeedbackMSCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class CourseFeedbackMSApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<CourseFeedbackMSAuthorizationProvider>();

            Configuration.Modules.AbpAutoMapper().Configurators.Add(config =>
            {
                config.CreateMap<Course, CourseDto>()
                      .ForMember(dest => dest.FeedbackCount,
                                 opt => opt.MapFrom(src => src.Feedbacks != null ? src.Feedbacks.Count : 0))
                      .ForMember(dest => dest.AverageRating,
                                 opt => opt.MapFrom(src => src.Feedbacks != null && src.Feedbacks.Any()
                                            ? src.Feedbacks.Average(f => f.Rating)
                                            : (double?)null));

                config.CreateMap<CreateCourseDto, Course>();
                config.CreateMap<UpdateCourseDto, Course>();

                config.CreateMap<Feedback, FeedbackDto>()
                      .ForMember(dest => dest.CourseName,
                                 opt => opt.MapFrom(src => src.Course != null ? src.Course.CourseName : ""));

                config.CreateMap<CreateFeedbackDto, Feedback>();
                config.CreateMap<UpdateFeedbackDto, Feedback>();
            });
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(CourseFeedbackMSApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
