namespace PrivacyGuard.DTOs
{
    public class GrantConsentDto
    {
        public string DataPrincipalId { get; set; } = string.Empty;
        public Guid DataInventoryId { get; set; }
        public string NoticeVersion { get; set; } = "1.0";
    }
}