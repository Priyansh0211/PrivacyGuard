public class GrievanceAuditExportDto
{
    public string ReferenceId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string DataPrincipalEmail { get; set; } = string.Empty; // PII Masked email
    public string AssignedAdminEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}