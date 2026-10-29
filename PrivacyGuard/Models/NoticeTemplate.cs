using System;

namespace PrivacyGuard.Models
{
    // PDF Requirement: Implement Notice and Notice Template entities linking to consents[cite: 9]
    public class NoticeTemplate
    {
        public Guid TemplateId { get; set; }
        public Guid OrganizationId { get; set; }
        public string Version { get; set; } = "1.0"; // Supports dynamic versions[cite: 9]
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}