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

        // Job create karne ka naya method (Controller directly DB me job na banaye isliye)
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
    }
}