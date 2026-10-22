namespace PrivacyGuard.DTOs
{
    public class DpiaListDto
    {
        public Guid Id { get; set; }
        public string ReferenceId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int PurposesCount { get; set; }
        public string ResidualRisk { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class CreateDpiaDto
    {
        public string Title { get; set; } = string.Empty;
        public int PurposesCount { get; set; }
        public string ResidualRisk { get; set; } = string.Empty;
        public string AssessmentDetails { get; set; } = string.Empty;
    }

    public class ReviewDpiaDto
    {
        public string Action { get; set; } = string.Empty; // "Approve" ya "SendBack"
        public string? ReviewNotes { get; set; }
    }
}