using System;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.DTOs
{
    public class RetentionMetricsDto
    {
        public int ActivePolicies { get; set; }
        public int Overdue { get; set; }
        public int JobsInQueue { get; set; }
    }

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

    public class CreateRetentionPolicyDto
    {
        [Required(ErrorMessage = "Policy Name is required.")]
        public string PolicyName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(1, 3650, ErrorMessage = "Retention period must be between 1 and 3650 days (10 years).")]
        public int RetentionPeriodInDays { get; set; }

        [Required(ErrorMessage = "Trigger Event is required.")]
        [RegularExpression("^(DataCreatedDate|LastActivity)$", ErrorMessage = "Unsupported trigger event. Allowed values: DataCreatedDate, LastActivity.")]
        public string TriggerEvent { get; set; } = "DataCreatedDate";

        [Required(ErrorMessage = "Action Type is required.")]
        [RegularExpression("^(Anonymize|Delete)$", ErrorMessage = "Unsupported action type. Allowed values: Anonymize, Delete.")]
        public string ActionType { get; set; } = "Anonymize";
    }

    public class UpdateRetentionPolicyDto
    {
        [Required(ErrorMessage = "Policy Name is required.")]
        public string PolicyName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(1, 3650, ErrorMessage = "Retention period must be between 1 and 3650 days (10 years).")]
        public int RetentionPeriodInDays { get; set; }

        [Required(ErrorMessage = "Trigger Event is required.")]
        [RegularExpression("^(DataCreatedDate|LastActivity)$", ErrorMessage = "Unsupported trigger event. Allowed values: DataCreatedDate, LastActivity.")]
        public string TriggerEvent { get; set; } = "DataCreatedDate";

        public bool IsActive { get; set; }
    }
}