using System.ComponentModel.DataAnnotations;

namespace CourseFeedbackMS.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}