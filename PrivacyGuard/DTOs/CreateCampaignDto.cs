using PrivacyGuard.Models;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.DTOs
{
    public class CreateCampaignDto
    {
        [Required] public string Title { get; set; }
        public string Description { get; set; }
        [Required] public string CampaignType { get; set; }
        [Required] public AudienceType TargetAudience { get; set; } // Fixed CS1061 Error
        [Required] public string TargetRole { get; set; }
        [Required] public DateTime StartDate { get; set; }
        [Required] public DateTime EndDate { get; set; }
        public Guid? DataInventoryId { get; set; }
    }
}
