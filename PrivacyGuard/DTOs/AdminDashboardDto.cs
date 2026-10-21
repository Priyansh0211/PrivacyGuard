namespace PrivacyGuard.DTOs
{
    public class AdminDashboardDto
    {
        // 1. Overall Health
        public int ComplianceHealthScore { get; set; }

        // 2. Basic Stats
        public int TotalUsers { get; set; }
        public int ActiveConsents { get; set; }

        // 3. SLA & Risk (DPDP Specific)
        public int PendingRequests { get; set; }
        public int SlaRiskRequests { get; set; }
        public int TotalDataBreaches { get; set; }

        // 4. Campaign Analytics
        public List<CampaignStatsDto> CampaignPerformances { get; set; } = new();

        // 5. Quick Action List
        public List<RecentBreachDto> RecentBreaches { get; set; } = new();
    }

    public class RecentBreachDto
    {
        public string Title { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime IncidentDate { get; set; }
    }
}