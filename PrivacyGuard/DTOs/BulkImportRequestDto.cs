using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.DTOs
{
    public class BulkImportRequestDto
    {
        [Required]
        public string ImportType { get; set; } = string.Empty; // Dropdown value: "Consumers" ya "Consumers + consent decisions"

        public string? FileName { get; set; }

        public IFormFile? CsvFile { get; set; }

        public string? RawCsvText { get; set; }

        public bool IsValid()
        {
            // Dono mein se ek cheez aani hi chahiye tabhi request aage badhegi
            return (CsvFile != null && CsvFile.Length > 0) || !string.IsNullOrWhiteSpace(RawCsvText);
        }
    }
}