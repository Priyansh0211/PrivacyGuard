using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces;
using System.Security.Claims;

namespace PrivacyGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires login
    public class DpiaController : ControllerBase
    {
        private readonly IDpiaService _dpiaService;

        public DpiaController(IDpiaService dpiaService)
        {
            _dpiaService = dpiaService;
        }

        // Helper method to extract OrgId
        private Guid GetOrganizationId()
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            return string.IsNullOrEmpty(orgIdStr) ? Guid.Empty : Guid.Parse(orgIdStr);
        }

        // Helper method to extract UserId
        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin,DPO")] // Sirf Admin ya DPO dekh sakte hain
        public async Task<IActionResult> GetAllDpia()
        {
            var orgId = GetOrganizationId();
            if (orgId == Guid.Empty) return Unauthorized("Invalid Organization.");

            var records = await _dpiaService.GetAllDpiaAsync(orgId);
            return Ok(records);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,DPO")]
        public async Task<IActionResult> GetDpiaDetails(Guid id)
        {
            var orgId = GetOrganizationId();
            if (orgId == Guid.Empty) return Unauthorized("Invalid Organization.");

            var record = await _dpiaService.GetDpiaByIdAsync(id, orgId);
            if (record == null) return NotFound("Record not found or unauthorized access.");

            return Ok(record);
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin")] // Sirf Maker (Admin) create kar sakta hai
        public async Task<IActionResult> CreateDpia([FromBody] CreateDpiaDto dto)
        {
            var orgId = GetOrganizationId();
            var userId = GetUserId();

            if (orgId == Guid.Empty || string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid security token.");

            var record = await _dpiaService.CreateDpiaAsync(dto, userId, orgId);
            return Ok(new { message = "DPIA Record created successfully.", data = record });
        }

        [HttpPut("{id}/review")]
        [Authorize(Roles = "DPO")] // Sirf Checker (DPO - Data Protection Officer) review karega
        public async Task<IActionResult> ReviewDpia(Guid id, [FromBody] ReviewDpiaDto dto)
        {
            var orgId = GetOrganizationId();
            var reviewerId = GetUserId(); // DPO ka UserId

            if (orgId == Guid.Empty || string.IsNullOrEmpty(reviewerId))
                return Unauthorized("Invalid security token.");

            var success = await _dpiaService.ReviewDpiaAsync(id, orgId, dto, reviewerId);

            if (!success) return NotFound("Record not found or unauthorized access.");

            return Ok(new { message = $"DPIA {dto.Action} processed successfully." });
        }
    }
}