using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.Filters;
using PrivacyGuard.Interfaces.Services;

namespace PrivacyGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet("logs")]
        [HasPermission("Audit.View")]
        public async Task<IActionResult> GetAuditLogs()
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;

            if (string.IsNullOrEmpty(orgIdClaim) || !Guid.TryParse(orgIdClaim, out Guid organizationId))
            {
                return Unauthorized(new { Error = "Invalid token or organization data." });
            }

            var logs = await _auditService.GetOrganizationLogsAsync(organizationId);
            return Ok(logs);
        }
    }
}
