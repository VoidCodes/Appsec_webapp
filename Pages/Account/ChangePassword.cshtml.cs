using Appsec_webapp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Appsec_webapp.Pages.Account
{
    [Authorize(Policy = "RequireUserRole", AuthenticationSchemes = "MyCookieAuthenticationScheme")]
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ChangePasswordModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public ChangePasswordInputModel Input { get; set; } = new ChangePasswordInputModel();

        public class ChangePasswordInputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current Password")]
            public string CurrentPassword { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            [StringLength(100, ErrorMessage = "The password must be at least 12 characters long with at least one uppercase letter, one lowercase letter, one digit, and one special character.", MinimumLength = 12)]
            public string NewPassword { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm New Password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmNewPassword { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //var user = await _userManager.GetUserAsync(User);
            var userData = HttpContext.Session.GetString("Userdata");
            //var user = JsonConvert.DeserializeObject<ApplicationUser>(userData!);

            if (string.IsNullOrEmpty(userData))
            {
                ModelState.AddModelError(string.Empty, "Session expired. Please log in again.");
                return RedirectToPage("/Account/Login");
            }

            var sessionUser = JsonConvert.DeserializeObject<ApplicationUser>(userData);
            if (sessionUser == null || string.IsNullOrEmpty(sessionUser.Id.ToString()))
            {
                ModelState.AddModelError(string.Empty, "User not found 1.");
                return RedirectToPage("/Account/Login");
            }

            var user = await _userManager.FindByIdAsync(sessionUser!.Id.ToString());
            //var user = await _userManager.FindByEmailAsync(sessionUser!.Email!);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found 2.");
                return RedirectToPage("/Account/Login");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user!, Input.CurrentPassword, Input.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            // Re-sign in the user to refresh authentication cookies
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Your password has been changed successfully!";
            return RedirectToPage("/Account/ChangePassword");
        }
    }
}
