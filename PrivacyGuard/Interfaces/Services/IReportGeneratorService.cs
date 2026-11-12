using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IReportGeneratorService
    {
        Task<ReportJob> InitializeReportJobAsync(Guid orgId, Guid userId, string reportType);
        Task<bool> GenerateAndSaveCsvEvidenceAsync<T>(Guid jobId, Guid organizationId, IEnumerable<T> reportData, string fileNamePrefix);

        Task<bool> GenerateGrievanceReportAsync(Guid jobId, Guid organizationId);

        Task<bool> GenerateGrievanceAuditReportAsync([FromBody] Guid organizationId, Guid organizationId1);

        Task<bool> GenerateConsentAuditReportAsync(Guid jobId, Guid organizationId);
        Task<bool> GenerateDataBreachReportAsync(Guid jobId, Guid organizationId);
        Task<bool> GenerateDpiaReportAsync(Guid jobId, Guid organizationId);
        Task<bool> GenerateDsarReportAsync(Guid jobId, Guid organizationId);
        Task<bool> GenerateDataInventoryReportAsync(Guid jobId, Guid organizationId);
        Task<bool> GenerateDataProcessingReportAsync(Guid jobId, Guid organizationId);
        Task<bool> GenerateDataRetentionReportAsync(Guid jobId, Guid organizationId);
        Task<bool> GenerateDataSharingReportAsync(Guid jobId, Guid organizationId);
        Task<bool> GenerateSecurityAuditReportAsync(Guid jobId, Guid organizationId);
        Task<bool> GenerateComplianceGapReportAsync(Guid jobId, Guid organizationId);

    }
}
