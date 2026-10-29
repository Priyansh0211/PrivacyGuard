using System;
using System.Collections.Generic;

namespace PrivacyGuard.Models
{
    // Enums ko yahin file mein define kar diya taaki CS0246 error kabhi na aaye
    public enum ConsentStatus
    {
        Granted,
        PartiallyGranted,
        Withdrawn,
        Expired
    }

    public enum ConsentChannel
    {
        Web,
        MobileApp,
        CallCentre
    }

    public class ConsentRecord
    {
        public Guid ConsentRecordId { get; set; }
        public Guid OrganizationId { get; set; }

        public Guid DataPrincipalId { get; set; }

        public string ReferenceNumber { get; set; } = string.Empty;
        public string ReceiptId { get; set; } = string.Empty;

        public ConsentChannel Channel { get; set; } = ConsentChannel.Web;
        public string Language { get; set; } = "en-IN";
        public ConsentStatus OverallStatus { get; set; }

        public string ArtifactHash { get; set; } = string.Empty;
        public string DigitalSignature { get; set; } = string.Empty;
        public string KeyId { get; set; } = string.Empty;

        public Guid? PreviousRecordId { get; set; }
        public int Version { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ConsentDetail> ConsentDetails { get; set; } = new List<ConsentDetail>();
    }
}