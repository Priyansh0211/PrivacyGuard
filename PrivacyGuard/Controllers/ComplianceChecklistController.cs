using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.Filters;
using PrivacyGuard.Interfaces.Services;

namespace PrivacyGuard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [HasPermission("Audit.View")]
    public class ComplianceChecklistController : ControllerBase
    {
        private readonly IComplianceChecklistService _complianceChecklistService;

        public ComplianceChecklistController(IComplianceChecklistService complianceChecklistService)
        {
            _complianceChecklistService = complianceChecklistService;
        }

        [HttpGet]
        public async Task<IActionResult> GetChecklist()
        {
            var organizationIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (!Guid.TryParse(organizationIdClaim, out var organizationId))
                return Unauthorized(new { message = "Organization context missing." });

            var checklist = await _complianceChecklistService.GetChecklistAsync(organizationId);
            return Ok(checklist);
        }
    }
}
