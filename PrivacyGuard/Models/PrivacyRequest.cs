using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class PrivacyRequest
    {
        [Key]
        public Guid RequestId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrganizationId { get; set; } // Multi-tenant ID

        [Required]
        public string DataPrincipalId { get; set; } = string.Empty; // User kisne request ki

        // Request ka type: "Access", "Correction", "Erasure", ya "Grievance"
        [Required]
        [MaxLength(50)]
        public string RequestType { get; set; } = string.Empty;

        // User ne detail mein kya manga hai
        [Required]
        public string RequestDetails { get; set; } = string.Empty;

        // Current status: "Pending", "In-Progress", "Resolved", "Rejected"
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending";

        // Admin/DPO ne kya action liya uska note
        public string? ResolutionRemarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }

        // DPDP kehta hai ki requests ek fix time mein resolve honi chahiye (SLA tracking)
        public DateTime TargetResolutionDate => CreatedAt.AddDays(30);
    }
}