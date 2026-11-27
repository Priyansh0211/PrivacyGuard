using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Services;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using PrivacyGuard.Filters;
using PrivacyGuard.Helpers;

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

        // Service ke exceptions ko sahi HTTP status me convert karta hai
        private IActionResult MapOrgUnitException(InvalidOperationException ex)
        {
            return ex switch
            {
                OrgUnitForbiddenException => StatusCode(403, new { Message = ex.Message }), // 403
                OrgUnitConflictException => Conflict(new { Message = ex.Message }),          // 409
                _ => BadRequest(new { Message = ex.Message })                                // 400
            };
        }

        [HttpPost]
        [HasPermission("OrgUnit.Manage")]
        public async Task<IActionResult> Create([FromBody] CreateOrgUnitDto dto)
        {
            if (!TryGetClaimData(out var orgId, out var callerUnitId, out var callerRole)) return Forbid();

            try
            {
                var result = await _service.CreateOrgUnitAsync(dto, orgId, callerRole, callerUnitId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return MapOrgUnitException(ex);
            }
        }

        [HttpGet("hierarchy")]
        [HasPermission("OrgUnit.View")]
        public async Task<IActionResult> GetHierarchy()
        {
            if (!TryGetClaimData(out var orgId, out var orgUnitId, out var role)) return Forbid();

            var tree = await _service.GetHierarchyAsync(orgId, role, orgUnitId);
            return Ok(tree);
        }

        [HttpPut("{id:guid}/deactivate")]
        [HasPermission("OrgUnit.Manage")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            if (!TryGetClaimData(out var orgId, out var callerUnitId, out var callerRole)) return Forbid();

            try
            {
                var success = await _service.DeactivateOrgUnitAsync(id, orgId, callerRole, callerUnitId);
                if (!success) return NotFound(new { Message = "Org unit not found." });
                return Ok(new { Message = "Deactivated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return MapOrgUnitException(ex);
            }
        }

        [HttpPut("{id:guid}/activate")]
        [HasPermission("OrgUnit.Manage")]
        public async Task<IActionResult> Activate(Guid id)
        {
            if (!TryGetClaimData(out var orgId, out var callerUnitId, out var callerRole)) return Forbid();

            try
            {
                var success = await _service.ActivateOrgUnitAsync(id, orgId, callerRole, callerUnitId);
                if (!success) return NotFound(new { Message = "Org unit not found." });
                return Ok(new { Message = "Activated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return MapOrgUnitException(ex);
            }
        }

        [HttpPut("{id:guid}")]
        [HasPermission("OrgUnit.Manage")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrgUnitDto dto)
        {
            if (!TryGetClaimData(out var orgId, out var callerUnitId, out var callerRole)) return Forbid();

            try
            {
                var result = await _service.UpdateOrgUnitAsync(id, orgId, dto, callerRole, callerUnitId);
                if (result == null) return NotFound(new { Message = "Org unit not found." });
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return MapOrgUnitException(ex);
            }
        }

        [HttpDelete("{id:guid}")]
        [HasPermission("OrgUnit.Manage")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!TryGetClaimData(out var orgId, out var callerUnitId, out var callerRole)) return Forbid();

            try
            {
                var success = await _service.DeleteOrgUnitAsync(id, orgId, callerRole, callerUnitId);
                if (!success) return NotFound(new { Message = "Org unit not found." });
                return Ok(new { Message = "Deleted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return MapOrgUnitException(ex);
            }
        }
    }
}