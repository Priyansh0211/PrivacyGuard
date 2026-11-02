using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Services;
using System.Security.Claims;
using System;
using System.Threading.Tasks;

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

        private bool TryGetClaimData(out Guid organizationId, out Guid orgUnitId, out string role)
        {
            organizationId = Guid.Empty;
            orgUnitId = Guid.Empty;
            role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            var orgClaim = User.FindFirst("OrganizationId");
            var unitClaim = User.FindFirst("OrgUnitId");

            bool hasOrgId = orgClaim != null && Guid.TryParse(orgClaim.Value, out organizationId);
            if (unitClaim != null) Guid.TryParse(unitClaim.Value, out orgUnitId);

            return hasOrgId;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateOrgUnitDto dto)
        {
            if (!TryGetClaimData(out var orgId, out _, out _)) return Forbid();
            var result = await _service.CreateOrgUnitAsync(dto, orgId);
            return Ok(result);
        }

        [HttpGet("hierarchy")]
        public async Task<IActionResult> GetHierarchy()
        {
            if (!TryGetClaimData(out var orgId, out var orgUnitId, out var role)) return Forbid();

            var tree = await _service.GetHierarchyAsync(orgId, role, orgUnitId);
            return Ok(tree);
        }

        [HttpPut("{id:guid}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            if (!TryGetClaimData(out var orgId, out _, out _)) return Forbid();
            var success = await _service.DeactivateOrgUnitAsync(id, orgId);
            if (!success) return NotFound();
            return Ok(new { Message = "Deactivated successfully." });
        }

        // NAYA: Activate endpoint
        [HttpPut("{id:guid}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(Guid id)
        {
            if (!TryGetClaimData(out var orgId, out _, out _)) return Forbid();
            var success = await _service.ActivateOrgUnitAsync(id, orgId);
            if (!success) return NotFound();
            return Ok(new { Message = "Activated successfully." });
        }

        // NAYA: Edit endpoint
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrgUnitDto dto)
        {
            if (!TryGetClaimData(out var orgId, out _, out _)) return Forbid();
            var result = await _service.UpdateOrgUnitAsync(id, orgId, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // NAYA: Delete endpoint
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!TryGetClaimData(out var orgId, out _, out _)) return Forbid();
            var success = await _service.DeleteOrgUnitAsync(id, orgId);
            if (!success) return NotFound();
            return Ok(new { Message = "Deleted successfully." });
        }
    }
}