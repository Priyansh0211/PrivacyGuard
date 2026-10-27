using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public class OrgUnit
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid? ParentId { get; set; } // Null for "New top-level unit" (Zone), Guid for "Add child"

        [Required]
        public string Name { get; set; } = string.Empty; // e.g., "Amravati"

        public string UnitType { get; set; } = string.Empty; // "Zone", "Circle", ya "Division"
        public string UnitCode { get; set; } = string.Empty; // "zone-amravati" (Unique identifier)

        public bool IsActive { get; set; } = true; // "Deactivate" button ke liye
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}