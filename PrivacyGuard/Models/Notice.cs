// Models/Notice.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class Notice
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrganizationId { get; set; }

        public Guid? NoticeTemplateId { get; set; }
        public NoticeTemplate? NoticeTemplate { get; set; }

        [Required, MaxLength(100)]
        public string Code { get; set; } = string.Empty;

        public string Status { get; set; } = "Draft";
        public int Version { get; set; } = 1;
        public string? Sha256Hash { get; set; }

        public string PurposesJson { get; set; } = "[]";
        public string Layout { get; set; } = string.Empty;
        public string PrimaryColor { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PublishedAt { get; set; }

        public ICollection<NoticeTranslation> Translations { get; set; } = new List<NoticeTranslation>();
    }
}