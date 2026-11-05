using System;
using System.Collections.Generic;

namespace PrivacyGuard.DTOs
{
    // List fetch karne ka response structure (with pagination details)
    public class DataPrincipalListResponseDto
    {
        public int TotalCount { get; set; }
        public List<DataPrincipalRowDto> Data { get; set; } = new List<DataPrincipalRowDto>();
    }

    // Grid ki har ek row ka structure (Masked data ke sath)
    public class DataPrincipalRowDto
    {
        public Guid DataPrincipalId { get; set; }
        public string ReferenceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ConsumerNo { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string PreferredLocale { get; set; } = string.Empty;
        public bool RequiresGuardianConsent { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
        public string Source { get; set; } = "Manual"; // Single Add ya Bulk Import Batch Name
    }
}