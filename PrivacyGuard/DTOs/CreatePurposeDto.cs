using System;
using System.Collections.Generic;

namespace PrivacyGuard.DTOs
{
    public class CreatePurposeDto
    {
        public string Code { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        public string LegalBasis { get; set; } = string.Empty;
        public string RetentionPeriod { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = "Low risk";
        public bool DoesConsentLapse { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsProhibitedForChildren { get; set; } = false;

        // Jo categories aur processors Admin select karega, unki IDs aayengi
        public List<Guid> CategoryIds { get; set; } = new List<Guid>();
        public List<Guid> ProcessorIds { get; set; } = new List<Guid>();
    }
}