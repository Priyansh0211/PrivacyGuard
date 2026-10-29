namespace PrivacyGuard.DTOs
{
    public class CreateConsumerDto
    {
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? ConsumerNo { get; set; }
        public string? FullName { get; set; }
        public string? DateOfBirth { get; set; } // yyyy-MM-dd format
        public string Class { get; set; } = "adult"; // adult, child, person_with_disability
        public string PreferredLocale { get; set; } = "en-IN";
    }
}