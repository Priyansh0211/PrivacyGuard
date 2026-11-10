using System;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.DTOs
{
    // Frontend ko policy list ya detail dikhane ke liye DTO
    public class RetentionPolicyDto
    {
        public Guid PolicyId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int RetentionPeriodInDays { get; set; }
        public string TriggerEvent { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Nayi policy create karne ke liye frontend se aane wala data
    public class CreateRetentionPolicyDto
    {
        [Required(ErrorMessage = "Policy Name is required.")]
        public string PolicyName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(1, 3650, ErrorMessage = "Retention period must be between 1 and 3650 days (10 years).")]
        public int RetentionPeriodInDays { get; set; }

        [Required(ErrorMessage = "Trigger Event is required.")]
        public string TriggerEvent { get; set; } = string.Empty;

        public string ActionType { get; set; } = "Anonymize";
    }

    // Existing policy update karne ke liye DTO
    public class UpdateRetentionPolicyDto
    {
        public string PolicyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int RetentionPeriodInDays { get; set; }
        public bool IsActive { get; set; }
    }
}