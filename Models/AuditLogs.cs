namespace Appsec_webapp.Models
{
    public class AuditLogs
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Action { get; set; } // e.g., "Login", "Logout"
        public DateTime Timestamp { get; set; }
    }
}
