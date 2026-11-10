using System;
using System.Threading.Tasks;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IReportRepository
    {
        Task<ReportJob> CreateJobAsync(ReportJob job);
        Task<ReportJob?> GetJobByIdAsync(Guid jobId);
        Task<ReportJob> UpdateJobAsync(ReportJob job);
        Task SaveGeneratedReportAsync(GeneratedReport report);
        Task<IEnumerable<ReportJob>> GetJobsByOrganizationAsync(Guid organizationId);
        Task<GeneratedReport?> GetReportByJobIdAsync(Guid jobId, Guid organizationId);
    }

}