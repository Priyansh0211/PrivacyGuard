using System;

namespace PrivacyGuard.DTOs
{
    public class CreateRuleDto
    {
        public Guid OrganizationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? DocumentUrl { get; set; }
    }

    public class RuleResponseDto : CreateRuleDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}