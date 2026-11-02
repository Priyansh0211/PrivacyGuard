namespace PrivacyGuard.DTOs
{
    public class CreateDataCategoryDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Sensitivity { get; set; } = "Low"; // Low, Medium, High, Critical
    }

    public class CreateDataProcessorDto
    {
        public string Code { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string Relationship { get; set; } = "Processor"; // Processor, Sub Processor, Fiduciary
        public string Country { get; set; } = "IN";
        public bool DataLeavesIndia { get; set; }
    }
}