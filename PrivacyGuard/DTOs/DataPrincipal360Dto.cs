using System;
using System.Collections.Generic;

namespace PrivacyGuard.DTOs
{
    // Main DTO jo API response mein jayega
    public class DataPrincipal360Dto
    {
        public RecordDto Record { get; set; } = new RecordDto();
        public List<IdentifierDto> Identifiers { get; set; } = new List<IdentifierDto>();
        public List<RightRequestSummaryDto> RightsRequests { get; set; } = new List<RightRequestSummaryDto>();
        public List<ConsentStateDto> ActiveConsents { get; set; } = new List<ConsentStateDto>();
    }

    // 1. Block: Basic Record Info
    public class RecordDto
    {
        public string ReferenceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty; // Masked Name jayega yahan
        public string Class { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string PreferredLanguage { get; set; } = string.Empty;
        public DateTime RegisteredDate { get; set; }
    }

    // 2. Block: Identifiers (Email, Mobile)
    public class IdentifierDto
    {
        public string Kind { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty; // Masked Value jayegi (e.g. 97****4997)
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
    }

    // 3. Block: Rights Requests History
    public class RightRequestSummaryDto
    {
        public Guid RequestId { get; set; }
        public string Reference { get; set; } = string.Empty; // e.g., DSR-2026-00024
        public string RightExercised { get; set; } = string.Empty; // Access, Erasure
        public string Status { get; set; } = string.Empty;
        public DateTime RaisedOn { get; set; }
        public DateTime Deadline { get; set; }
    }

    // 4. Block: Active Consents
    public class ConsentStateDto
    {
        public string PurposeName { get; set; } = string.Empty;
        public DateTime GrantedOn { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}