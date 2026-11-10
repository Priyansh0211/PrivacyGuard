using System;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class ReportJob
    {
        [Key]
        public Guid JobId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrganizationId { get; set; } // Tenant Isolation

        [Required, MaxLength(100)]
        public string ReportType { get; set; } // e.g., "ConsentAudit", "CampaignResponse"

        public string? ParametersJson { get; set; } // Filters (dates, campaignId, etc.)

        [Required]
        public Guid RequestedBy { get; set; } // DPO ya Admin ki ID

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Queued"; // Queued, Processing, Completed, Failed

        // --- Naye Fields (Enterprise Features) ---
        [Required, MaxLength(20)]
        public string ReportFormat { get; set; } = "CSV"; // Default CSV, baad me Excel/PDF aayega

        public int Priority { get; set; } = 1; // 1 = Normal, 2 = High (Urgent requests ke liye)

        public int RetryCount { get; set; } = 0; // Agar process fail ho to track karega

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }
}