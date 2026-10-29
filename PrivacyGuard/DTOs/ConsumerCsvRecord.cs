using CsvHelper.Configuration.Attributes;

namespace PrivacyGuard.DTOs
{
    public class ConsumerCsvRecord
    {
        [Name("mobile")]
        public string? Mobile { get; set; }

        [Name("email")]
        public string? Email { get; set; }

        [Name("consumer_no")]
        public string? ConsumerNo { get; set; }

        [Name("display_name")]
        public string? DisplayName { get; set; }

        [Name("full_name")]
        public string? FullName { get; set; }

        [Name("date_of_birth")]
        public string? DateOfBirth { get; set; }

        [Name("class")]
        public string? Class { get; set; }

        [Name("preferred_locale")]
        public string? PreferredLocale { get; set; }
    }
}