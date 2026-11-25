using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public class DataPrincipal
    {
        [Key]
        public Guid DataPrincipalId { get; set; }

        public Guid OrganizationId { get; set; }
        public Guid? OrgUnitId { get; set; }

        public Guid? RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public Role? Role { get; set; }

        [Required]
        [MaxLength(20)]
        public string ReferenceId { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? Mobile { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        public string? PasswordHash { get; set; }
        public bool IsActive { get; set; } = true;

        [MaxLength(50)]
        public string? ConsumerNo { get; set; }

        [MaxLength(100)]
        public string? DisplayName { get; set; }

        [MaxLength(150)]
        public string? FullName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Required]
        [MaxLength(30)]
        public string Class { get; set; } = "adult";

        [MaxLength(10)]
        public string PreferredLocale { get; set; } = "en-IN";

        public bool RequiresGuardianConsent { get; set; } = false;

        // ==========================================
        // RETENTION, ERASURE & LEGAL HOLD (DPDP Sec 8(7))
        // ==========================================
        public DateTime? AnonymizedAt { get; set; }

        public bool IsOnLegalHold { get; set; } = false;

        [MaxLength(250)]
        public string? LegalHoldReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ==========================================
        // 360-DEGREE VIEW CONNECTIONS
        // ==========================================
        public ICollection<PrincipalIdentifier> Identifiers { get; set; } = new List<PrincipalIdentifier>();
        public ICollection<PrivacyRequest> PrivacyRequests { get; set; } = new List<PrivacyRequest>();
        public ICollection<ConsentRecord> ConsentRecords { get; set; } = new List<ConsentRecord>();
    }
}