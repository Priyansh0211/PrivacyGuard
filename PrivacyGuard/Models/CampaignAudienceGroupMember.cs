using System;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class CampaignAudienceGroupMember
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid GroupId { get; set; }

        [Required]
        public Guid MemberId { get; set; }

        [Required, MaxLength(20)]
        public string UserType { get; set; } = string.Empty;

        public CampaignAudienceGroup? Group { get; set; }
    }
}
