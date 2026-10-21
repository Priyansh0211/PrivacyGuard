using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public class Campaign
    {
        [Key]
        public Guid CampaignId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrganizationId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        // Isme aayega: "Awareness" (Normal policy) ya "Consent" (Product data permission)
        [Required]
        [MaxLength(30)]
        public string CampaignType { get; set; } = "Awareness";

        // Kisko dikhana hai: "All", "Admin", ya "NormalUser"
        [Required]
        [MaxLength(50)]
        public string TargetRole { get; set; } = "All";

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // Campaign ka current status: "Draft", "Active", "Completed"
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Draft";

        // Tumhare dost ke DataInventory table se link (Sirf Consent campaigns ke liye)
        public Guid? DataInventoryId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // --- Navigation Properties ---
        public Organization? Organization { get; set; }

        [ForeignKey("DataInventoryId")]
        public DataInventory? DataInventory { get; set; }
    }
}
