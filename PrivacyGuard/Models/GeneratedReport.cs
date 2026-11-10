using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public class GeneratedReport
    {
        [Key]
        public Guid ReportId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid JobId { get; set; }

        [Required]
        public Guid OrganizationId { get; set; }

        [Required, MaxLength(500)]
        public string FilePath { get; set; } // S3 ya Local path

        [Required, MaxLength(64)]
        public string ChecksumSha256 { get; set; } // Tamper-proof evidence

        // --- Naye Fields (Audit & Analytics) ---
        public int RecordCount { get; set; } // Report me kitni rows hain (e.g., 5000 users)

        public long FileSizeKb { get; set; } // File ka size UI par dikhane ke liye

        public int DownloadCount { get; set; } = 0; // Security track: file kitni baar download hui

        public DateTime ExpiresAt { get; set; } // Download link expiry

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("JobId")]
        public ReportJob? ReportJob { get; set; }
    }
}