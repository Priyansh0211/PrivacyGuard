using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public class Grievance
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Privacy ID (e.g., GRV-20261026-1234)
        [Required, MaxLength(50)]
        public string ReferenceId { get; set; } = string.Empty;

        // Tenant Isolation
        public Guid OrganizationId { get; set; }

        // Jis user (Data Principal) ne shikayat ki
        public Guid DataPrincipalId { get; set; }
        [ForeignKey("DataPrincipalId")]
        public User? DataPrincipal { get; set; }

        [Required, MaxLength(255)]
        public string Subject { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = "Other";

        public string Severity { get; set; } = "Medium";

        public string Status { get; set; } = "Open";

        // Jis Admin/DPO ne ticket li
        public Guid? AssignedToId { get; set; }
        [ForeignKey("AssignedToId")]
        public User? AssignedAdmin { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property: Ek ticket ke andar bahut saare messages/notes honge
        public ICollection<GrievanceMessage> Messages { get; set; } = new List<GrievanceMessage>();
    }
}