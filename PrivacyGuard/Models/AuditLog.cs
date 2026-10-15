using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class AuditLog
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? UserId { get; set; } // Kisne action liya
        public Guid? OrganizationId { get; set; } // Kis company ka data hai

        public string TableName { get; set; } = string.Empty; // Konsi table modify hui
        public string ActionType { get; set; } = string.Empty; // Create, Update, Delete
        public string PrimaryKey { get; set; } = string.Empty; // Jis row me change hua uski ID

        public string? OldValues { get; set; } // JSON format (Purana data)
        public string? NewValues { get; set; } // JSON format (Naya data)

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
