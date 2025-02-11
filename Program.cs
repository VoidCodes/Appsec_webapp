using Appsec_webapp.Middleware;
using Appsec_webapp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AuthDbContext>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AuthDbContext>();
builder.Services.AddHttpContextAccessor();

// Authorization
builder.Services.AddAuthorization(options =>
{
    //options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireClaim("Role", "User"));
});

// Account Lockout
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;
});

// Session
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDistributedMemoryCache(); //save session in memory
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true; // session cookie is not accessible through client side script
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Require HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // Prevent CSRF attacks
    options.IdleTimeout = TimeSpan.FromMinutes(5); // session expires after 30 minutes of inactivity
    options.Cookie.IsEssential = true;
});

// HTTPS Redirection
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 443;
});

// HSTS
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(60);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

// Cookie Authentication
/*builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });*/

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/ErrorPages/AccessDenied";
    options.Cookie.Name = "MyCookieAuthenticationScheme";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5); /*TimeSpan.FromSeconds(10);*/
    options.SlidingExpiration = true;
});

// Cookie Authentication
builder.Services.AddAuthentication("MyCookieAuthenticationScheme")
    .AddCookie("MyCookieAuthenticationScheme", options =>
    {
        options.Cookie.Name = "MyCookieAuthenticationScheme";

        // Cookie settings
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";

        // Other options
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = /*TimeSpan.FromMinutes(5)*/ TimeSpan.FromSeconds(10);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = context =>
        {
            //context.Response.Redirect("/Account/Login");
            context.Response.Redirect("/Account/Login?sessionExpired=true");
            return Task.CompletedTask;
        };
    });

// Data Protection
builder.Services.AddDataProtection()
    .SetApplicationName("Appsec_webapp");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    
    // Error Page redirection
    //app.UseStatusCodePagesWithReExecute("/ErrorPages" , "?statusCode={0}");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Error Page redirection
app.UseStatusCodePagesWithReExecute("/ErrorPages/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseMiddleware<SessionValidationMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
