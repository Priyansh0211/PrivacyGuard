namespace PrivacyGuard.DTOs
{
    public class ConsentAuditExportDto
    {
        public Guid ConsentDetailId { get; set; }     // 1. Unique Detail ID
        public Guid ConsentRecordId { get; set; }     // 2. Parent Record ID
        public Guid ConsentPurposeId { get; set; }    // 3. Purpose ID
        public string Decision { get; set; } = string.Empty;        // 4. Decision (Yes/No)
        public string Status { get; set; } = string.Empty;          // 5. Status (Active, etc.)
        public string ExpiryDate { get; set; } = string.Empty;     // 6. Formatted Expiry Date
    }
}