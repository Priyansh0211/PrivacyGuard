using System;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class DataPrincipal
    {
        [Key]
        public Guid DataPrincipalId { get; set; }

        public Guid OrganizationId { get; set; }

        // Auto-generated reference format: PSD-XXXXXXXX
        [Required]
        [MaxLength(20)]
        public string ReferenceId { get; set; }

        [MaxLength(15)]
        public string? Mobile { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? ConsumerNo { get; set; }

        [MaxLength(100)]
        public string? DisplayName { get; set; }

        [MaxLength(150)]
        public string? FullName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        // Restricted to: adult, child, person_with_disability
        [Required]
        [MaxLength(30)]
        public string Class { get; set; } = "adult";

        // Supported locales: mr-IN, en-IN, hi-IN
        [MaxLength(10)]
        public string PreferredLocale { get; set; } = "en-IN";

        // DPDP Rule for child or person_with_disability
        public bool RequiresGuardianConsent { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}