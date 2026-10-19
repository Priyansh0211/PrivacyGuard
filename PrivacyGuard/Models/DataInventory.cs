using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class DataInventory
    {
        [Key]
        public Guid DataInventoryId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrganizationId { get; set; }

        // Name: Data point ka naam - Jaise "Email Address" ya "Aadhar Number" - Create API ke form me admin yeh fill karega
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        // DataCategory: Yeh kis type ka data hai - Jaise "Contact Info" ya "Sensitive Personal Data" - GetAll API list me filter lagane ke kaam aayega
        [Required]
        [MaxLength(100)]
        public string DataCategory { get; set; } = string.Empty;

        // Purpose: DPDP Act ka rule hai ki user ko batana hoga ki data kyun le rahe hain - Jaise "OTP verification aur account recovery ke liye" - Yeh column GetById API ke through user ke consent form (notice) par dikhaya jayega
        [Required]
        [MaxLength(300)]
        public string Purpose { get; set; } = string.Empty;

        // DataSource: Data kahan se mil raha hai - Jaise "User Input form" ya "Third-party vendor"
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