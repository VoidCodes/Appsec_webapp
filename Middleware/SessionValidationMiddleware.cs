using Appsec_webapp.Models;
using Microsoft.AspNetCore.Identity;

namespace Appsec_webapp.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SessionValidationMiddleware> _logger;

        public SessionValidationMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory = scopeFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.User.Identity!.IsAuthenticated)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();

                    var user = await userManager.GetUserAsync(context.User);
                    var sessionToken = context.Session.GetString("SessionID");

                    _logger.LogInformation("SessionValidationMiddleware is executing...");
                    _logger.LogInformation($"Session Token in Session: {sessionToken}");
                    _logger.LogInformation($"Session Token in DB: {user.SessionToken}");

                    if (user != null && sessionToken != user.SessionToken)
                    {
                        // Session mismatch, force logout
                        await signInManager.SignOutAsync();
                        context.Session.Clear();
                        context.Response.Redirect("/Account/Login?sessionExpired=true");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
