using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Filters; // FIX: Permission attribute ke liye
using PrivacyGuard.Interfaces.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PrivacyGuard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Token zaroori hai
    public class PrivacyRequestController : ControllerBase
    {
        private readonly IPrivacyRequestService _service;

        public PrivacyRequestController(IPrivacyRequestService service)
        {
            _service = service;
        }

        // Helper: Token se OrganizationId nikalne ke liye
        private bool TryGetOrganizationId(out Guid organizationId)
        {
            organizationId = Guid.Empty;
            var claim = User.FindFirst("OrganizationId");
            return claim != null && Guid.TryParse(claim.Value, out organizationId);
        }

        // API 1: User Request Raise karega
        [HttpPost("raise")]
        [HasPermission("PrivacyRequest.CreatePublic")] // UPDATE: User-facing permission
        public async Task<IActionResult> RaiseRequest([FromBody] CreatePrivacyRequestDto dto)
        {
            if (!TryGetOrganizationId(out var orgId))
                return Forbid();

            var result = await _service.RaiseRequestAsync(dto, orgId);

            return Ok(new { Message = "Privacy request submitted successfully.", RequestId = result.RequestId });
        }

        // API 2: Admin/DPO Request Resolve karega
        [HttpPut("resolve/{id:guid}")]
        [HasPermission("PrivacyRequest.Manage")] // UPDATE: Admin-only permission
        public async Task<IActionResult> ResolveRequest(Guid id, [FromBody] ResolvePrivacyRequestDto dto)
        {
            if (!TryGetOrganizationId(out var orgId))
                return Forbid();

            var success = await _service.ResolveRequestAsync(id, dto, orgId);

            if (!success)
                return NotFound(new { Message = "Privacy request not found." });

            return Ok(new { Message = "Privacy request resolved successfully." });
        }

        // API 3: User apni saari requests dekhega
        [HttpGet("my-requests/{dataPrincipalId}")]
        [HasPermission("PrivacyRequest.ReadPublic")] // UPDATE: User-facing permission
        public async Task<IActionResult> GetMyRequests(string dataPrincipalId)
        {
            if (!TryGetOrganizationId(out var orgId))
                return Forbid();

            var requests = await _service.GetMyRequestsAsync(dataPrincipalId, orgId);

            return Ok(requests);
        }

        // API 4: Admin Dashboard Filter & List
        [HttpGet]
        [HasPermission("PrivacyRequest.View")] // UPDATE: Admin View permission
        public async Task<IActionResult> GetRequests(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] string? type,
            [FromQuery] bool overdueOnly = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!TryGetOrganizationId(out var orgId))
                return Forbid();

            if (status == "Any status") status = null;
            if (type == "Any right") type = null;

            var result = await _service.GetFilteredRequestsAsync(orgId, search, status, type, overdueOnly, page, pageSize);

            return Ok(result);
        }

        // API 5: Get Single Privacy Request Details for Admin
        [HttpGet("{id:guid}/detail")]
        [HasPermission("PrivacyRequest.View")] // UPDATE: Admin View permission
        public async Task<IActionResult> GetRequestDetail(Guid id)
        {
            if (!TryGetOrganizationId(out var orgId))
                return Forbid();

            var detail = await _service.GetRequestDetailsAsync(id, orgId);

            if (detail == null)
                return NotFound(new { Message = "Privacy request not found." });

            return Ok(detail);
        }
    }
}