using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class DpiaRecord
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrganizationId { get; set; } // Multi-tenant isolation

        [Required]
        [MaxLength(50)]
        public string ReferenceId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public int PurposesCount { get; set; }

        [Required]
        [MaxLength(20)]
        public string ResidualRisk { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "In Review";

        public string AssessmentDetails { get; set; } = string.Empty;

        public string? ReviewNotes { get; set; }

        // Audit Trail Fields
        [Required]
        public string CreatedByUserId { get; set; } = string.Empty; // Maker
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? ReviewedByUserId { get; set; } // Checker (DPO)
        public DateTime? ReviewedAt { get; set; }

        public DateTime? NextReviewDate { get; set; }
    }
}