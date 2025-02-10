using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Appsec_webapp.Models
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IConfiguration _config;

        public AuthDbContext(IConfiguration config, DbContextOptions<AuthDbContext> options) : base(options)
        {
            _config = config;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = _config.GetConnectionString("AuthConnStr");
            optionsBuilder.UseSqlServer(connectionString);
        }

        // Add a DbSet for each entity type that you want to include in your model
        public DbSet<AuditLogs> AuditLogs { get; set; }
    }
}
