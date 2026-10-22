using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Services;
using System.Security.Claims;

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
        public async Task<IActionResult> RaiseRequest([FromBody] CreatePrivacyRequestDto dto)
        {
            if (!TryGetOrganizationId(out var orgId))
                return Forbid();

            var result = await _service.RaiseRequestAsync(dto, orgId);

            return Ok(new { Message = "Privacy request submitted successfully.", RequestId = result.RequestId });
        }

        // API 2: Admin/DPO Request Resolve karega
        [HttpPut("resolve/{id:guid}")]
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
        public async Task<IActionResult> GetMyRequests(string dataPrincipalId)
        {
            if (!TryGetOrganizationId(out var orgId))
                return Forbid();

            var requests = await _service.GetMyRequestsAsync(dataPrincipalId, orgId);

            return Ok(requests);
        }
    }
}