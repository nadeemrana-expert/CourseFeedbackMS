using CourseFeedbackMS.Debugging;

namespace CourseFeedbackMS
{
    public class CourseFeedbackMSConsts
    {
        public const string LocalizationSourceName = "CourseFeedbackMS";

        public const string ConnectionStringName = "Default";

        public const bool MultiTenancyEnabled = true;


        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public static readonly string DefaultPassPhrase =
            DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "d5ae348fbee3437a9a8d22e7a84c8159";
    }
}
