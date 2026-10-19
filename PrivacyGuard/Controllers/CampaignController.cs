using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces;
using PrivacyGuard.Interfaces.Services;
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
        public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignDto dto)
        {
            try
            {
                // Token se OrganizationId nikalo
                var orgIdStr = User.FindFirstValue("OrganizationId");
                if (string.IsNullOrEmpty(orgIdStr))
                    return Unauthorized(new { message = "Invalid token: OrganizationId missing." });

                var organizationId = Guid.Parse(orgIdStr);

                // Service call karo
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
                // Token se User ki saari details nikalo
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var orgIdStr = User.FindFirstValue("OrganizationId");
                var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "NormalUser";

                if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(orgIdStr))
                    return Unauthorized(new { message = "Invalid token details." });

                var userId = Guid.Parse(userIdStr);
                var organizationId = Guid.Parse(orgIdStr);

                // Service se data mangwao
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


        [HttpGet("all")]
        public async Task<IActionResult> GetAllCampaigns()
        {
            try
            {
                var orgIdStr = User.FindFirstValue("OrganizationId");
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (string.IsNullOrEmpty(orgIdStr))
                    return Unauthorized(new { message = "Invalid token." });

                // Optional: Tum chaho toh yahan check laga sakte ho ki sirf Admin hi dekh paye
                if (userRole != "Admin")
                {
                    return Forbid("Only Admins can view all campaigns.");
                }

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
        public async Task<IActionResult> GetCampaignStats(Guid id)
        {
            try
            {
                var orgIdStr = User.FindFirstValue("OrganizationId");
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (string.IsNullOrEmpty(orgIdStr))
                    return Unauthorized(new { message = "Invalid token." });

                if (userRole != "Admin")
                    return Forbid("Only Admins can view campaign statistics.");

                var organizationId = Guid.Parse(orgIdStr);

                var stats = await _campaignService.GetCampaignStatsAsync(id, organizationId);

                // Percentage nikalna bahut aasan hai frontend par, par hum backend se bhi bhej sakte hain
                var response = new
                {
                    stats.CampaignId,
                    stats.Title,
                    stats.TotalResponses,
                    stats.AcceptedCount,
                    stats.RejectedCount,
                    stats.CompletedCount,
                    // Percentage logic:
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