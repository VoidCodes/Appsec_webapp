using Appsec_webapp.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Appsec_webapp.ViewModels
{
    public class Register
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        [RegularExpression(@"^[STFG]\d{7}[A-Z]$", ErrorMessage = "Invalid NRIC format. Please enter a valid NRIC.")]
        public string NRIC { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, DataType(DataType.Password)]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,}$", ErrorMessage = "Password must be at least 12 characters and contain at least one uppercase letter, one lowercase letter, one digit and one special character.")]
        public string Password { get; set; }
        [Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public string DateOfBirth { get; set; }
        // Field for file upload
        [Required]
        [AllowedExtensions(new string[] { ".pdf", ".doc", ".docx" })]
        public IFormFile Resume { get; set; }
        [Required]
        public string WhoAmI { get; set; }
    }
}
