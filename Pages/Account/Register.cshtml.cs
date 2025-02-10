using Appsec_webapp.ViewModels;
using System.Text.Encodings.Web;
using System.Security.Cryptography;
using Appsec_webapp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using Appsec_webapp.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Appsec_webapp.Pages.Account
{
    [RequireHttps]
    [ValidateAntiForgeryToken]
    public class RegisterModel : PageModel
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _config;
        // Sanitize input
        private readonly HtmlEncoder _sanitizer;
        // Data Protection API
        private readonly IDataProtectionProvider _dataProtectionProvider;
        // Application user manager
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        static string HashPassword(string password, byte[] salt)
        {
            using (var sha256 = new SHA256Managed())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];

                // Concatenate password and salt
                Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
                Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

                // Hash the concatenated password and salt
                byte[] hashedBytes = sha256.ComputeHash(saltedPassword);

                // Concatenate the salt and hashed password for storage
                byte[] hashedPasswordWithSalt = new byte[hashedBytes.Length + salt.Length];
                Buffer.BlockCopy(salt, 0, hashedPasswordWithSalt, 0, salt.Length);
                Buffer.BlockCopy(hashedBytes, 0, hashedPasswordWithSalt, salt.Length, hashedBytes.Length);

                return Convert.ToBase64String(hashedPasswordWithSalt);
            }
        }

        static byte[] GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[16]; // Adjust the size based on your security requirements
                rng.GetBytes(salt);
                return salt;
            }
        }

        public RegisterModel(AuthDbContext context, HtmlEncoder sanitizer,
            IDataProtectionProvider dataProtectionProvider, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration config)
        {
            _context = context;
            _sanitizer = sanitizer;
            _dataProtectionProvider = dataProtectionProvider;
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        [BindProperty]
        public Register Input { get; set; }

        private async Task LogAuditEvent(string userName, string email, string action)
        {
            var auditLog = new AuditLogs
            {
                UserName = userName,
                Email = email,
                Action = action,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Sanitize input
            var sanitizedFname = _sanitizer.Encode(Input.FirstName.Trim());
            var sanitizedLname = _sanitizer.Encode(Input.LastName.Trim());
            var sanitizedGender = _sanitizer.Encode(Input.Gender.Trim());
            var sanitizedEmail = _sanitizer.Encode(Input.Email.Trim());
            var sanitizedWhoAmI = _sanitizer.Encode(Input.WhoAmI.Trim());
            var sanitizedDob = _sanitizer.Encode(Input.DateOfBirth.Trim());

            // Generate salt and hash the password
            byte[] salt = GenerateSalt();
            string hashedPassword = HashPassword(Input.Password, salt);

            // Convert salt to string and back to byte array
            string saltString = Convert.ToBase64String(salt);
            byte[] saltBytes = Convert.FromBase64String(saltString);

            // Protect NRIC
            var protector = _dataProtectionProvider.CreateProtector("NRICProtection");
            string protectedNRIC = protector.Protect(Input.NRIC);

            // Check if the email already exists in the database
            var existingUser = await _userManager.FindByEmailAsync(sanitizedEmail);
            if (existingUser != null)
            {
                ModelState.AddModelError("Input.Email", "Email already exists.");
                return Page();
            }
            /*var identityUser = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == sanitizedEmail.ToUpper());
            if (identityUser != null)
            {
                ModelState.AddModelError("Input.Email", "Email already exists.");
                return Page();
            }*/
            /*if (_context.Users.Any(u => u.Email == sanitizedEmail))
            {
                ModelState.AddModelError("Input.Email", "Email already exists.");
                return Page();
            }*/

            string recaptchaResponse = Request.Form["g-recaptcha-response"].ToString();
            
            // Verify the reCAPTCHA
            string secretKey = _config["reCAPTCHA:SecretKey"]!;
            string verificationUrl = _config["reCAPTCHA:ValidationUrl"]!;
            bool isCaptchaValid = await Recaptcha.verifyCaptcha(recaptchaResponse, secretKey, verificationUrl);

            if (!isCaptchaValid)
            {
                ModelState.AddModelError("Input.Captcha", "Please complete the reCAPTCHA.");
                return Page();
            }


            // Save the file to the wwwroot folder
            var fileName = Path.GetFileName(Input.Resume.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await Input.Resume.CopyToAsync(fileStream);
            }

            // Identity User registration
            var user = new ApplicationUser()
            { 
                FirstName = sanitizedFname,
                LastName = sanitizedLname,
                Gender = sanitizedGender,
                NRIC = protectedNRIC,
                UserName = sanitizedWhoAmI,
                Email = sanitizedEmail,
                HashPassword = hashedPassword,
                DateOfBirth = sanitizedDob,
                //Resume = Input.Resume,
                WhoAmI = sanitizedWhoAmI,
                Salt = saltBytes
            };

            if (Input.Resume != null)
            {
                user.ResumeFilePath = filePath;
            }

            // Create the user
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // Prevent session fixation attack
                HttpContext.Session.Clear(); // Clear old session
                HttpContext.Session.SetString("SessionID", Guid.NewGuid().ToString());

                // Sign in the user
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, user.Email),
                    new(ClaimTypes.Role, "User"),
                    new("Role", "User"),
                    new ("SessionID", HttpContext.Session.GetString("SessionID")!)
                };

                // V2
                var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuthenticationScheme");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                
                // Sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);
                await HttpContext.SignInAsync("MyCookieAuthenticationScheme", claimsPrincipal);

                // Log the audit event
                await LogAuditEvent(user.UserName, user.Email, "New user registered");

                // Store the user's email in a session
                //HttpContext.Session.SetString("Email", sanitizedEmail);
                HttpContext.Session.SetString("Userdata", JsonConvert.SerializeObject(user));
                return RedirectToPage("/Index");
            }

            // Save the user to the database
            return RedirectToPage("/Index");
        }
    }
}
