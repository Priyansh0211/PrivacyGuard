using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.DTOs
{
    public class CreateTemplateDto
    {
        [Required] public string Code { get; set; } = string.Empty;
        [Required] public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Purposes { get; set; } = new List<string>();
        public string LogoUrl { get; set; } = string.Empty;
        public string PrimaryColor { get; set; } = "#0F62FE";
    }

    public class UpdateTemplateDto
    {
        [Required] public string Code { get; set; } = string.Empty;
        [Required] public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Purposes { get; set; } = new List<string>();
        public string LogoUrl { get; set; } = string.Empty;
        public string PrimaryColor { get; set; } = "#0F62FE";
    }

    public class TemplateListDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Purposes { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
    }

    public class PurposeCheckboxItemDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}