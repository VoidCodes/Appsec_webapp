using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Appsec_webapp.Pages.Account
{
    //[Authorize(Policy = "RequireUserRole", AuthenticationSchemes = "MyCookieAuthenticationScheme")]
    public class ChangePasswordModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
