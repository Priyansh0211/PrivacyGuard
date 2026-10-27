using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class ConsentRecord
    {
        [Key]
        public Guid ConsentRecordId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrganizationId { get; set; } // Tumhara existing tenant ID

        [Required]
        public string DataPrincipalId { get; set; } = string.Empty; // Tumhara existing string type user ID

        [Required]
        [MaxLength(50)]
        public string NoticeVersion { get; set; } = "1.0"; // Ek baar save hoga ki user ne kaunsa version padha tha

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow; // 'GrantedAt' ki jagah SubmittedAt, kyunki form mein deny bhi ho sakta hai

        // Navigation Property: Ek form submission mein multiple checkbox (details) honge
        public virtual ICollection<ConsentDetail> ConsentDetails { get; set; } = new List<ConsentDetail>();
    }
}