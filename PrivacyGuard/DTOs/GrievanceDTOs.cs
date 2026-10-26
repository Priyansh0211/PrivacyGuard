using System;
using System.Collections.Generic;

namespace PrivacyGuard.DTOs
{
    public class CreateGrievanceDto
    {
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "Other";
    }

    public class AddGrievanceMessageDto
    {
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
        public string Title { get; set; } = string.Empty;
        public string BreachDescription { get; set; } = string.Empty;
        public string Severity { get; set; } = "High";
    }
}