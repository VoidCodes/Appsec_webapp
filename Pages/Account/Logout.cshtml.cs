using Appsec_webapp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Appsec_webapp.Pages.Account
{
    [Authorize(Policy = "RequireUserRole", AuthenticationSchemes = "MyCookieAuthenticationScheme")]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _context;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, AuthDbContext context, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _context = context;
            _userManager = userManager;
        }

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

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            var userData = HttpContext.Session.GetString("Userdata");
            var user = JsonConvert.DeserializeObject<ApplicationUser>(userData!);
            user.SessionToken = null;
            await _userManager.UpdateAsync(user);

            await LogAuditEvent(user!.UserName!, user!.Email!, "Logout");
            
            await _signInManager.SignOutAsync();
            // Reset cookie
            Response.Cookies.Delete("MyCookieAuthenticationScheme");
            // Reset session
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostDontLogoutAsync()
        {
            return RedirectToPage("/Profile");
        }
        public void OnGet()
        {
        }
    }
}
