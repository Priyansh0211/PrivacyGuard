using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace PrivacyGuard.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string ReferenceId { get; set; } = string.Empty;

        // Tenant Isolation
        public Guid OrganizationId { get; set; }
        [ForeignKey("OrganizationId")]
        public Organization? Organization { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Department { get; set; }

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        public Guid? OrgUnitId { get; set; } // Nullable rakha hai taaki admin ke liye khali reh sake
        public virtual OrgUnit? OrgUnit { get; set; } 
                                                                                                                                                                                                                                    
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

       
        public Guid? RoleId { get; set; }
        [ForeignKey("RoleId")]
        public Role? UserRole { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginDate { get; set; }

        [MaxLength(50)]
        public string? LastLoginIP { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}