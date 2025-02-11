using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
//using Appsec_webapp.Migrations;
using Appsec_webapp.Models;
using Appsec_webapp.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Appsec_webapp.Pages.Account
{
    [RequireHttps]
    [ValidateAntiForgeryToken]
    public class LoginModel : PageModel
    {
        private readonly AuthDbContext _context;
        
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(AuthDbContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public Login Input { get; set; }

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

            
            var normalizedEmail = _userManager.NormalizeEmail(Input.Email.Trim().ToLower());
            var identityUser = await _userManager.FindByEmailAsync(normalizedEmail);

            if (identityUser == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password 1.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(identityUser, Input.Password, false, true);
            //var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, false, true);
            var user = await _userManager.FindByEmailAsync(Input.Email);

            

            if (result.Succeeded)
            {
                // Prevent session fixation attack
                HttpContext.Session.Clear(); // Clear old session
                HttpContext.Session.SetString("SessionID", Guid.NewGuid().ToString());

                identityUser.SessionToken = HttpContext.Session.GetString("SessionID");
                await _userManager.UpdateAsync(identityUser);
                
                // Sign in the user
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, identityUser!.Email!),
                    new(ClaimTypes.Role, "User"),
                    new("Role", "User"),
                    new ("SessionID", HttpContext.Session.GetString("SessionID")!)
                };

                // V2
                var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuthenticationScheme");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Store user's email in the session (V2)
                await _signInManager.RefreshSignInAsync(identityUser);
                await HttpContext.SignInAsync("MyCookieAuthenticationScheme", claimsPrincipal);

                // Store user's data in the session
                /*HttpContext.Session.SetString("Email", identityUser!.Email!);
                HttpContext.Session.SetString("Username", identityUser!.UserName!);*/
                //HttpContext.Session.SetString("Userdata", identityUser!.ToString());
                HttpContext.Session.SetString("Userdata", JsonConvert.SerializeObject(identityUser));

                await LogAuditEvent(identityUser!.UserName!, identityUser!.Email!, "Login");
                //return RedirectToPage("/Account/Profile");
                //return RedirectToPage("/Index");
                return RedirectToPage("/Profile");
            }
            else if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Your account has been locked due to multiple failed login attempts. Please try again later..");
                return Page();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return Page();
            }
        }
        public void OnGet()
        {
        }
    }
}
