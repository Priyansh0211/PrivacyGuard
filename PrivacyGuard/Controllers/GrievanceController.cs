using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Filters;
using PrivacyGuard.Interfaces.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PrivacyGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GrievanceController : ControllerBase
    {
        private readonly IGrievanceService _grievanceService;
        private readonly IBreachService _breachService;

        public GrievanceController(IGrievanceService grievanceService, IBreachService breachService)
        {
            _grievanceService = grievanceService;
            _breachService = breachService;
        }

        private (bool isValid, Guid userId, Guid orgId) GetUserClaims()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orgIdStr = User.FindFirstValue("OrganizationId");

            if (Guid.TryParse(userIdStr, out Guid userId) && Guid.TryParse(orgIdStr, out Guid orgId))
            {
                return (true, userId, orgId);
            }
            return (false, Guid.Empty, Guid.Empty);
        }

        private bool IsAdminUser() => User.HasClaim(c => c.Type == "Permission" && c.Value == "Grievances.Manage");

        // 1. Get Details
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGrievanceDetails(Guid id)
        {
            var claims = GetUserClaims();
            if (!claims.isValid) return Unauthorized();

            var result = await _grievanceService.GetGrievanceDetailsAsync(id, claims.orgId, IsAdminUser());
            if (result == null) return NotFound(new { message = "Grievance not found." });

            return Ok(result);
        }

        // 2. Raise Ticket (For Data Principal)
        [HttpPost("raise")]
        public async Task<IActionResult> RaiseGrievance([FromBody] CreateGrievanceDto dto)
        {
            var claims = GetUserClaims();
            if (!claims.isValid) return Unauthorized();

            var ticketId = await _grievanceService.CreateGrievanceAsync(claims.userId, claims.orgId, dto);
            return Ok(new { message = "Grievance submitted successfully.", ticketId = ticketId });
        }

        // 3. Get ALL (For DPO/Admin)
        [HttpGet("all")]
        [HasPermission("Grievances.View")]
        public async Task<IActionResult> GetAllGrievances()
        {
            var claims = GetUserClaims();
            if (!claims.isValid) return Unauthorized();

            var grievances = await _grievanceService.GetAllGrievancesAsync(claims.orgId);
            return Ok(grievances);
        }

        // 🚨 4. Get MY Tickets (For Data Principal)
        [HttpGet("my-grievances")]
        public async Task<IActionResult> GetMyGrievances()
        {
            var claims = GetUserClaims();
            if (!claims.isValid) return Unauthorized();

            var grievances = await _grievanceService.GetMyGrievancesAsync(claims.userId, claims.orgId);
            return Ok(grievances);
        }

        // 5. Assign to Me (For DPO)
        [HttpPatch("{id}/assign")]
        [HasPermission("Grievances.Manage")]
        public async Task<IActionResult> AssignToMe(Guid id)
        {
            var claims = GetUserClaims();
            if (!claims.isValid) return Unauthorized();

            var success = await _grievanceService.AssignTicketAsync(id, claims.orgId, claims.userId);
            if (!success) return NotFound(new { message = "Grievance not found." });
            return Ok(new { message = "Ticket assigned successfully and is now In Progress." });
        }

        // 🚨 6. Resolve Ticket (For DPO)
        [HttpPatch("{id}/resolve")]
        [HasPermission("Grievances.Manage")]
        public async Task<IActionResult> ResolveGrievance(Guid id)
        {
            var claims = GetUserClaims();
            if (!claims.isValid) return Unauthorized();

            try
            {
                var success = await _grievanceService.ResolveGrievanceAsync(id, claims.orgId, claims.userId);
                if (!success) return NotFound(new { message = "Grievance not found." });
                return Ok(new { message = "Grievance has been marked as Resolved." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 7. Add Reply/Internal Note
        [HttpPost("{id}/reply")]
        public async Task<IActionResult> AddReply(Guid id, [FromBody] AddGrievanceMessageDto dto)
        {
            var claims = GetUserClaims();
            if (!claims.isValid) return Unauthorized();

            bool isAdmin = IsAdminUser();
            var success = await _grievanceService.AddReplyAsync(id, claims.userId, claims.orgId, dto, isAdmin);

            if (!success) return NotFound(new { message = "Grievance not found." });
            return Ok(new { message = dto.IsInternal && isAdmin ? "Internal note added." : "Reply sent successfully." });
        }

        // 8. Escalate to Breach
        [HttpPost("{id}/escalate-to-breach")]
        [HasPermission("Breaches.Manage")] // Strict check
        public async Task<IActionResult> EscalateToBreach(Guid id, [FromBody] EscalateToBreachDto dto)
        {
            var claims = GetUserClaims();
            if (!claims.isValid) return Unauthorized();

            var result = await _breachService.EscalateGrievanceToBreachAsync(id, claims.userId, claims.orgId, dto);

            if (result.StartsWith("Error")) return BadRequest(new { message = result });

            return Ok(new { message = "Grievance successfully escalated to a Data Breach Incident." });
        }
    }
}