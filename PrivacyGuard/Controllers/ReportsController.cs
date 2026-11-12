using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
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



        public ReportsController(ICampaignService campaignService, IReportGeneratorService reportGenerator , IReportManagementService reportManagementService)
        {
            _campaignService = campaignService;
            _reportGenerator = reportGenerator;
            _reportManagementService = reportManagementService;
        }

        [HttpPost("generate-campaign-report/{campaignId}")]
        public async Task<IActionResult> GenerateCampaignReport(Guid campaignId)
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(orgIdStr) || string.IsNullOrEmpty(userIdStr))
                return Unauthorized(new { message = "Invalid token." });

            var orgId = Guid.Parse(orgIdStr);
            var userId = Guid.Parse(userIdStr);

            // 1. Service ke through Job create karo (Clean approach)
            var job = await _reportGenerator.InitializeReportJobAsync(orgId, userId, "CampaignAudit");

            // 2. Apni purani Service se data nikalo
            var reportData = await _campaignService.GetCampaignStatsAsync(campaignId, orgId);
            var dataList = new List<CampaignStatsDto> { reportData };

            // 3. Generator Service ko data de do
            bool success = await _reportGenerator.GenerateAndSaveCsvEvidenceAsync(
                job.JobId,
                orgId,
                dataList,
                "CampaignAudit"
            );

            if (!success)
                return StatusCode(500, new { message = "Report generation failed. Check job logs." });

            return Ok(new { message = "Report generated successfully.", jobId = job.JobId });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetReportHistory()
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr)) return Unauthorized();

            var orgId = Guid.Parse(orgIdStr);

            // Management service ke through history fetch karo
            var jobs = await _reportManagementService.GetReportHistoryAsync(orgId);

            return Ok(jobs);
        }


        [HttpGet("download/{jobId}")]
        public async Task<IActionResult> DownloadReport(Guid jobId)
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr)) return Unauthorized();

            var orgId = Guid.Parse(orgIdStr);

            // Management service ke through file bytes aur validation check karo
            var result = await _reportManagementService.GetReportForDownloadAsync(jobId, orgId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return File(result.FileBytes!, "text/csv", result.FileName!);
        }

        [HttpPost("generate-grievance-report")]
        public async Task<IActionResult> GenerateGrievanceReport()
        {
            
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr))
                return Unauthorized(new { message = "Invalid token or Organization ID missing." });

            var organizationId = Guid.Parse(orgIdStr);


            var jobId = await _reportManagementService.CreateReportJobAsync(organizationId, "GrievanceReport");
            var success = await _reportGenerator.GenerateGrievanceReportAsync(jobId, organizationId);

            if (!success)
                return BadRequest(new { message = "Failed to generate Grievance Report." });

            return Ok(new { message = "Grievance Report generation started successfully.", jobId });
        }

        [HttpPost("generate-grievance-audit")]
        public async Task<IActionResult> GenerateGrievanceAuditReport()
        {
           
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr))
                return Unauthorized(new { message = "Invalid token or Organization ID missing." });

            var organizationId = Guid.Parse(orgIdStr);

           
            var jobId = await _reportManagementService.CreateReportJobAsync(organizationId, "GrievanceAuditReport");
            var success = await _reportGenerator.GenerateGrievanceAuditReportAsync(jobId, organizationId);

            if (!success)
                return BadRequest(new { message = "Failed to generate Grievance Audit Report." });

            return Ok(new { message = "Grievance Audit Report generation started successfully.", jobId });
        }

        [HttpPost("generate-consent-audit")]
        public async Task<IActionResult> GenerateConsentAuditReport()
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr))
                return Unauthorized(new { message = "Invalid token or Organization ID missing." });

            var organizationId = Guid.Parse(orgIdStr);

            var jobId = await _reportManagementService.CreateReportJobAsync(organizationId, "ConsentAuditReport");
            var success = await _reportGenerator.GenerateConsentAuditReportAsync(jobId, organizationId);

            if (!success)
                return BadRequest(new { message = "Failed to generate Consent Audit Report." });

            return Ok(new { message = "Consent Audit Report generation started successfully.", jobId });
        }

        [HttpPost("generate-data-breach")]
        public async Task<IActionResult> GenerateDataBreachReport()
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr))
                return Unauthorized(new { message = "Invalid token or Organization ID missing." });

            var organizationId = Guid.Parse(orgIdStr);

            var jobId = await _reportManagementService.CreateReportJobAsync(organizationId, "DataBreachReport");
            var success = await _reportGenerator.GenerateDataBreachReportAsync(jobId, organizationId);

            if (!success)
                return BadRequest(new { message = "Failed to generate Data Breach Report." });

            return Ok(new { message = "Data Breach Report generation started successfully.", jobId });
        }

        [HttpPost("generate-dpia")]
        public async Task<IActionResult> GenerateDpiaReport()
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr))
                return Unauthorized(new { message = "Invalid token or Organization ID missing." });

            var organizationId = Guid.Parse(orgIdStr);

            var jobId = await _reportManagementService.CreateReportJobAsync(organizationId, "DpiaReport");
            var success = await _reportGenerator.GenerateDpiaReportAsync(jobId, organizationId);

            if (!success)
                return BadRequest(new { message = "Failed to generate DPIA Report." });

            return Ok(new { message = "DPIA Report generation started successfully.", jobId });
        }

        [HttpPost("generate-dsar-report")]
        public Task<IActionResult> GenerateDsarReport() => GenerateOrganizationReportAsync(
            "DsarReport", "DSAR", _reportGenerator.GenerateDsarReportAsync);

        [HttpPost("generate-data-inventory-report")]
        public Task<IActionResult> GenerateDataInventoryReport() => GenerateOrganizationReportAsync(
            "DataInventoryReport", "data inventory", _reportGenerator.GenerateDataInventoryReportAsync);

        [HttpPost("generate-data-processing-report")]
        public Task<IActionResult> GenerateDataProcessingReport() => GenerateOrganizationReportAsync(
            "DataProcessingReport", "data processing", _reportGenerator.GenerateDataProcessingReportAsync);

        [HttpPost("generate-data-retention-report")]
        public Task<IActionResult> GenerateDataRetentionReport() => GenerateOrganizationReportAsync(
            "DataRetentionReport", "data retention", _reportGenerator.GenerateDataRetentionReportAsync);

        [HttpPost("generate-data-sharing-report")]
        public Task<IActionResult> GenerateDataSharingReport() => GenerateOrganizationReportAsync(
            "DataSharingReport", "data sharing", _reportGenerator.GenerateDataSharingReportAsync);

        [HttpPost("generate-security-audit-report")]
        public Task<IActionResult> GenerateSecurityAuditReport() => GenerateOrganizationReportAsync(
            "SecurityAuditReport", "security audit", _reportGenerator.GenerateSecurityAuditReportAsync);

        [HttpPost("generate-compliance-gap-report")]
        public Task<IActionResult> GenerateComplianceGapReport() => GenerateOrganizationReportAsync(
            "ComplianceGapReport", "compliance gap", _reportGenerator.GenerateComplianceGapReportAsync);

        private async Task<IActionResult> GenerateOrganizationReportAsync(
            string reportType,
            string reportName,
            Func<Guid, Guid, Task<bool>> generateReport)
        {
            var organizationIdClaim = User.FindFirstValue("OrganizationId");
            if (!Guid.TryParse(organizationIdClaim, out var organizationId))
                return Unauthorized(new { message = "Invalid token or Organization ID missing." });

            var jobId = await _reportManagementService.CreateReportJobAsync(organizationId, reportType);
            var success = await generateReport(jobId, organizationId);

            if (!success)
                return BadRequest(new { message = $"Failed to generate {reportName} report." });

            return Ok(new { message = $"{reportName} report generated successfully.", jobId });
        }
    }
}