using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Filters; // FIX: Permission filter ke liye namespace zaroori hai
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

        // ENDPOINT: Paginated List for Dashboard Grid
         
        [HttpGet]
        [HasPermission("DataPrincipal.View")] // UPDATE: View permission add kar di
        public async Task<IActionResult> GetDataPrincipals(
            [FromQuery] string search = "",
            [FromQuery] string classType = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var result = await _dataPrincipalService.GetDataPrincipalsListAsync(
                Guid.Parse(orgIdClaim), search, classType, page, pageSize);

            return Ok(result);
        }

        // ==========================================
        // ENDPOINT: Add Single Data Principal
        // ==========================================
        [HttpPost("add")]
        [HasPermission("DataPrincipal.Manage")] // UPDATE: Create/Manage ke liye strict permission
        public async Task<IActionResult> AddDataPrincipal([FromBody] DataPrincipalDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            if (string.IsNullOrEmpty(request.Mobile) && string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("At least Mobile or Email is required.");
            }

            var dataPrincipal = await _dataPrincipalService.CreateSingleDataPrincipalAsync(request, Guid.Parse(orgIdClaim));

            return Ok(new
            {
                Message = "Data Principal added successfully",
                ReferenceId = dataPrincipal.ReferenceId,
                DataPrincipalId = dataPrincipal.DataPrincipalId
            });
        }

        // ENDPOINT: 360-Degree View Dashboard
        // ==========================================
        [HttpGet("{id}/360-view")]
        [HasPermission("DataPrincipal.View")] // UPDATE: View permission add kar di
        public async Task<IActionResult> Get360View(Guid id)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var viewData = await _dataPrincipalService.Get360ViewAsync(id, Guid.Parse(orgIdClaim));

            if (viewData == null)
                return NotFound(new { Message = "Data Principal not found." });

            return Ok(viewData);
        }
        // ==========================================
        // 1. GET: DATA PRINCIPAL COUNT (Dashboard Widget Ke Liye)
        // ==========================================
        [HttpGet("count")]
        [HasPermission("DataPrincipal.View")]
        public async Task<IActionResult> GetTotalCount([FromQuery] Guid? orgUnitId)
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr))
                return Unauthorized(new { message = "Organization ID missing in token." });

            var organizationId = Guid.Parse(orgIdStr);

            // Agar login karne wala employee kisi specific Circle/OrgUnit se juda hai, 
            // toh token se uska OrgUnitId uthao taaki use sirf apne circle ka count dikhe.
            var tokenOrgUnitIdStr = User.FindFirstValue("OrgUnitId");
            Guid? finalOrgUnitId = orgUnitId ?? (string.IsNullOrEmpty(tokenOrgUnitIdStr) ? null : Guid.Parse(tokenOrgUnitIdStr));

            var count = await _dataPrincipalService.GetTotalPrincipalsCountAsync(organizationId, finalOrgUnitId);

            return Ok(new { totalCount = count });
        }

        // ==========================================
        // 2. POST: BULK IMPORT (CSV ya JSON Array data ke liye)
        // ==========================================
        [HttpPost("bulk-import")]
        [HasPermission("DataPrincipal.Manage")]
        public async Task<IActionResult> BulkImport([FromBody] List<DataPrincipalDto> request)
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr))
                return Unauthorized(new { message = "Organization ID missing in token." });

            var organizationId = Guid.Parse(orgIdStr);

            // Bulk upload karne wale staff ka circle ID (agar ho toh)
            var orgUnitIdStr = User.FindFirstValue("OrgUnitId");
            Guid? orgUnitId = string.IsNullOrEmpty(orgUnitIdStr) ? null : Guid.Parse(orgUnitIdStr);

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