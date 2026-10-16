namespace PrivacyGuard.DTOs
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string? IpAddress { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string PrimaryKey { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
