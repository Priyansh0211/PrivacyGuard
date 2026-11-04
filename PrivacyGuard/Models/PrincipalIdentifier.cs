using System;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class PrincipalIdentifier
    {
        [Key]
        public Guid IdentifierId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DataPrincipalId { get; set; }

        // e.g., "Mobile", "Email", "Consumer No"
        [Required]
        [MaxLength(50)]
        public string Kind { get; set; } = string.Empty;

        // Asli unmasked value yahan save hogi (e.g. 9712344997)
        [Required]
        [MaxLength(150)]
        public string Value { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;
        public bool IsVerified { get; set; } = true;

        public DataPrincipal? DataPrincipal { get; set; }
    }
}