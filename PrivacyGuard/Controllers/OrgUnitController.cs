using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Services;
using System.Security.Claims;

namespace PrivacyGuard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrgUnitController : ControllerBase
    {
        private readonly IOrgUnitService _service;

        public OrgUnitController(IOrgUnitService service)
        {
            _service = service;
        }

        // Helper method: JWT token se OrganizationId nikalne ke liye
        private bool TryGetOrganizationId(out Guid organizationId)
        {
            organizationId = Guid.Empty;
            var claim = User.FindFirst("OrganizationId");
            return claim != null && Guid.TryParse(claim.Value, out organizationId);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrgUnitDto dto)
        {
            if (!TryGetOrganizationId(out var orgId))
                return Forbid(); // Agar token mein OrgId nahi hai toh block kar do

            var result = await _service.CreateOrgUnitAsync(dto, orgId);
            return Ok(result);
        }

        [HttpGet("hierarchy")]
        public async Task<IActionResult> GetHierarchy()
        {
            if (!TryGetOrganizationId(out var orgId))
                return Forbid();

            var tree = await _service.GetHierarchyAsync(orgId);
            return Ok(tree);
        }

        [HttpPut("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            if (!TryGetOrganizationId(out var orgId))
                return Forbid();

            var success = await _service.DeactivateOrgUnitAsync(id, orgId);
            if (!success)
                return NotFound(new { Message = "Organization unit not found or access denied." });

            return Ok(new { Message = "Organization unit deactivated successfully." });
        }
    }
}