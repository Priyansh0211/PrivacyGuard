using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.DTOs
{
    public class UpdateDataInventoryDto
    {
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
    }
}