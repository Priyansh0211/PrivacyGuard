namespace PrivacyGuard.DTOs
{
    public class ComplianceChecklistDto
    {
        public int TotalItems { get; set; }
        public int TrackedItems { get; set; }
        public int CompleteItems { get; set; }
        public int NeedsAttentionItems { get; set; }
        public int NotTrackedItems { get; set; }
        public int ProgressPercent { get; set; }
        public string Disclaimer { get; set; } = "Checklist status reflects records available in PrivacyGuard and is not a legal compliance determination.";
        public List<ComplianceChecklistItemDto> Items { get; set; } = new();
    }

    public class ComplianceChecklistItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Requirement { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string EvidenceSummary { get; set; } = string.Empty;
        public int EvidenceCount { get; set; }
        public string NextAction { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
    }
}
