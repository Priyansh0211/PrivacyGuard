using System;
using System.Collections.Generic;

namespace PrivacyGuard.Models
{
    public class ConsentPurpose
    {
        public Guid ConsentPurposeId { get; set; }
        public Guid OrganizationId { get; set; }

        public string Code { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public bool IsMandatory { get; set; } = false;

        public string Description { get; set; } = string.Empty;
        public string LegalBasis { get; set; } = string.Empty;
        public string RetentionPeriod { get; set; } = string.Empty;
        public bool DoesConsentLapse { get; set; } = false;
        public string RiskLevel { get; set; } = "Low risk";

        public bool IsActive { get; set; } = true;
        public bool IsProhibitedForChildren { get; set; } = false;

        public int Version { get; set; } = 1;
        public Guid? PreviousPurposeId { get; set; } // Agar edit hua hai toh purane record ki ID
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- MAPPINGS (Data Categories aur Processors ke liye) ---
        public ICollection<ConsentPurposeDataCategory> Categories { get; set; } = new List<ConsentPurposeDataCategory>();
        public ICollection<ConsentPurposeDataProcessor> Processors { get; set; } = new List<ConsentPurposeDataProcessor>();
    }
}