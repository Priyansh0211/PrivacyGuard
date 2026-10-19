using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public class CampaignResponse
    {
        [Key]
        public Guid ResponseId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CampaignId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        // Isme aayega: "Pending", "Completed" (Awareness ke liye), "Accepted", "Rejected" (Consent ke liye)
        [Required]
        [MaxLength(30)]
        public string ResponseStatus { get; set; } = "Pending";

        public DateTime? RespondedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- Navigation Properties ---
        [ForeignKey("CampaignId")]
        public Campaign? Campaign { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
