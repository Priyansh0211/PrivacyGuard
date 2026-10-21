using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.Interfaces.Services;
using System.Security.Claims;

namespace PrivacyGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public DashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }


        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var orgIdStr = User.FindFirstValue("OrganizationId");
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (string.IsNullOrEmpty(orgIdStr))
                    return Unauthorized(new { message = "Invalid token: OrganizationId missing." });

               
               if (userRole != "Admin")
               {
                   return Forbid("Only Admins can access the dashboard.");
               }
               

               var organizationId = Guid.Parse(orgIdStr);

                var summary = await _adminDashboardService.GetDashboardSummaryAsync(organizationId);

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
