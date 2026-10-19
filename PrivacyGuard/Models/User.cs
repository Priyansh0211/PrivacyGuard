using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();

        // Tenant Isolation: Har user ek organization se juda hoga
        public Guid OrganizationId { get; set; }
        [ForeignKey("OrganizationId")]
        public Organization? Organization { get; set; }



        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;



        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;


                                                                                                                                                                                                                                    
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Role { get; set; } = "DataPrincipal";

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginDate { get; set; }

        [MaxLength(50)]
        public string? LastLoginIP { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}