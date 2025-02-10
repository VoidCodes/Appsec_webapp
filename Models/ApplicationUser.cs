using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Appsec_webapp.Extensions;
using Microsoft.AspNetCore.Identity;

namespace Appsec_webapp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        [RegularExpression(@"^[STFG]\d{7}[A-Z]$", ErrorMessage = "Invalid NRIC format. Please enter a valid NRIC.")]
        public string NRIC { get; set; }
        /*[Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }*/
        [Required]
        [DataType(DataType.Password)]
        public string HashPassword { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public string DateOfBirth { get; set; }
        // Field for file upload
        [Required]
        [NotMapped]
        [AllowedExtensions(new string[] { ".pdf", ".doc", ".docx" })]
        public IFormFile Resume { get; set; }
        public string ResumeFilePath { get; set; }
        [Required]
        public string WhoAmI { get; set; }
        public byte[] Salt { get; set; }
        public string? SessionToken { get; set; }
    }
}
