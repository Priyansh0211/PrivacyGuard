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
        private readonly PdfReportRenderer _pdfRenderer;

        public ReportGeneratorService(IReportRepository reportRepository, IWebHostEnvironment env, PdfReportRenderer pdfRenderer)
        {
            _reportRepository = reportRepository;
            _env = env;
            _pdfRenderer = pdfRenderer;
        }


        public async Task<ReportJob> InitializeReportJobAsync(Guid orgId, Guid userId, string reportType, string format = "CSV")
        {
            var job = new ReportJob
            {
                OrganizationId = orgId,
                ReportType = reportType,
                RequestedBy = userId,
                ReportFormat = NormalizeFormat(format),
                Status = "Queued"
            };
            return await _reportRepository.CreateJobAsync(job);
        }

        public async Task<bool> GenerateAndSaveCsvEvidenceAsync<T>(Guid jobId, Guid organizationId, IEnumerable<T> reportData, string fileNamePrefix)
        {
            return await GenerateAndSaveEvidenceAsync(jobId, organizationId, reportData, fileNamePrefix, "CSV", fileNamePrefix);
        }

        public async Task<bool> GenerateAndSaveEvidenceAsync<T>(Guid jobId, Guid organizationId, IEnumerable<T> reportData, string fileNamePrefix, string format, string reportTitle)
        {
            var job = await _reportRepository.GetJobByIdAsync(jobId);
            if (job == null) return false;

            try
            {
                job.Status = "Processing";
                job.ReportFormat = NormalizeFormat(format);
                job.CreatedAt = DateTime.UtcNow;
                await _reportRepository.UpdateJobAsync(job); 

                var records = reportData.ToList();
                var recordCount = records.Count;

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

                var normalizedFormat = NormalizeFormat(format);
                var fileExtension = normalizedFormat == "PDF" ? "pdf" : "csv";
                var fileName = $"{fileNamePrefix}_{organizationId}_{DateTime.UtcNow:yyyyMMddHHmmss}.{fileExtension}";
                var filePath = Path.Combine(folderPath, fileName);
                if (normalizedFormat == "PDF")
                {
                    var pdfBytes = _pdfRenderer.Render(reportTitle, fileNamePrefix, records);
                    await File.WriteAllBytesAsync(filePath, pdfBytes);
                }
                else
                {
                    var csvContent = new StringBuilder();
                    var properties = typeof(T).GetProperties();
                    csvContent.AppendLine(string.Join(",", Array.ConvertAll(properties, p => p.Name)));
                    foreach (var item in records)
                    {
                        var values = Array.ConvertAll(properties, p =>
                        {
                            var val = p.GetValue(item)?.ToString() ?? "";
                            if (val.Contains(",")) val = $"\"{val}\"";
                            return val;
                        });
                        csvContent.AppendLine(string.Join(",", values));
                    }
                    await File.WriteAllTextAsync(filePath, csvContent.ToString());
                }

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

        public async Task<bool> GenerateGrievanceReportAsync(Guid jobId, Guid organizationId, string format = "CSV") 
        {
            
            var grievanceData = await _reportRepository.GetGrievanceReportDataAsync(organizationId);
            return await GenerateAndSaveEvidenceAsync(jobId, organizationId, grievanceData, "GrievanceReport", format, "Grievance Report");
        }

        public async Task<bool> GenerateGrievanceAuditReportAsync(Guid jobId, Guid organizationId, string format = "CSV")
        {
          
            var auditData = await _reportRepository.GetGrievanceAuditReportDataAsync(organizationId);

         
            return await GenerateAndSaveEvidenceAsync(jobId, organizationId, auditData, "GrievanceAuditReport", format, "Grievance Audit Report");
        }

        public async Task<bool> GenerateConsentAuditReportAsync(Guid jobId, Guid organizationId, string format = "CSV")
        {
            var data = await _reportRepository.GetConsentAuditReportDataAsync(organizationId);
            return await GenerateAndSaveEvidenceAsync(jobId, organizationId, data, "ConsentAuditReport", format, "Consent Audit Report");
        }

        public async Task<bool> GenerateDataBreachReportAsync(Guid jobId, Guid organizationId, string format = "CSV")
        {
            var data = await _reportRepository.GetDataBreachReportDataAsync(organizationId);
            return await GenerateAndSaveEvidenceAsync(jobId, organizationId, data, "DataBreachReport", format, "Data Breach Report");
        }

        public async Task<bool> GenerateDpiaReportAsync(Guid jobId, Guid organizationId, string format = "CSV")
        {
            var data = await _reportRepository.GetDpiaReportDataAsync(organizationId);
            return await GenerateAndSaveEvidenceAsync(jobId, organizationId, data, "DpiaReport", format, "DPIA Report");
        }

        public async Task<bool> GenerateDsarReportAsync(Guid jobId, Guid organizationId, string format = "CSV")
        {
            var data = await _reportRepository.GetDsarReportDataAsync(organizationId);
            return await GenerateAndSaveEvidenceAsync(jobId, organizationId, data, "DsarReport", format, "DSAR Report");
        }

        public async Task<bool> GenerateDataInventoryReportAsync(Guid jobId, Guid organizationId, string format = "CSV")
        {
            var data = await _reportRepository.GetDataInventoryReportDataAsync(organizationId);
            return await GenerateAndSaveEvidenceAsync(jobId, organizationId, data, "DataInventoryReport", format, "Data Inventory Report");
        }

        public async Task<bool> GenerateDataProcessingReportAsync(Guid jobId, Guid organizationId, string format = "CSV")
        {
            var data = await _reportRepository.GetDataProcessingReportDataAsync(organizationId);
            return await GenerateAndSaveEvidenceAsync(jobId, organizationId, data, "DataProcessingReport", format, "Data Processing Report");
        }

        public async Task<bool> GenerateDataRetentionReportAsync(Guid jobId, Guid organizationId, string format = "CSV")
        {
            var data = await _reportRepository.GetDataRetentionReportDataAsync(organizationId);
            return await GenerateAndSaveEvidenceAsync(jobId, organizationId, data, "DataRetentionReport", format, "Data Retention Report");
        }

        public async Task<bool> GenerateDataSharingReportAsync(Guid jobId, Guid organizationId, string format = "CSV")
        {
            var data = await _reportRepository.GetDataSharingReportDataAsync(organizationId);
            return await GenerateAndSaveEvidenceAsync(jobId, organizationId, data, "DataSharingReport", format, "Data Sharing Report");
        }

        public async Task<bool> GenerateSecurityAuditReportAsync(Guid jobId, Guid organizationId, string format = "CSV")
        {
            var data = await _reportRepository.GetSecurityAuditReportDataAsync(organizationId);
            return await GenerateAndSaveEvidenceAsync(jobId, organizationId, data, "SecurityAuditReport", format, "Security Audit Report");
        }

        public async Task<bool> GenerateComplianceGapReportAsync(Guid jobId, Guid organizationId, string format = "CSV")
        {
            var data = await _reportRepository.GetComplianceGapReportDataAsync(organizationId);
            return await GenerateAndSaveEvidenceAsync(jobId, organizationId, data, "ComplianceGapReport", format, "Compliance Gap Report");
        }

        private static string NormalizeFormat(string format)
        {
            return string.Equals(format, "PDF", StringComparison.OrdinalIgnoreCase) ? "PDF" : "CSV";
        }
    }
}