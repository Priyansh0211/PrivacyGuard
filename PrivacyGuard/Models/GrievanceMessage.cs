using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public class GrievanceMessage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

  
        public Guid GrievanceId { get; set; }
        [ForeignKey("GrievanceId")]
        public Grievance? Grievance { get; set; }

       
        public Guid SenderId { get; set; }
        [ForeignKey("SenderId")]
        public User? Sender { get; set; }

        [Required]
        public string MessageText { get; set; } = string.Empty;

       
        public bool IsInternal { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}