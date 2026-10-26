using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public class Role
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OrganizationId { get; set; } 

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty; 

        [MaxLength(500)]
        public string? Description { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}