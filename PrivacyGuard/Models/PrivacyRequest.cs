using System;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class PrivacyRequest
    {
        [Key]
        public Guid RequestId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrganizationId { get; set; }

        // BUG FIXED: string se Guid kar diya taaki DataPrincipal se link ho sake
        [Required]
        public Guid DataPrincipalId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequestType { get; set; } = string.Empty;

        [Required]
        public string RequestDetails { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending";

        public string? ResolutionRemarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
        public DateTime TargetResolutionDate => CreatedAt.AddDays(30);

        // NAVIGATION PROPERTY: EF Core ko batane ke liye ki ye request kis user ki hai
        public DataPrincipal? DataPrincipal { get; set; }
    }
}