public class DataBreachExportDto
{
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int EstimatedAffectedRecords { get; set; }
    public DateTime IncidentDate { get; set; }
    public DateTime DiscoveryDate { get; set; }
    public DateTime CreatedAt { get; set; }
}