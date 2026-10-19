namespace PrivacyGuard.DTOs
{
    // Employee jab naya ticket banayega
    public class ReportBreachDto
    {
        public Guid OrganizationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime IncidentDate { get; set; }
        public DateTime DiscoveryDate { get; set; }
    }

    // Admin jab ticket par action lega (Triage, Mitigation, Closure)
    public class UpdateBreachStatusDto
    {
        public string Status { get; set; } = string.Empty; // "Investigating", "Mitigated", "Closed"
        public string? Severity { get; set; } // "Low", "Medium", "High", "Critical"
        public int? EstimatedAffectedRecords { get; set; }
        public string? MitigationSteps { get; set; }
    }

    // Frontend ko data wapas bhejne ke liye
    public class BreachResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public int EstimatedAffectedRecords { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DiscoveryDate { get; set; }
        public string? ReporterName { get; set; }
        public string? AssignedUserName { get; set; }
        public string? MitigationSteps { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}
