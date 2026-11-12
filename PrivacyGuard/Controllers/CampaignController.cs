using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Filters;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PrivacyGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignService _campaignService;
        public CampaignController(ICampaignService campaignService) { _campaignService = campaignService; }

        private bool IsInternalUser() => (User.FindFirstValue(ClaimTypes.Role) ?? "") != "DataPrincipal";

        [HttpPost("create")]
        [HasPermission("Campaigns.Create")]
        public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignDto dto)
        {
            try
            {
                var orgIdStr = User.FindFirstValue("OrganizationId");
                if (string.IsNullOrEmpty(orgIdStr)) return Unauthorized();
                var campaign = await _campaignService.CreateCampaignAsync(dto, Guid.Parse(orgIdStr));
                return Ok(new { message = "Campaign created successfully.", campaignId = campaign.CampaignId });
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpGet("my-pending")]
        public async Task<IActionResult> GetMyPendingCampaigns()
        {
            try
            {
                var actorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var orgIdStr = User.FindFirstValue("OrganizationId");
                var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "NormalUser";
                if (string.IsNullOrEmpty(actorIdStr) || string.IsNullOrEmpty(orgIdStr)) return Unauthorized();

                var campaigns = await _campaignService.GetMyActiveCampaignsAsync(Guid.Parse(orgIdStr), Guid.Parse(actorIdStr), userRole, IsInternalUser());
                        return Ok(campaigns);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    rootError = ex.InnerException?.InnerException?.Message
                });
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpPost("respond")]
        public async Task<IActionResult> SubmitResponse([FromBody] SubmitCampaignResponseDto dto)
        {
            try
            {
                var actorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var orgIdStr = User.FindFirstValue("OrganizationId");
                if (string.IsNullOrEmpty(actorIdStr) || string.IsNullOrEmpty(orgIdStr)) return Unauthorized();

                var response = await _campaignService.SubmitCampaignResponseAsync(dto, Guid.Parse(actorIdStr), Guid.Parse(orgIdStr), IsInternalUser());
                return Ok(new { message = "Response recorded successfully.", responseStatus = response.ResponseStatus });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    rootError = ex.InnerException?.InnerException?.Message
                });
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpGet("audiences")]
        [HasPermission("Campaigns.Create")]
        public async Task<IActionResult> GetAudienceList([FromQuery] string type = "customer")
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr)) return Unauthorized();
            var audiences = await _campaignService.GetAudienceListAsync(Guid.Parse(orgIdStr), type);
            return Ok(audiences);
        }

        [HttpPost("link-audience")]
        [HasPermission("Campaigns.Create")]
        public async Task<IActionResult> LinkAudience([FromBody] LinkAudienceRequestDto request)
        {
            var success = await _campaignService.AssignAudienceAsync(request);
            if (!success) return BadRequest(new { message = "No valid Audience selected." });
            return Ok(new { message = "Audience successfully linked to the campaign!" });
        }

        [HttpGet("all")]
        [HasPermission("Campaigns.View")]
        public async Task<IActionResult> GetAllCampaigns()
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr)) return Unauthorized();
            return Ok(await _campaignService.GetAllCampaignsAsync(Guid.Parse(orgIdStr)));
        }

        [HttpGet("{id}/stats")]
        [HasPermission("Campaigns.View")]
        public async Task<IActionResult> GetCampaignStats(Guid id)
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr)) return Unauthorized();
            var stats = await _campaignService.GetCampaignStatsAsync(id, Guid.Parse(orgIdStr));
            return Ok(stats);
        }
    }
}