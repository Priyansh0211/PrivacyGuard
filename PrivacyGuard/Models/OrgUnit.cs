using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public class OrgUnit
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // 1. NAYI LINE: Kis main company ka branch hai yeh? (Multi-tenant link)
        [Required]
        public Guid OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        public Organization? Organization { get; set; } // Navigation property

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // Zone, Circle, Division

        public Guid? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public OrgUnit? Parent { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}