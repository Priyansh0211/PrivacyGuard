using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IReportGeneratorService
    {
        Task<ReportJob> InitializeReportJobAsync(Guid orgId, Guid userId, string reportType, string format = "CSV");
        Task<bool> GenerateAndSaveCsvEvidenceAsync<T>(Guid jobId, Guid organizationId, IEnumerable<T> reportData, string fileNamePrefix);
        Task<bool> GenerateAndSaveEvidenceAsync<T>(Guid jobId, Guid organizationId, IEnumerable<T> reportData, string fileNamePrefix, string format, string reportTitle);

        Task<bool> GenerateGrievanceReportAsync(Guid jobId, Guid organizationId, string format = "CSV");

        Task<bool> GenerateGrievanceAuditReportAsync(Guid jobId, Guid organizationId, string format = "CSV");

        Task<bool> GenerateConsentAuditReportAsync(Guid jobId, Guid organizationId, string format = "CSV");
        Task<bool> GenerateDataBreachReportAsync(Guid jobId, Guid organizationId, string format = "CSV");
        Task<bool> GenerateDpiaReportAsync(Guid jobId, Guid organizationId, string format = "CSV");
        Task<bool> GenerateDsarReportAsync(Guid jobId, Guid organizationId, string format = "CSV");
        Task<bool> GenerateDataInventoryReportAsync(Guid jobId, Guid organizationId, string format = "CSV");
        Task<bool> GenerateDataProcessingReportAsync(Guid jobId, Guid organizationId, string format = "CSV");
        Task<bool> GenerateDataRetentionReportAsync(Guid jobId, Guid organizationId, string format = "CSV");
        Task<bool> GenerateDataSharingReportAsync(Guid jobId, Guid organizationId, string format = "CSV");
        Task<bool> GenerateSecurityAuditReportAsync(Guid jobId, Guid organizationId, string format = "CSV");
        Task<bool> GenerateComplianceGapReportAsync(Guid jobId, Guid organizationId, string format = "CSV");

    }
}
