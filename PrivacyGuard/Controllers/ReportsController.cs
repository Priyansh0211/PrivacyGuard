using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.Interfaces.Services;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using PrivacyGuard.DTOs;

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
    }
}