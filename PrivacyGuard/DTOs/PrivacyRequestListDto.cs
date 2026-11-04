using System;
using System.Collections.Generic;

namespace PrivacyGuard.DTOs
{
    public class PrivacyRequestListResponseDto
    {
        public int TotalCount { get; set; }
        public List<PrivacyRequestRowDto> Data { get; set; } = new List<PrivacyRequestRowDto>();
    }

    public class PrivacyRequestRowDto
    {
        public Guid RequestId { get; set; }
        public string ReferenceId { get; set; } = string.Empty;

        // Data Principal Details
        public Guid DataPrincipalId { get; set; }
        public string PrincipalName { get; set; } = string.Empty;

        public string RequestType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime TargetResolutionDate { get; set; }
        public bool IsOverdue { get; set; }
    }
}