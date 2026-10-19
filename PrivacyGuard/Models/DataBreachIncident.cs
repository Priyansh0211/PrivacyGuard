using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public class DataBreachIncident
    {
        [Key]
        public Guid Id { get; set; }

        // Tenant Isolation
        [Required]
        public Guid OrganizationId { get; set; }

        // Incident Details
        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public string Severity { get; set; } = "Unknown"; // "Low", "Medium", "High", "Critical", "Unknown"

        // Timing
        public DateTime IncidentDate { get; set; }
        public DateTime DiscoveryDate { get; set; }

        // Impact
        public int EstimatedAffectedRecords { get; set; } = 0;

        // Workflow & Response
        public string Status { get; set; } = "Open"; // "Open", "Investigating", "Mitigated", "Closed"

        public Guid? ReportedByUserId { get; set; }
        [ForeignKey("ReportedByUserId")]
        public User? ReportedByUser { get; set; }

        public Guid? AssignedToUserId { get; set; }
        [ForeignKey("AssignedToUserId")]
        public User? AssignedUser { get; set; }

        public string? MitigationSteps { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }
    }
}
