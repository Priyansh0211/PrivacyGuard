using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using PrivacyGuard.Models;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace PrivacyGuard.Services
{
    public class ReportGeneratorService : IReportGeneratorService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IWebHostEnvironment _env;

        public ReportGeneratorService(IReportRepository reportRepository, IWebHostEnvironment env)
        {
            _reportRepository = reportRepository;
            _env = env;
        }


        public async Task<ReportJob> InitializeReportJobAsync(Guid orgId, Guid userId, string reportType)
        {
            var job = new ReportJob
            {
                OrganizationId = orgId,
                ReportType = reportType,
                RequestedBy = userId,
                ReportFormat = "CSV",
                Status = "Queued"
            };
            return await _reportRepository.CreateJobAsync(job);
        }

        // CSV Generate & Hash method
        public async Task<bool> GenerateAndSaveCsvEvidenceAsync<T>(Guid jobId, Guid organizationId, IEnumerable<T> reportData, string fileNamePrefix)
        {
            var job = await _reportRepository.GetJobByIdAsync(jobId);
            if (job == null) return false;

            try
            {
                job.Status = "Processing";
                job.CreatedAt = DateTime.UtcNow;
                await _reportRepository.UpdateJobAsync(job); 

                // CSV Content Generator
                var csvContent = new StringBuilder();
                var properties = typeof(T).GetProperties();
                csvContent.AppendLine(string.Join(",", Array.ConvertAll(properties, p => p.Name)));

                int recordCount = 0;
                foreach (var item in reportData)
                {
                    var values = Array.ConvertAll(properties, p =>
                    {
                        var val = p.GetValue(item)?.ToString() ?? "";
                        if (val.Contains(",")) val = $"\"{val}\"";
                        return val;
                    });
                    csvContent.AppendLine(string.Join(",", values));
                    recordCount++;
                }

                var webRootPath = _env.WebRootPath;
                if (string.IsNullOrEmpty(webRootPath))
                {
                    webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                // File Save Logic
                var folderPath = Path.Combine(webRootPath, "Reports");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = $"{fileNamePrefix}_{organizationId}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
                var filePath = Path.Combine(folderPath, fileName);
                await File.WriteAllTextAsync(filePath, csvContent.ToString());

                // SHA-256 Hash
                string checksum;
                using (var sha256 = SHA256.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        var hash = sha256.ComputeHash(stream);
                        checksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }

                // Evidence Object
                var generatedReport = new GeneratedReport
                {
                    JobId = jobId,
                    OrganizationId = organizationId,
                    FilePath = $"/Reports/{fileName}",
                    ChecksumSha256 = checksum,
                    RecordCount = recordCount,
                    FileSizeKb = new FileInfo(filePath).Length / 1024,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };

                await _reportRepository.SaveGeneratedReportAsync(generatedReport); 

                job.Status = "Completed";
                job.CompletedAt = DateTime.UtcNow;
                await _reportRepository.UpdateJobAsync(job); 

                return true;
            }
            catch (Exception ex)
            {
                job.Status = "Failed";
                job.ErrorMessage = ex.Message;
                job.CompletedAt = DateTime.UtcNow;
                await _reportRepository.UpdateJobAsync(job);
                return false;
            }
        }

        public async Task<bool> GenerateGrievanceReportAsync(Guid jobId, Guid organizationId) 
        {
            
            var grievanceData = await _reportRepository.GetGrievanceReportDataAsync(organizationId);
            return await GenerateAndSaveCsvEvidenceAsync(jobId, organizationId, grievanceData, "GrievanceReport");
        }

        public async Task<bool> GenerateGrievanceAuditReportAsync(Guid jobId, Guid organizationId)
        {
          
            var auditData = await _reportRepository.GetGrievanceAuditReportDataAsync(organizationId);

         
            return await GenerateAndSaveCsvEvidenceAsync(jobId, organizationId, auditData, "GrievanceAuditReport");
        }

        public async Task<bool> GenerateConsentAuditReportAsync(Guid jobId, Guid organizationId)
        {
            var data = await _reportRepository.GetConsentAuditReportDataAsync(organizationId);
            return await GenerateAndSaveCsvEvidenceAsync(jobId, organizationId, data, "ConsentAuditReport");
        }

        public async Task<bool> GenerateDataBreachReportAsync(Guid jobId, Guid organizationId)
        {
            var data = await _reportRepository.GetDataBreachReportDataAsync(organizationId);
            return await GenerateAndSaveCsvEvidenceAsync(jobId, organizationId, data, "DataBreachReport");
        }

        public async Task<bool> GenerateDpiaReportAsync(Guid jobId, Guid organizationId)
        {
            var data = await _reportRepository.GetDpiaReportDataAsync(organizationId);
            return await GenerateAndSaveCsvEvidenceAsync(jobId, organizationId, data, "DpiaReport");
        }

        public async Task<bool> GenerateDsarReportAsync(Guid jobId, Guid organizationId)
        {
            var data = await _reportRepository.GetDsarReportDataAsync(organizationId);
            return await GenerateAndSaveCsvEvidenceAsync(jobId, organizationId, data, "DsarReport");
        }

        public async Task<bool> GenerateDataInventoryReportAsync(Guid jobId, Guid organizationId)
        {
            var data = await _reportRepository.GetDataInventoryReportDataAsync(organizationId);
            return await GenerateAndSaveCsvEvidenceAsync(jobId, organizationId, data, "DataInventoryReport");
        }

        public async Task<bool> GenerateDataProcessingReportAsync(Guid jobId, Guid organizationId)
        {
            var data = await _reportRepository.GetDataProcessingReportDataAsync(organizationId);
            return await GenerateAndSaveCsvEvidenceAsync(jobId, organizationId, data, "DataProcessingReport");
        }

        public async Task<bool> GenerateDataRetentionReportAsync(Guid jobId, Guid organizationId)
        {
            var data = await _reportRepository.GetDataRetentionReportDataAsync(organizationId);
            return await GenerateAndSaveCsvEvidenceAsync(jobId, organizationId, data, "DataRetentionReport");
        }

        public async Task<bool> GenerateDataSharingReportAsync(Guid jobId, Guid organizationId)
        {
            var data = await _reportRepository.GetDataSharingReportDataAsync(organizationId);
            return await GenerateAndSaveCsvEvidenceAsync(jobId, organizationId, data, "DataSharingReport");
        }

        public async Task<bool> GenerateSecurityAuditReportAsync(Guid jobId, Guid organizationId)
        {
            var data = await _reportRepository.GetSecurityAuditReportDataAsync(organizationId);
            return await GenerateAndSaveCsvEvidenceAsync(jobId, organizationId, data, "SecurityAuditReport");
        }

        public async Task<bool> GenerateComplianceGapReportAsync(Guid jobId, Guid organizationId)
        {
            var data = await _reportRepository.GetComplianceGapReportDataAsync(organizationId);
            return await GenerateAndSaveCsvEvidenceAsync(jobId, organizationId, data, "ComplianceGapReport");
        }

    }
}