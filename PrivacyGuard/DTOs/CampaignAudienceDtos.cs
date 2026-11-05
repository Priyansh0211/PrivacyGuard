using System;
using System.Collections.Generic;

namespace PrivacyGuard.DTOs
{
    public class AudienceMemberDto
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string Email { get; set; }
        public string UserType { get; set; }
    }


    public class LinkAudienceRequestDto
    {
        public Guid CampaignId { get; set; }
        public List<Guid> SelectedIds { get; set; } = new List<Guid>();
        public bool IsInternalUserMapping { get; set; } // true = Staff, false = Customer
    }
}