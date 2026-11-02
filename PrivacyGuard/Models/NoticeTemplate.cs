using System;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class NoticeTemplate
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrganizationId { get; set; }

        [Required, MaxLength(100)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string PurposesJson { get; set; } = "[]";

        public string LogoUrl { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PrimaryColor { get; set; } = "#0F62FE";

        [MaxLength(50)]
        public string Layout { get; set; } = "Standard";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}