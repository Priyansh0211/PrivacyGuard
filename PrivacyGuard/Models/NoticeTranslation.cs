using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public class NoticeTranslation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid NoticeId { get; set; }
        [ForeignKey("NoticeId")]
        public Notice Notice { get; set; }

        [Required, MaxLength(50)]
        public string LanguageCode { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;
        public bool IsTranslated { get; set; } = true;
    }
}