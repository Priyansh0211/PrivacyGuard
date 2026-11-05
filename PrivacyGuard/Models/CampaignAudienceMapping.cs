using System;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class CampaignAudienceMapping
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CampaignId { get; set; }
        public Guid? DataPrincipalId { get; set; }
        public Guid? UserId { get; set; }

        public DateTime AssignedOn { get; set; } = DateTime.UtcNow;
    }
}