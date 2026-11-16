namespace PrivacyGuard.DTOs;

public sealed class CampaignAuditReportDto
{
    public Guid CampaignId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CampaignType { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public string TargetRole { get; set; } = string.Empty;
    public string CampaignStatus { get; set; } = string.Empty;
    public string LinkedDataInventory { get; set; } = "Not linked";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int TotalResponses { get; set; }
    public int AcceptedCount { get; set; }
    public int RejectedCount { get; set; }
    public int CompletedCount { get; set; }
    public decimal AcceptanceRate { get; set; }
    public decimal RejectionRate { get; set; }
    public decimal CompletionRate { get; set; }
    public string AuditResult { get; set; } = string.Empty;
    public string AuditRemarks { get; set; } = string.Empty;
    public List<string> ComplianceChecks { get; set; } = new();
    public List<CampaignResponseAuditDto> Responses { get; set; } = new();
}

public sealed class CampaignResponseAuditDto
{
    public string ResponseReference { get; set; } = string.Empty;
    public string ActorType { get; set; } = string.Empty;
    public string MaskedEmail { get; set; } = "Not available";
    public string ResponseStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}
