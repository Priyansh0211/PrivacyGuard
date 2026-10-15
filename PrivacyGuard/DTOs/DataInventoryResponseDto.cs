namespace PrivacyGuard.DTOs
{
    public class DataInventoryResponseDto
    {
        public Guid DataInventoryId { get; set; }

        public string Name { get; set; } = string.Empty;

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