using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace CourseFeedbackMS.Localization
{
    public static class CourseFeedbackMSLocalizationConfigurer
    {
        public static void Configure(ILocalizationConfiguration localizationConfiguration)
        {
            localizationConfiguration.Sources.Add(
                new DictionaryBasedLocalizationSource(CourseFeedbackMSConsts.LocalizationSourceName,
                    new XmlEmbeddedFileLocalizationDictionaryProvider(
                        typeof(CourseFeedbackMSLocalizationConfigurer).GetAssembly(),
                        "CourseFeedbackMS.Localization.SourceFiles"
                    )
                )
            );
        }
    }
}
