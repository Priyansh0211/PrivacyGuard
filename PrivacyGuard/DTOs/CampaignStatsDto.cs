namespace PrivacyGuard.DTOs
{
    public class CampaignStatsDto
    {

        public Guid CampaignId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TotalResponses { get; set; }
        public int AcceptedCount { get; set; }
        public int RejectedCount { get; set; }
        public int CompletedCount { get; set; } // Awareness campaigns ke liye
    }
}
