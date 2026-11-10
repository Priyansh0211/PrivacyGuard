public class DpiaReportExportDto
{
    public string ReferenceId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int PurposesCount { get; set; }
    public string ResidualRisk { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}