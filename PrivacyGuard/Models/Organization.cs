using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class Organization
    {

        [Key]
        public Guid OrganizationId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
