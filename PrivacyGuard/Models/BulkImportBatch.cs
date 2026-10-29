using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class BulkImportBatch
    {
        [Key]
        public Guid BatchId { get; set; }

        public Guid OrganizationId { get; set; }
        public Guid UploadedByUserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty; // e.g. pune-circle-migration.csv

        [Required]
        [MaxLength(50)]
        // UI Dropdown ke hisaab se: "Consumers" ya "Consumers + consent decisions"
        public string ImportType { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        // UI Dropdown wale exact status: Uploaded, Validating, Validated, Importing, Completed, Failed, Cancelled
        public string Status { get; set; } = "Uploaded";

        public int TotalRows { get; set; } = 0;
        public int ProcessedRows { get; set; } = 0;
        public int FailedRows { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // CS8618 Warning Fix: Collection ko initialize kar diya taaki null reference error na aaye
        public ICollection<BulkImportRow> Rows { get; set; } = new List<BulkImportRow>();
    }
}