using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public class ConsentDetail
    {
        [Key]
        public Guid ConsentDetailId { get; set; } = Guid.NewGuid();

        // Master table se link karne ke liye Foreign Key
        [Required]
        public Guid ConsentRecordId { get; set; }

        [ForeignKey("ConsentRecordId")]
        public ConsentRecord? ConsentRecord { get; set; }

        // Yeh tumhara existing field hai (e.g., "WhatsApp Alert" ya "Marketing Email" inventory ID)
        [Required]
        public Guid DataInventoryId { get; set; }

        [ForeignKey("DataInventoryId")]
        public DataInventory? DataInventory { get; set; }

        // Yahan aayega tumhara granular status: "Granted", "Denied", ya "Withdrawn"
        [Required]
        [MaxLength(20)]
        public string ConsentStatus { get; set; } = string.Empty;

        // Agar user pehle Granted kare aur baad mein withdraw kare, toh yeh update hoga
        public DateTime? WithdrawnAt { get; set; }
    }
}