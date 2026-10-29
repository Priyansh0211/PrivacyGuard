using Microsoft.AspNetCore.Http;

namespace PrivacyGuard.DTOs
{
    public class BulkImportRequestDto
    {
        // Dropdown se aayega, e.g., "Consumers"
        public string ImportType { get; set; } = "Consumers";

        // File ka naam reference ke liye
        public string? FileName { get; set; }

        // Agar user ne file upload ki hai
        public IFormFile? CsvFile { get; set; }

        // Agar user ne direct text paste kiya hai
        public string? RawCsvData { get; set; }
    }
}