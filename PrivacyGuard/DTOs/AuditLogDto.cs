namespace PrivacyGuard.DTOs
{
    public class AuditLogDto
    {
        public long Seq { get; set; }
        public DateTime When { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public string Actor { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty;
        public string EntryHash { get; set; } = string.Empty;
    }
}
