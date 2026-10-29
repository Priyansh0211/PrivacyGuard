using System;

namespace PrivacyGuard.Models
{
    public class ConsentDetail
    {
        public Guid ConsentDetailId { get; set; }
        public Guid ConsentRecordId { get; set; }

        // Link to new ConsentPurpose entity[cite: 9]
        public Guid ConsentPurposeId { get; set; }
        public ConsentPurpose ConsentPurpose { get; set; } = null!;

        public string Decision { get; set; } = "No";

        // Granular per-purpose status & expiry[cite: 10]
        public string Status { get; set; } = "Active";
        public DateTime? ExpiryDate { get; set; }

        public ConsentRecord ConsentRecord { get; set; } = null!;
    }
}