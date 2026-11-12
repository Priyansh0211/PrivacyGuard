using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.DTOs
{
    public class CampaignAudienceGroupMemberRequestDto
    {
        [Required]
        public Guid MemberId { get; set; }

        [Required]
        public string UserType { get; set; } = string.Empty;
    }

    public class CreateCampaignAudienceGroupDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, MinLength(1)]
        public List<CampaignAudienceGroupMemberRequestDto> Members { get; set; } = new();
    }

    public class UpdateCampaignAudienceGroupDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, MinLength(1)]
        public List<CampaignAudienceGroupMemberRequestDto> Members { get; set; } = new();
    }

    public class CampaignAudienceGroupMemberDto
    {
        public Guid MemberId { get; set; }
        public string UserType { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }

    public class CampaignAudienceGroupDto
    {
        public Guid GroupId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<CampaignAudienceGroupMemberDto> Members { get; set; } = new();
    }

    public class ApplyCampaignAudienceGroupDto
    {
        [Required]
        public Guid CampaignId { get; set; }
    }
}
