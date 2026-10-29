using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyGuard.Models
{
    public class BulkImportRow
    {
        [Key]
        public Guid RowId { get; set; }

        public Guid BatchId { get; set; }
        [ForeignKey("BatchId")]
        public BulkImportBatch Batch { get; set; }

        public int RowNumber { get; set; } // CSV mein line number

        [Required]
        public string RawData { get; set; } // Puri CSV row ko comma separated ya JSON mein save karenge

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Success, Failed

        public string? ErrorMessage { get; set; } // Agar fail hua toh reason
    }
}