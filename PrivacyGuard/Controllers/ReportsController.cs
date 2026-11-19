using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Filters; // FIX: Permission attribute access karne ke liye
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PrivacyGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly ICampaignService _campaignService;
        private readonly IReportGeneratorService _reportGenerator;
        private readonly IReportManagementService _reportManagementService;

        public ReportsController(ICampaignService campaignService, IReportGeneratorService reportGenerator, IReportManagementService reportManagementService)
        {
            _campaignService = campaignService;
            _reportGenerator = reportGenerator;
            _reportManagementService = reportManagementService;
        }

        [HttpPost("generate-campaign-report/{campaignId}")]
        [HasPermission("Audit.Manage")] // UPDATE: Manage permission to generate reports
        public async Task<IActionResult> GenerateCampaignReport(Guid campaignId, [FromQuery] string format = "PDF")
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(orgIdStr) || string.IsNullOrEmpty(userIdStr))
                return Unauthorized(new { message = "Invalid token." });

            var orgId = Guid.Parse(orgIdStr);
            var userId = Guid.Parse(userIdStr);

            var job = await _reportGenerator.InitializeReportJobAsync(orgId, userId, "CampaignAudit", format);
            bool success;
            if (string.Equals(format, "PDF", StringComparison.OrdinalIgnoreCase))
            {
                var auditReport = await _campaignService.GetCampaignAuditReportAsync(campaignId, orgId);
                success = await _reportGenerator.GenerateAndSaveEvidenceAsync(
                    job.JobId, orgId, new[] { auditReport }, "CampaignAudit", format, "Campaign Audit Report");
            }
            else
            {
                var reportData = await _campaignService.GetCampaignStatsAsync(campaignId, orgId);
                success = await _reportGenerator.GenerateAndSaveEvidenceAsync(
                    job.JobId, orgId, new[] { reportData }, "CampaignAudit", format, "Campaign Audit Report");
            }

            if (!success)
                return StatusCode(500, new { message = "Report generation failed. Check job logs." });

            return Ok(new { message = "Report generated successfully.", jobId = job.JobId });
        }

        [HttpGet("history")]
        [HasPermission("Audit.View")] // UPDATE: View permission to see generated reports
        public async Task<IActionResult> GetReportHistory()
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr)) return Unauthorized();

            var orgId = Guid.Parse(orgIdStr);
            var jobs = await _reportManagementService.GetReportHistoryAsync(orgId);

            return Ok(jobs);
        }

        [HttpGet("download/{jobId}")]
        [HasPermission("Audit.View")] // UPDATE: View permission to download files
        public async Task<IActionResult> DownloadReport(Guid jobId)
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr)) return Unauthorized();

            var orgId = Guid.Parse(orgIdStr);
            var result = await _reportManagementService.GetReportForDownloadAsync(jobId, orgId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            var contentType = string.Equals(Path.GetExtension(result.FileName), ".pdf", StringComparison.OrdinalIgnoreCase)
                ? "application/pdf"
                : "text/csv";
            return File(result.FileBytes!, contentType, result.FileName!);
        }

        [HttpPost("generate-grievance-report")]
        [HasPermission("Audit.Manage")]
        public async Task<IActionResult> GenerateGrievanceReport([FromQuery] string format = "PDF")
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr))
                return Unauthorized(new { message = "Invalid token or Organization ID missing." });

            var organizationId = Guid.Parse(orgIdStr);

            var jobId = await _reportManagementService.CreateReportJobAsync(organizationId, "GrievanceReport", format);
            var success = await _reportGenerator.GenerateGrievanceReportAsync(jobId, organizationId, format);

            if (!success)
                return BadRequest(new { message = "Failed to generate Grievance Report." });

            return Ok(new { message = "Grievance Report generation started successfully.", jobId });
        }

        [HttpPost("generate-grievance-audit")]
        [HasPermission("Audit.Manage")]
        public async Task<IActionResult> GenerateGrievanceAuditReport([FromQuery] string format = "PDF")
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr))
                return Unauthorized(new { message = "Invalid token or Organization ID missing." });

            var organizationId = Guid.Parse(orgIdStr);

            var jobId = await _reportManagementService.CreateReportJobAsync(organizationId, "GrievanceAuditReport", format);
            var success = await _reportGenerator.GenerateGrievanceAuditReportAsync(jobId, organizationId, format);

            if (!success)
                return BadRequest(new { message = "Failed to generate Grievance Audit Report." });

            return Ok(new { message = "Grievance Audit Report generation started successfully.", jobId });
        }

        [HttpPost("generate-consent-audit")]
        [HasPermission("Audit.Manage")]
        public async Task<IActionResult> GenerateConsentAuditReport([FromQuery] string format = "PDF")
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr))
                return Unauthorized(new { message = "Invalid token or Organization ID missing." });

            var organizationId = Guid.Parse(orgIdStr);

            var jobId = await _reportManagementService.CreateReportJobAsync(organizationId, "ConsentAuditReport", format);
            var success = await _reportGenerator.GenerateConsentAuditReportAsync(jobId, organizationId, format);

            if (!success)
                return BadRequest(new { message = "Failed to generate Consent Audit Report." });

            return Ok(new { message = "Consent Audit Report generation started successfully.", jobId });
        }

        [HttpPost("generate-data-breach")]
        [HasPermission("Audit.Manage")]
        public async Task<IActionResult> GenerateDataBreachReport([FromQuery] string format = "PDF")
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr))
                return Unauthorized(new { message = "Invalid token or Organization ID missing." });

            var organizationId = Guid.Parse(orgIdStr);

            var jobId = await _reportManagementService.CreateReportJobAsync(organizationId, "DataBreachReport", format);
            var success = await _reportGenerator.GenerateDataBreachReportAsync(jobId, organizationId, format);

            if (!success)
                return BadRequest(new { message = "Failed to generate Data Breach Report." });

            return Ok(new { message = "Data Breach Report generation started successfully.", jobId });
        }

        [HttpPost("generate-dpia")]
        [HasPermission("Audit.Manage")]
        public async Task<IActionResult> GenerateDpiaReport([FromQuery] string format = "PDF")
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr))
                return Unauthorized(new { message = "Invalid token or Organization ID missing." });

            var organizationId = Guid.Parse(orgIdStr);

            var jobId = await _reportManagementService.CreateReportJobAsync(organizationId, "DpiaReport", format);
            var success = await _reportGenerator.GenerateDpiaReportAsync(jobId, organizationId, format);

            if (!success)
                return BadRequest(new { message = "Failed to generate DPIA Report." });

            return Ok(new { message = "DPIA Report generation started successfully.", jobId });
        }

        [HttpPost("generate-dsar-report")]
        [HasPermission("Audit.Manage")]
        public Task<IActionResult> GenerateDsarReport([FromQuery] string format = "PDF") => GenerateOrganizationReportAsync(
            "DsarReport", "DSAR", format, _reportGenerator.GenerateDsarReportAsync);

        [HttpPost("generate-data-inventory-report")]
        [HasPermission("Audit.Manage")]
        public Task<IActionResult> GenerateDataInventoryReport([FromQuery] string format = "PDF") => GenerateOrganizationReportAsync(
            "DataInventoryReport", "data inventory", format, _reportGenerator.GenerateDataInventoryReportAsync);

        [HttpPost("generate-data-processing-report")]
        [HasPermission("Audit.Manage")]
        public Task<IActionResult> GenerateDataProcessingReport([FromQuery] string format = "PDF") => GenerateOrganizationReportAsync(
            "DataProcessingReport", "data processing", format, _reportGenerator.GenerateDataProcessingReportAsync);

        [HttpPost("generate-data-retention-report")]
        [HasPermission("Audit.Manage")]
        public Task<IActionResult> GenerateDataRetentionReport([FromQuery] string format = "PDF") => GenerateOrganizationReportAsync(
            "DataRetentionReport", "data retention", format, _reportGenerator.GenerateDataRetentionReportAsync);

        [HttpPost("generate-data-sharing-report")]
        [HasPermission("Audit.Manage")]
        public Task<IActionResult> GenerateDataSharingReport([FromQuery] string format = "PDF") => GenerateOrganizationReportAsync(
            "DataSharingReport", "data sharing", format, _reportGenerator.GenerateDataSharingReportAsync);

        [HttpPost("generate-security-audit-report")]
        [HasPermission("Audit.Manage")]
        public Task<IActionResult> GenerateSecurityAuditReport([FromQuery] string format = "PDF") => GenerateOrganizationReportAsync(
            "SecurityAuditReport", "security audit", format, _reportGenerator.GenerateSecurityAuditReportAsync);

        [HttpPost("generate-compliance-gap-report")]
        [HasPermission("Audit.Manage")]
        public Task<IActionResult> GenerateComplianceGapReport([FromQuery] string format = "PDF") => GenerateOrganizationReportAsync(
            "ComplianceGapReport", "compliance gap", format, _reportGenerator.GenerateComplianceGapReportAsync);

        private async Task<IActionResult> GenerateOrganizationReportAsync(
            string reportType,
            string reportName,
            string format,
            Func<Guid, Guid, string, Task<bool>> generateReport)
        {
            var organizationIdClaim = User.FindFirstValue("OrganizationId");
            if (!Guid.TryParse(organizationIdClaim, out var organizationId))
                return Unauthorized(new { message = "Invalid token or Organization ID missing." });

            var jobId = await _reportManagementService.CreateReportJobAsync(organizationId, reportType, format);
            var success = await generateReport(jobId, organizationId, format);

            if (!success)
                return BadRequest(new { message = $"Failed to generate {reportName} report." });

            return Ok(new { message = $"{reportName} report generated successfully.", jobId });
        }
    }
}