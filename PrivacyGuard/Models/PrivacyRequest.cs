using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public class PrivacyRequest
    {
        [Key]
        public Guid RequestId { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required, MaxLength(50)]
        public string RequestType { get; set; } = string.Empty; 

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Open";

        public string? RequestDetails { get; set; }

        public Guid? AssignedAdminId { get; set; }

        public DateTime RaisedOn { get; set; } = DateTime.UtcNow;
        public DateTime SlaDueDate { get; set; } = DateTime.UtcNow.AddDays(30);

        public string? ResolutionRemarks { get; set; }
        public DateTime? ResolvedOn { get; set; }
    }
}