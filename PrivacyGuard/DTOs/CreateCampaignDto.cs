using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.DTOs
{
    public class CreateCampaignDto
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Campaign Type is required (Awareness or Consent)")]
        public string CampaignType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Target Role is required (All, Admin, or NormalUser)")]
        public string TargetRole { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // Optional: Agar CampaignType "Consent" hai, toh Admin idhar DataInventoryId bhej sakta hai
        public Guid? DataInventoryId { get; set; }
    }
}
