using System.ComponentModel.DataAnnotations;

namespace Carly.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}