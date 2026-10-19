namespace PrivacyGuard.DTOs
{
    public class DataInventoryResponseDto
    {
        public Guid DataInventoryId { get; set; }

        // Name: Data point ka naam - Jaise "Email Address" ya "Aadhar Number" - Create API ke form me admin yeh fill karega
        public string Name { get; set; } = string.Empty;

        // DataCategory: Yeh kis type ka data hai - Jaise "Contact Info" ya "Sensitive Personal Data" - GetAll API list me filter lagane ke kaam aayega

        public string DataCategory { get; set; } = string.Empty;

        public string Purpose { get; set; } = string.Empty;

        public string? DataSource { get; set; }

        public string? StorageLocation { get; set; }

        public int? RetentionPeriodDays { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}