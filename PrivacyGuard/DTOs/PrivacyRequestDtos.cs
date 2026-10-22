namespace PrivacyGuard.DTOs
{
    // User jab request raise karega
    public class CreatePrivacyRequestDto
    {
        public string DataPrincipalId { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string RequestDetails { get; set; } = string.Empty;
    }

    // Admin jab request resolve karega
    public class ResolvePrivacyRequestDto
    {
        public string Status { get; set; } = string.Empty; // e.g., "Resolved"
        public string ResolutionRemarks { get; set; } = string.Empty; 
    }
}