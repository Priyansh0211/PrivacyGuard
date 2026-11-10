using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using PrivacyGuard.Models;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;

namespace PrivacyGuard.Services
{
    public class ReportManagementService : IReportManagementService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IWebHostEnvironment _env; 
        public ReportManagementService(IReportRepository reportRepository, IWebHostEnvironment env)
        {
            _reportRepository = reportRepository;
            _env = env;
        }

        public async Task<IEnumerable<ReportJob>> GetReportHistoryAsync(Guid organizationId)
        {
            return await _reportRepository.GetJobsByOrganizationAsync(organizationId);
        }

        public async Task<ReportDownloadResultDto> GetReportForDownloadAsync(Guid jobId, Guid organizationId)
        {
            var report = await _reportRepository.GetReportByJobIdAsync(jobId, organizationId);

            if (report == null)
            {
                return new ReportDownloadResultDto { IsSuccess = false, ErrorMessage = "Report not found or not ready yet." };
            }

            if (report.ExpiresAt < DateTime.UtcNow)
            {
                return new ReportDownloadResultDto { IsSuccess = false, ErrorMessage = "Download link has expired. Please generate a new report." };
            }

            // Path resolve karo
            var webRootPath = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var cleanRelativePath = report.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var filePath = Path.Combine(webRootPath, cleanRelativePath);

            // 🚨 DEBUG: Agar file na mile, toh error mein exact path return karwa do
            if (!File.Exists(filePath))
            {
                return new ReportDownloadResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"File not found! Looking at path: {filePath}"
                };
            }

            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);

            return new ReportDownloadResultDto
            {
                IsSuccess = true,
                FileBytes = fileBytes,
                FileName = fileName
            };
        }
    }
}