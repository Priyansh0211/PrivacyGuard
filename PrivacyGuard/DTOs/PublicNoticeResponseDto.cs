namespace PrivacyGuard.DTOs
{
    public class PublicNoticeResponseDto
    {
        public string Code { get; set; }
        public int Version { get; set; }
        public string Layout { get; set; }
        public string PrimaryColor { get; set; }
        public string LogoUrl { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string PurposesJson { get; set; }
        public string Sha256Hash { get; set; }
    }
}