using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public class ConsentRecords
    {
        [Key]
        public Guid ConsentRecordId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrganizationId { get; set; }

        [Required]
        public string DataPrincipalId { get; set; } = string.Empty; // User/Customer ki ID

        [Required]
        public Guid DataInventoryId { get; set; }

        [ForeignKey("DataInventoryId")]
        public DataInventory? DataInventory { get; set; }

        [Required]
        [MaxLength(20)]
        public string ConsentStatus { get; set; } = "Granted"; // "Granted" ya "Withdrawn"

        [Required]
        [MaxLength(50)]
        public string NoticeVersion { get; set; } = "1.0"; // DPDP notice proof

        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
        public DateTime? WithdrawnAt { get; set; }
    }
}