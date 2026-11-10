using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrivacyGuard.Models;
using PrivacyGuard.DTOs; // DTO import kar liya

namespace PrivacyGuard.Interfaces.Services
{
    public interface IReportManagementService
    {
        Task<IEnumerable<ReportJob>> GetReportHistoryAsync(Guid organizationId);

        Task<ReportDownloadResultDto> GetReportForDownloadAsync(Guid jobId, Guid organizationId);
    }
}