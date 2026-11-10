using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IReportGeneratorService
    {
        Task<ReportJob> InitializeReportJobAsync(Guid orgId, Guid userId, string reportType);
        Task<bool> GenerateAndSaveCsvEvidenceAsync<T>(Guid jobId, Guid organizationId, IEnumerable<T> reportData, string fileNamePrefix);
    }
}
