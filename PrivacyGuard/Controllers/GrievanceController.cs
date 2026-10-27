using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Filters;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
// using PrivacyGuard.Filters; // [HasPermission] yahan se aayega

namespace PrivacyGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GrievanceController : ControllerBase
    {
        private readonly IGrievanceService _grievanceService;
        private readonly IBreachService _breachService;
        public GrievanceController(IGrievanceService grievanceService , IBreachService breachService)
        {
            _grievanceService = grievanceService; 
            _breachService = breachService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGrievanceDetails(Guid id)
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr)) return Unauthorized();

            // Agar permission based hai:
            bool isAdmin = User.HasClaim(c => c.Type == "Permission" && c.Value == "Grievances.Manage");

            var result = await _grievanceService.GetGrievanceDetailsAsync(id, Guid.Parse(orgIdStr), isAdmin);
            if (result == null) return NotFound(new { message = "Grievance not found." });

            return Ok(result);
        }

        [HttpPost("raise")]
        public async Task<IActionResult> RaiseGrievance([FromBody] CreateGrievanceDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orgIdStr = User.FindFirstValue("OrganizationId");

            var ticketId = await _grievanceService.CreateGrievanceAsync(Guid.Parse(userIdStr!), Guid.Parse(orgIdStr!), dto);
            return Ok(new { message = "Grievance submitted successfully.", ticketId = ticketId });
        }

        [HttpGet("all")]
        [HasPermission("Grievances.View")] // Custom RBAC 
        public async Task<IActionResult> GetAllGrievances()
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            var grievances = await _grievanceService.GetAllGrievancesAsync(Guid.Parse(orgIdStr!));
            return Ok(grievances);
        }

        [HttpPatch("{id}/assign")]
        [HasPermission("Grievances.Manage")] // Custom RBAC
        public async Task<IActionResult> AssignToMe(Guid id)
        {
            var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orgIdStr = User.FindFirstValue("OrganizationId");

            var success = await _grievanceService.AssignTicketAsync(id, Guid.Parse(orgIdStr!), Guid.Parse(adminIdStr!));
            if (!success) return NotFound(new { message = "Grievance not found." });
            return Ok(new { message = "Ticket assigned successfully and is now In Progress." });
        }

        [HttpPost("{id}/reply")]
        public async Task<IActionResult> AddReply(Guid id, [FromBody] AddGrievanceMessageDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orgIdStr = User.FindFirstValue("OrganizationId");

            bool isAdmin = User.HasClaim(c => c.Type == "Permission" && c.Value == "Grievances.Manage");

            var success = await _grievanceService.AddReplyAsync(id, Guid.Parse(userIdStr!), Guid.Parse(orgIdStr!), dto, isAdmin);
            if (!success) return NotFound(new { message = "Grievance not found." });
            return Ok(new { message = dto.IsInternal && isAdmin ? "Internal note added." : "Reply sent successfully." });
        }


        [HttpPost("{id}/escalate-to-breach")]
        [HasPermission("Breaches.Manage")]
        public async Task<IActionResult> EscalateToBreach(Guid id, [FromBody] EscalateToBreachDto dto)
        {
            var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orgIdStr = User.FindFirstValue("OrganizationId");

            
            var result = await _breachService.EscalateGrievanceToBreachAsync(
                id,
                Guid.Parse(adminIdStr!),
                Guid.Parse(orgIdStr!),
                dto
            );

            if (result.StartsWith("Error"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = "Grievance successfully escalated to a Data Breach Incident." });
        }
    }
}
