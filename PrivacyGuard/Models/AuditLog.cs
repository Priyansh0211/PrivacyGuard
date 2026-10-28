using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public long SequenceNumber { get; set; } // UI ka 'SEQ'
        public Guid? OrganizationId { get; set; }
        public Guid? UserId { get; set; }
        public string? ActorEmail { get; set; } // UI ka 'ACTOR' email
        public string? ActorRole { get; set; }  // UI ka 'ACTOR' role
        public string? IpAddress { get; set; }
        public string TableName { get; set; } = string.Empty; // UI ka 'ENTITY'
        public string ActionType { get; set; } = string.Empty; // UI ka 'ACTION'
        public string PrimaryKey { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string Outcome { get; set; } = "Success"; // UI ka 'OUTCOME'
        public DateTime Timestamp { get; set; } // UI ka 'WHEN'

        // Hash Chaining Columns
        public string? PreviousHash { get; set; }
        public string EntryHash { get; set; } = string.Empty; // UI ka 'ENTRY HASH'
    }
}
