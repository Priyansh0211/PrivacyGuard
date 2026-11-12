using System;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    // Ye table define karti hai ki data kab tak rakhna hai aur kab delete karna hai
    public class RetentionPolicy
    {
        // Primary key is policy table ke liye
        [Key]
        public Guid PolicyId { get; set; }

        // Multi-tenant architecture ke liye OrganizationId zaroori hai
        public Guid OrganizationId { get; set; }

        // Policy ka naam (e.g., "Financial Data Policy", "Inactive User Policy")
        [Required]
        [MaxLength(200)]
        public string PolicyName { get; set; } = string.Empty;

        // Policy kya karti hai uski detail description
        public string Description { get; set; } = string.Empty;

        // DPDP Act ke hisaab se kitne din tak data rakhne ki permission hai
        public int RetentionPeriodInDays { get; set; }

        // Countdown kab start hoga? (e.g., "ConsentWithdrawn", "LastLoginDate", "DataCreatedDate")
        [Required]
        public string TriggerEvent { get; set; } = string.Empty;

        // Data delete karna hai (Erasure) ya sirf naam chhupana hai (Anonymization)
        public string ActionType { get; set; } = "Anonymize";

        // Agar false kar diya, toh background job is policy ko ignore kar degi
        public bool IsActive { get; set; } = true;

        // Record kab bana tha
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Record last kab update hua
        public DateTime? UpdatedAt { get; set; }
    }
}