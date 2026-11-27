using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Filters;
using PrivacyGuard.Helpers;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PrivacyGuard.Controllers
{
    [Route("api/dataprincipal")]
    [ApiController]
    [Authorize]
    public class DataPrincipalController : ControllerBase
    {
        private readonly IDataPrincipalService _dataPrincipalService;

        public DataPrincipalController(IDataPrincipalService dataPrincipalService)
        {
            _dataPrincipalService = dataPrincipalService;
        }

        private void GetCallerContext(out string? callerRole, out Guid callerUnitId)
        {
            callerRole = User.FindFirst(ClaimTypes.Role)?.Value;
            Guid.TryParse(User.FindFirst("OrgUnitId")?.Value, out callerUnitId);
        }

        // Non-admin user jiske paas koi unit nahi hai, wo kuch create nahi kar sakta
        private static bool IsUnitlessScopedCaller(string? role, Guid unitId) =>
            RoleGuard.IsBranchScoped(role, unitId) && unitId == Guid.Empty;

        // ENDPOINT: Paginated List for Dashboard Grid
        [HttpGet]
        [HasPermission("DataPrincipal.View")]
        public async Task<IActionResult> GetDataPrincipals(
            [FromQuery] string search = "",
            [FromQuery] string classType = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            GetCallerContext(out var callerRole, out var callerUnitId);

            var result = await _dataPrincipalService.GetDataPrincipalsListAsync(
                Guid.Parse(orgIdClaim), search, classType, page, pageSize, callerRole, callerUnitId);

            return Ok(result);
        }

        // ENDPOINT: Add Single Data Principal
        [HttpPost("add")]
        [HasPermission("DataPrincipal.Manage")]
        public async Task<IActionResult> AddDataPrincipal([FromBody] DataPrincipalDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            if (string.IsNullOrEmpty(request.Mobile) && string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("At least Mobile or Email is required.");
            }

            GetCallerContext(out var callerRole, out var callerUnitId);
            if (IsUnitlessScopedCaller(callerRole, callerUnitId))
                return StatusCode(403, new { Message = "You are not assigned to any branch." });

            // Principal caller ki branch me banega (org-wide admin ki unit na ho to unit null)
            Guid? orgUnitId = callerUnitId == Guid.Empty ? null : callerUnitId;

            var dataPrincipal = await _dataPrincipalService.CreateSingleDataPrincipalAsync(request, Guid.Parse(orgIdClaim), orgUnitId);

            return Ok(new
            {
                Message = "Data Principal added successfully",
                ReferenceId = dataPrincipal.ReferenceId,
                DataPrincipalId = dataPrincipal.DataPrincipalId
            });
        }

        // ENDPOINT: 360-Degree View Dashboard
        [HttpGet("{id}/360-view")]
        [HasPermission("DataPrincipal.View")]
        public async Task<IActionResult> Get360View(Guid id)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            GetCallerContext(out var callerRole, out var callerUnitId);

            var viewData = await _dataPrincipalService.Get360ViewAsync(id, Guid.Parse(orgIdClaim), callerRole, callerUnitId);

            if (viewData == null)
                return NotFound(new { Message = "Data Principal not found." });

            return Ok(viewData);
        }

        // ENDPOINT: Edit / Update Data Principal
        [HttpPut("{id}")]
        [HasPermission("DataPrincipal.Manage")]
        public async Task<IActionResult> UpdateDataPrincipal(Guid id, [FromBody] DataPrincipalDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            GetCallerContext(out var callerRole, out var callerUnitId);

            var success = await _dataPrincipalService.UpdateDataPrincipalAsync(id, request, Guid.Parse(orgIdClaim), callerRole, callerUnitId);

            if (!success) return NotFound(new { Message = "Data Principal not found." });

            return Ok(new { Message = "Data Principal updated successfully." });
        }

        // ENDPOINT: Delete / Anonymize (Right to Erasure)
        [HttpDelete("{id}")]
        [HasPermission("DataPrincipal.Manage")]
        public async Task<IActionResult> DeleteDataPrincipal(Guid id)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            GetCallerContext(out var callerRole, out var callerUnitId);

            // DPDP Act: Hard delete ki jagah Anonymize/Scramble kar rahe hain
            bool success;
            try
            {
                success = await _dataPrincipalService.AnonymizeDataPrincipalAsync(id, Guid.Parse(orgIdClaim), callerRole, callerUnitId);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }

            if (!success) return NotFound(new { Message = "Data Principal not found." });

            return Ok(new { Message = "Data Principal has been successfully anonymized as per Right to Erasure." });
        }

        // 1. GET: DATA PRINCIPAL COUNT (Dashboard Widget Ke Liye)
        [HttpGet("count")]
        [HasPermission("DataPrincipal.View")]
        public async Task<IActionResult> GetTotalCount([FromQuery] Guid? orgUnitId)
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr))
                return Unauthorized(new { message = "Organization ID missing in token." });

            var organizationId = Guid.Parse(orgIdStr);

            GetCallerContext(out var callerRole, out var callerUnitId);

            try
            {
                // Scoped user ko apni branch (subtree) ka count milta hai, doosri branch par 403
                var count = await _dataPrincipalService.GetTotalPrincipalsCountAsync(organizationId, orgUnitId, callerRole, callerUnitId);
                return Ok(new { totalCount = count });
            }
            catch (OrgUnitForbiddenException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        // 2. POST: BULK IMPORT (CSV ya JSON Array data ke liye)
        [HttpPost("bulk-import")]
        [HasPermission("DataPrincipal.Manage")]
        public async Task<IActionResult> BulkImport([FromBody] List<DataPrincipalDto> request)
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr))
                return Unauthorized(new { message = "Organization ID missing in token." });

            var organizationId = Guid.Parse(orgIdStr);

            GetCallerContext(out var callerRole, out var callerUnitId);
            if (IsUnitlessScopedCaller(callerRole, callerUnitId))
                return StatusCode(403, new { message = "You are not assigned to any branch." });

            Guid? orgUnitId = callerUnitId == Guid.Empty ? null : callerUnitId;

            if (request == null || request.Count == 0)
                return BadRequest(new { message = "No data provided for import. List is empty." });

            var result = await _dataPrincipalService.BulkImportPrincipalsAsync(request, organizationId, orgUnitId);

            return Ok(new
            {
                success = true,
                importedCount = result.SuccessCount,
                message = result.Message
            });
        }
    }
}