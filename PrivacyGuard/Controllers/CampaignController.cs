using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Filters; // 🚨 Naya Custom Filter Import Kiya
using System.Security.Claims;

namespace PrivacyGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Is poore controller ko hit karne ke liye Token zaroori hai
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignService _campaignService;

        public CampaignController(ICampaignService campaignService)
        {
            _campaignService = campaignService;
        }

        // ==========================================
        // 1. CREATE CAMPAIGN (Usually for Admins)
        // ==========================================
        [HttpPost("create")]
        [HasPermission("Campaigns.Create")] // 🚨 Permission Check Added
        public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignDto dto)
        {
            try
            {
                var orgIdStr = User.FindFirstValue("OrganizationId");
                if (string.IsNullOrEmpty(orgIdStr))
                    return Unauthorized(new { message = "Invalid token: OrganizationId missing." });

                var organizationId = Guid.Parse(orgIdStr);

                var campaign = await _campaignService.CreateCampaignAsync(dto, organizationId);

                return Ok(new { message = "Campaign created successfully.", campaignId = campaign.CampaignId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ==========================================
        // 2. GET MY PENDING CAMPAIGNS (For Logged-in User)
        // ==========================================
        [HttpGet("my-pending")]

        public async Task<IActionResult> GetMyPendingCampaigns()
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var orgIdStr = User.FindFirstValue("OrganizationId");
                var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "NormalUser";

                if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(orgIdStr))
                    return Unauthorized(new { message = "Invalid token details." });

                var userId = Guid.Parse(userIdStr);
                var organizationId = Guid.Parse(orgIdStr);

                var campaigns = await _campaignService.GetMyActiveCampaignsAsync(organizationId, userId, userRole);

                return Ok(campaigns);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ==========================================
        // 3. SUBMIT RESPONSE (Allow/Deny/Mark as Read)
        // ==========================================
        [HttpPost("respond")]
      
        public async Task<IActionResult> SubmitResponse([FromBody] SubmitCampaignResponseDto dto)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var orgIdStr = User.FindFirstValue("OrganizationId");

                if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(orgIdStr))
                    return Unauthorized(new { message = "Invalid token." });

                var userId = Guid.Parse(userIdStr);
                var organizationId = Guid.Parse(orgIdStr);

                var response = await _campaignService.SubmitCampaignResponseAsync(dto, userId, organizationId);

                return Ok(new { message = "Response recorded successfully.", responseStatus = response.ResponseStatus });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ==========================================
        // 4. GET ALL CAMPAIGNS (For Admin)
        // ==========================================
        [HttpGet("all")]
        [HasPermission("Campaigns.View")]
        public async Task<IActionResult> GetAllCampaigns()
        {
            try
            {
                var orgIdStr = User.FindFirstValue("OrganizationId");

                if (string.IsNullOrEmpty(orgIdStr))
                    return Unauthorized(new { message = "Invalid token." });

                var organizationId = Guid.Parse(orgIdStr);
                var campaigns = await _campaignService.GetAllCampaignsAsync(organizationId);

                return Ok(campaigns);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ==========================================
        // 5. GET CAMPAIGN STATS (For Admin Analytics)
        // ==========================================
        [HttpGet("{id}/stats")]
        [HasPermission("Campaigns.View")]
        public async Task<IActionResult> GetCampaignStats(Guid id)
        {
            try
            {
                var orgIdStr = User.FindFirstValue("OrganizationId");

                if (string.IsNullOrEmpty(orgIdStr))
                    return Unauthorized(new { message = "Invalid token." });

                var organizationId = Guid.Parse(orgIdStr);

                var stats = await _campaignService.GetCampaignStatsAsync(id, organizationId);

                var response = new
                {
                    stats.CampaignId,
                    stats.Title,
                    stats.TotalResponses,
                    stats.AcceptedCount,
                    stats.RejectedCount,
                    stats.CompletedCount,
                    AcceptanceRate = stats.TotalResponses == 0 ? 0 : Math.Round((double)stats.AcceptedCount / stats.TotalResponses * 100, 2)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}