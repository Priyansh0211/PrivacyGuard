namespace PrivacyGuard.DTOs
{
    public class UserDashboardDto
    {
        public int PrivacyHealthScore { get; set; }
        public int PendingActionsCount { get; set; }
        public int ActiveConsentsCount { get; set; }
        public int TotalRequestsRaised { get; set; }

        public List<MyActivityLogDto> RecentActivityLog { get; set; } = new();
        public List<MyRecentRequestDto> RecentRequests { get; set; } = new();
        public List<MyActiveConsentDto> RecentConsents { get; set; } = new();
    }

    public class MyActivityLogDto
    {
        public string Action { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class MyRecentRequestDto
    {
        public string RequestType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class MyActiveConsentDto
    {
        public string Purpose { get; set; } = string.Empty;
        public DateTime GrantedAt { get; set; }
    }
}