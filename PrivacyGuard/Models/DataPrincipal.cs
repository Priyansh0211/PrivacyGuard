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

        // Credentials used by the shared login endpoint.
        // Existing records must be assigned a password before they can log in.
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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ==========================================
        // 360-DEGREE VIEW CONNECTIONS (Navigation Properties)
        // ==========================================

        // 1. Identifiers list (Email, Mobile mappings)
        public ICollection<PrincipalIdentifier> Identifiers { get; set; } = new List<PrincipalIdentifier>();

        // 2. User ki saari Rights/Privacy Requests
        public ICollection<PrivacyRequest> PrivacyRequests { get; set; } = new List<PrivacyRequest>();

        // 3. User ke saare Consents (Jo record table tumne pehle banai thi)
        public ICollection<ConsentRecord> ConsentRecords { get; set; } = new List<ConsentRecord>();
    }
}