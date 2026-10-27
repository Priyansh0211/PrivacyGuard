using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.DTOs
{
    public class SubmitConsentDto
    {
        [Required]
        public Guid DataPrincipalId { get; set; }

        [Required]
        public List<ConsentPreferenceDto> Preferences { get; set; } = new();
    }

    public class ConsentPreferenceDto
    {
        [Required]
        public Guid DataInventoryId { get; set; } // Kis data/purpose ke liye hai

        [Required]
        public string Status { get; set; } = string.Empty; // "Granted" ya "Denied"
    }
}