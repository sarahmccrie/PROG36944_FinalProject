using System.ComponentModel.DataAnnotations;

namespace FinalProject_ERMS.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Role")]
        public string Role { get; set; }
    }
}
