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
        public string ImportType { get; set; } = string.Empty; // "Consumers" ya aur koi type

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Staged"; // Staged, Processing, Completed, Failed

        public int TotalRows { get; set; } = 0;
        public int ProcessedRows { get; set; } = 0;
        public int FailedRows { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public ICollection<BulkImportRow> Rows { get; set; } 
    }
}