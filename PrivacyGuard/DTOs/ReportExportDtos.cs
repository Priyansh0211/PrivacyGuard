using System;

namespace PrivacyGuard.DTOs
{
    public class DsarReportExportDto
    {
        public string ReferenceId { get; set; } = string.Empty;
        public string DataPrincipalReferenceId { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime TargetResolutionDate { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class DataInventoryReportExportDto
    {
        public string Name { get; set; } = string.Empty;
        public string DataCategory { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string? DataSource { get; set; }
        public string? StorageLocation { get; set; }
        public int? RetentionPeriodDays { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }

    public class DataProcessingReportExportDto
    {
        public string Purpose { get; set; } = string.Empty;
        public string DataCategory { get; set; } = string.Empty;
        public string LegalBasis { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }

    public class DataRetentionReportExportDto
    {
        public string DataElement { get; set; } = string.Empty;
        public string DataCategory { get; set; } = string.Empty;
        public int? RetentionPeriodDays { get; set; }
        public DateTime? ExpectedDeletionDate { get; set; }
        public string ComplianceStatus { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class DataSharingReportExportDto
    {
        public string Processor { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool DataLeavesIndia { get; set; }
    }

    public class SecurityAuditReportExportDto
    {
        public long SequenceNumber { get; set; }
        public string? ActorEmail { get; set; }
        public string? ActorRole { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? EntryHash { get; set; }
    }

    public class ComplianceGapReportExportDto
    {
        public string Area { get; set; } = string.Empty;
        public string Requirement { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public string Gap { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
    }
}
