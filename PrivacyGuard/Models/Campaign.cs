using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public enum AudienceType
    {
        Specific = 1,
        AllDataPrincipals = 2,
        AllInternalUsers = 3,
        Everyone = 4
    }

    public class Campaign
    {
        [Key]
        public Guid CampaignId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrganizationId { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required, MaxLength(30)]
        public string CampaignType { get; set; } = "Awareness";

        public AudienceType TargetAudience { get; set; } = AudienceType.Specific;

        [Required, MaxLength(50)]
        public string TargetRole { get; set; } = "All";

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required, MaxLength(30)]
        public string Status { get; set; } = "Draft";

        public Guid? DataInventoryId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Organization? Organization { get; set; }
        [ForeignKey("DataInventoryId")]
        public DataInventory? DataInventory { get; set; }
    }
}