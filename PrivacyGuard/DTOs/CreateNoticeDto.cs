using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.DTOs
{
    public class CreateNoticeDto
    {
        [Required] public string Code { get; set; } = string.Empty;
        [Required] public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        [Required] public string PrimaryLanguage { get; set; } = string.Empty;
    }

    public class NoticeListDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int PurposesCount { get; set; }
        public int Version { get; set; }
        public string? Sha256Hash { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TranslationDto
    {
        public string LanguageCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public bool IsTranslated { get; set; }
    }

    public class NoticeDetailDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Version { get; set; }
        public string? Sha256Hash { get; set; }
        public string Layout { get; set; } = string.Empty;
        public string PrimaryColor { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string PurposesJson { get; set; } = "[]";
        public List<TranslationDto> Translations { get; set; } = new List<TranslationDto>();
    }

    public class UpdateNoticeDto
    {
        [Required]
        public Guid NoticeTemplateId { get; set; }

        public List<string> Purposes { get; set; } = new List<string>();
        public List<TranslationDto> Translations { get; set; } = new List<TranslationDto>();
    }
}