using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        Task<IEnumerable<GrievanceReportExportDto>> GetGrievanceReportDataAsync(Guid organizationId);
        Task<IEnumerable<GrievanceAuditExportDto>> GetGrievanceAuditReportDataAsync(Guid organizationId);
        Task<IEnumerable<ConsentAuditExportDto>> GetConsentAuditReportDataAsync(Guid organizationId);
        Task<IEnumerable<DataBreachExportDto>> GetDataBreachReportDataAsync(Guid organizationId);
        Task<IEnumerable<DpiaReportExportDto>> GetDpiaReportDataAsync(Guid organizationId);
        Task<IEnumerable<DsarReportExportDto>> GetDsarReportDataAsync(Guid organizationId);
        Task<IEnumerable<DataInventoryReportExportDto>> GetDataInventoryReportDataAsync(Guid organizationId);
        Task<IEnumerable<DataProcessingReportExportDto>> GetDataProcessingReportDataAsync(Guid organizationId);
        Task<IEnumerable<DataRetentionReportExportDto>> GetDataRetentionReportDataAsync(Guid organizationId);
        Task<IEnumerable<DataSharingReportExportDto>> GetDataSharingReportDataAsync(Guid organizationId);
        Task<IEnumerable<SecurityAuditReportExportDto>> GetSecurityAuditReportDataAsync(Guid organizationId);
        Task<IEnumerable<ComplianceGapReportExportDto>> GetComplianceGapReportDataAsync(Guid organizationId);


    }

}