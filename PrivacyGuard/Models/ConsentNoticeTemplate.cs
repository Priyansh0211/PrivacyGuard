using System;

namespace PrivacyGuard.Models
{
    // PDF Req: Notice templates with dynamic versions[cite: 9]
    public class ConsentNoticeTemplate
    {
        public Guid ConsentNoticeTemplateId { get; set; }
        public Guid OrganizationId { get; set; }
        public string Version { get; set; } = "1.0";
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}