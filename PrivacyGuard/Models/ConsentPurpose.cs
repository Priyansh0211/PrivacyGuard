using System;

namespace PrivacyGuard.Models
{
    // PDF Req: Dedicated entity, Legal Basis, IsMandatory[cite: 9]
    public class ConsentPurpose
    {
        public Guid ConsentPurposeId { get; set; }
        public Guid OrganizationId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        public string LegalBasis { get; set; } = "Consent";
    }
}