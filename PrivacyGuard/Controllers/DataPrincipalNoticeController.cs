using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.Interfaces.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PrivacyGuard.Controllers
{
    [Route("api/dataprincipal/notice")]
    [ApiController]
    [Authorize]
    public class DataPrincipalNoticeController : ControllerBase
    {
        private readonly INoticeService _noticeService;

        public DataPrincipalNoticeController(INoticeService noticeService)
        {
            _noticeService = noticeService;
        }

        // Token se Organization ID nikalne ka helper
        private (bool isValid, Guid orgId) GetOrgIdClaim()
        {
            var orgIdClaim = User.Claims.FirstOrDefault(c => c.Type == "orgId" || c.Type == "OrganizationId");

            if (orgIdClaim != null && Guid.TryParse(orgIdClaim.Value, out Guid orgId))
            {
                return (true, orgId);
            }
            return (false, Guid.Empty);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllActiveNotices([FromQuery] string lang = "en")
        {
            var claim = GetOrgIdClaim();
            if (!claim.isValid)
                return Unauthorized(new { message = "Invalid or missing Organization Token." });

            var notices = await _noticeService.GetAllActiveNoticesAsync(claim.orgId, lang);

            if (notices == null || notices.Count == 0)
                return NotFound(new { message = "No active notices found." });

            return Ok(notices);
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetActiveNotice(string code, [FromQuery] string lang = "en")
        {
            var claim = GetOrgIdClaim();
            if (!claim.isValid)
                return Unauthorized(new { message = "Invalid or missing Organization Token." });

            var result = await _noticeService.GetActiveNoticeForPrincipalAsync(code, lang, claim.orgId);

            if (result == null)
                return NotFound(new { message = "Active privacy notice not found." });

            return Ok(result);
        }
    }
}