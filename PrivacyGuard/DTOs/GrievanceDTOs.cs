using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.DTOs
{
    public class CreateGrievanceDto
    {
        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = "Other";
    }

    public class AddGrievanceMessageDto
    {
        [Required]
        public string MessageText { get; set; } = string.Empty;
        public bool IsInternal { get; set; } = false;
    }

    public class GrievanceMessageDto
    {
        public string MessageText { get; set; } = string.Empty;
        public bool IsInternal { get; set; }
        public string SenderType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class GrievanceDetailDto
    {
        public Guid Id { get; set; }
        public string ReferenceId { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string RaisedByReferenceId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<GrievanceMessageDto> Messages { get; set; } = new List<GrievanceMessageDto>();
    }

    public class EscalateToBreachDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string BreachDescription { get; set; } = string.Empty;
        public string Severity { get; set; } = "High";
    }
}