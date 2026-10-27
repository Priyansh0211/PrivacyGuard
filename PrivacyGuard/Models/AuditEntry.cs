using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace PrivacyGuard.Models
{
    public class AuditEntry
    {
        public EntityEntry Entry { get; }
        public Guid? UserId { get; set; }
        public Guid? OrganizationId { get; set; }
        public string? ActorEmail { get; set; }
        public string? ActorRole { get; set; }
        public string? IpAddress { get; set; }
        public string TableName { get; set; } = string.Empty;
        public Dictionary<string, object?> KeyValues { get; } = new();
        public Dictionary<string, object?> OldValues { get; } = new();
        public Dictionary<string, object?> NewValues { get; } = new();
        public List<PropertyEntry> TemporaryProperties { get; } = new();
        public string ActionType { get; set; } = string.Empty;

        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public AuditLog ToAudit()
        {
            // Action format like UI: "user.added", "consent.modified"
            var formattedAction = $"{TableName.ToLower()}.{ActionType.ToLower()}";

            return new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                OrganizationId = OrganizationId,
                ActorEmail = ActorEmail ?? "System",
                ActorRole = ActorRole ?? "System",
                IpAddress = IpAddress,
                TableName = TableName,
                ActionType = formattedAction,
                PrimaryKey = JsonSerializer.Serialize(KeyValues),
                OldValues = OldValues.Count == 0 ? null : JsonSerializer.Serialize(OldValues),
                NewValues = NewValues.Count == 0 ? null : JsonSerializer.Serialize(NewValues),
                Outcome = "Success",
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
