using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public class ConsentRecord
    {
        [Key]
        public Guid ConsentId { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required, MaxLength(100)]
        public string ConsentPurpose { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string NoticeVersion { get; set; } = string.Empty;

        public bool IsGranted { get; set; }
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
        public DateTime? WithdrawnAt { get; set; }

        [MaxLength(50)]
        public string? IPAddress { get; set; }

        public string? DeviceUserAgent { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}