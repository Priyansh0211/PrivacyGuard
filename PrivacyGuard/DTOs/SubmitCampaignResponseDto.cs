using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.DTOs
{
    public class SubmitCampaignResponseDto
    {
        [Required]
        public Guid CampaignId { get; set; }

        // Isme frontend bhejega: "Accepted", "Rejected", ya "Completed"
        [Required(ErrorMessage = "Response Status is required")]
        public string ResponseStatus { get; set; } = string.Empty;
    }
}
