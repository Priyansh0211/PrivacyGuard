using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces;
using PrivacyGuard.Interfaces.Services;
using System.Security.Claims;

namespace PrivacyGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Is controller ki har API ke liye valid token chahiye
    public class BreachController : ControllerBase
    {
        private readonly IBreachService _breachService;

        public BreachController(IBreachService breachService)
        {
            _breachService = breachService;
        }

        // 1. Employee Report API
        // POST: api/breach/report
        [HttpPost("report")]
        public async Task<IActionResult> ReportIncident([FromBody] ReportBreachDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orgIdStr = User.FindFirstValue("OrganizationId");

            if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(orgIdStr))
                return Unauthorized(new { message = "Invalid token claims." });

            var userId = Guid.Parse(userIdStr);
            dto.OrganizationId = Guid.Parse(orgIdStr); // Security ke liye token se OrgId force karenge

            var result = await _breachService.ReportIncidentAsync(userId, dto);
            if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = "Security incident reported successfully." });
        }

        // 2. Admin Dashboard API
        // GET: api/breach/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin,SystemAdmin")] // Sirf Admins dekh sakte hain
        public async Task<IActionResult> GetAllIncidents()
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr)) return Unauthorized();

            var incidents = await _breachService.GetAllIncidentsAsync(Guid.Parse(orgIdStr));
            return Ok(incidents);
        }

        // 3. Admin Assign API (Smart Trigger)
        // PATCH: api/breach/{id}/assign
        [HttpPatch("{id}/assign")]
        [Authorize(Roles = "Admin,SystemAdmin")]
        public async Task<IActionResult> AssignIncident(Guid id)
        {
            var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orgIdStr = User.FindFirstValue("OrganizationId");

            var result = await _breachService.AssignIncidentAsync(id, Guid.Parse(orgIdStr!), Guid.Parse(adminIdStr!));
            if (!result.IsSuccess) return NotFound(new { message = result.ErrorMessage });

            return Ok(new { message = "Incident assigned to you and status updated to Investigating." });
        }

        // 4. Admin Update/Close API
        // PATCH: api/breach/{id}/update
        [HttpPatch("{id}/update")]
        [Authorize(Roles = "Admin,SystemAdmin")]
        public async Task<IActionResult> UpdateIncident(Guid id, [FromBody] UpdateBreachStatusDto dto)
        {
            var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orgIdStr = User.FindFirstValue("OrganizationId");

            var result = await _breachService.UpdateIncidentStatusAsync(id, Guid.Parse(orgIdStr!), Guid.Parse(adminIdStr!), dto);
            if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = $"Incident status updated to {dto.Status}." });
        }
    }
}