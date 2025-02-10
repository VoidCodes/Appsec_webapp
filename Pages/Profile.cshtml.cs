using Appsec_webapp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Appsec_webapp.Extensions;

namespace Appsec_webapp.Pages
{
    [Authorize(Policy = "RequireUserRole", AuthenticationSchemes = "MyCookieAuthenticationScheme")]
    public class ProfileModel : PageModel
    {
        private readonly AuthDbContext _context;
        // Data Protection API
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public ProfileModel(AuthDbContext context, IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _dataProtectionProvider = dataProtectionProvider;
        }

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
