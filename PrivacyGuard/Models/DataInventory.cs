using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class DataInventory
    {
        [Key]
        public Guid DataInventoryId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrganizationId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string DataCategory { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Purpose { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? DataSource { get; set; }

        [MaxLength(200)]
        public string? StorageLocation { get; set; }

        [Range(1, 36500)]
        public int? RetentionPeriodDays { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Organization? Organization { get; set; }
    }
}