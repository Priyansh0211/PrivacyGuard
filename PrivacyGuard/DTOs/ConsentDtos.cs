using System;
using System.Collections.Generic;
using PrivacyGuard.Models;

namespace PrivacyGuard.DTOs
{
    // Frontend se naya consent create karte waqt yeh data aayega
    public class CreateConsentRequestDto
    {
        public Guid DataPrincipalId { get; set; }
        public ConsentChannel Channel { get; set; }
        public string Language { get; set; } = "en-IN";
        public List<PurposeDecisionDto> Decisions { get; set; } = new List<PurposeDecisionDto>();
    }

    // Har purpose (jaise billing, marketing) ke liye Yes ya No
    public class PurposeDecisionDto
    {
        public Guid ConsentPurposeId { get; set; }
        public string Decision { get; set; } = string.Empty;
    }
}