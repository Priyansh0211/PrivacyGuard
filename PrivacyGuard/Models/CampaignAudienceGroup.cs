using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class CampaignAudienceGroup
    {
        [Key]
        public Guid GroupId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrganizationId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CampaignAudienceGroupMember> Members { get; set; } = new List<CampaignAudienceGroupMember>();
    }
}
