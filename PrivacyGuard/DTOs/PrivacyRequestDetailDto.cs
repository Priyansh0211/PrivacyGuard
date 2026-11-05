using System;

namespace PrivacyGuard.DTOs
{
    public class PrivacyRequestDetailDto
    {
        public Guid RequestId { get; set; }
        public string ReferenceId { get; set; } = string.Empty;

        // Data Principal Info (Masked for safety)
        public Guid DataPrincipalId { get; set; }
        public string PrincipalName { get; set; } = string.Empty;
        public string PrincipalEmail { get; set; } = string.Empty;
        public string PrincipalMobile { get; set; } = string.Empty;

        // Request Specifics
        public string RequestType { get; set; } = string.Empty;
        public string RequestDetails { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        // Timelines & Resolution
        public DateTime CreatedAt { get; set; }
        public DateTime TargetResolutionDate { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string ResolutionRemarks { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
    }
}