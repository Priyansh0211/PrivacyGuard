using System;

namespace PrivacyGuard.Models
{
    // Screenshot 103: Data Categories
    public class DataCategory
    {
        public Guid DataCategoryId { get; set; }
        public Guid OrganizationId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Sensitivity { get; set; } = "Low";
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    // Screenshot 104: Data Processors (Third Party)
    public class DataProcessor
    {
        public Guid DataProcessorId { get; set; }
        public Guid OrganizationId { get; set; }
        public string Processor { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Relationship { get; set; } = "Processor";
        public string Location { get; set; } = "IN";
        public string LegalName { get; set; } = string.Empty;
        public string Country { get; set; } = "IN";
        public bool DataLeavesIndia { get; set; }
    }

    // Mapping Tables (Many-to-Many join tables)
    public class ConsentPurposeDataCategory
    {
        public Guid ConsentPurposeId { get; set; }
        public ConsentPurpose ConsentPurpose { get; set; }
        public Guid DataCategoryId { get; set; }
        public DataCategory DataCategory { get; set; }
    }

    public class ConsentPurposeDataProcessor
    {
        public Guid ConsentPurposeId { get; set; }
        public ConsentPurpose ConsentPurpose { get; set; }
        public Guid DataProcessorId { get; set; }
        public DataProcessor DataProcessor { get; set; }
    }
}