using System;

namespace PrivacyGuard.DTOs
{
    public class PublicNoticeListDto
    {
        public string Code { get; set; }
        public int Version { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string LogoUrl { get; set; }
        public string PrimaryColor { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}