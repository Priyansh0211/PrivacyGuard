using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Filters;
using PrivacyGuard.Interfaces;
using System.Security.Claims;

namespace PrivacyGuard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    //[Authorize(Roles = "Admin")] 
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        private void GetCallerContext(out Guid callerUserId, out string? callerRole, out Guid callerUnitId)
        {
            Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out callerUserId);
            callerRole = User.FindFirst(ClaimTypes.Role)?.Value;
            Guid.TryParse(User.FindFirst("OrgUnitId")?.Value, out callerUnitId);
        }

        [HttpPost("add-employee")]
        [HasPermission("Users.Create")]
        public async Task<IActionResult> AddEmployee([FromBody] CreateUserDto request)
        {
            // JWT Token ke claims me se OrganizationId nikalna
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;

            if (string.IsNullOrEmpty(orgIdClaim) || !Guid.TryParse(orgIdClaim, out Guid organizationId))
            {
                return Unauthorized(new { Error = "Invalid token or organization data." });
            }

            GetCallerContext(out var callerUserId, out var callerRole, out var callerUnitId);
            var result = await _userService.AddEmployeeAsync(request, organizationId, callerUserId, callerRole, callerUnitId);
            if (result.IsSuccess)
            {
                return Ok(new { Message = "New employee added successfully!" });
            }

            return BadRequest(new { Error = result.ErrorMessage });
        }

        [HttpGet("my-team")]
        [HasPermission("Users.View")]
        public async Task<IActionResult> GetMyTeam()
        {
           
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;

            if (string.IsNullOrEmpty(orgIdClaim) || !Guid.TryParse(orgIdClaim, out Guid organizationId))
            {
                return Unauthorized(new { Error = "Invalid token or organization data." });
            }

            GetCallerContext(out _, out var callerRole, out var callerUnitId);
            var team = await _userService.GetOrganizationEmployeesAsync(organizationId, callerRole, callerUnitId);
            return Ok(team);
        }

        [HttpPatch("toggle-status/{userId}")]
        [HasPermission("Users.Edit")]
        public async Task<IActionResult> ToggleEmployeeStatus(Guid userId)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;

            if (string.IsNullOrEmpty(orgIdClaim) || !Guid.TryParse(orgIdClaim, out Guid organizationId))
            {
                return Unauthorized(new { Error = "Invalid token or organization data." });
            }

            GetCallerContext(out var callerUserId, out var callerRole, out var callerUnitId);
            var result = await _userService.ToggleUserStatusAsync(userId, organizationId, callerUserId, callerRole, callerUnitId);
            if (result.IsSuccess)
            {
                return Ok(new { Message = "User status updated successfully." });
            }

            return BadRequest(new { Error = result.ErrorMessage });
        }

       
        [HttpPut("edit/{userId}")]
        [HasPermission("Users.Edit")]
        public async Task<IActionResult> EditEmployee(Guid userId, [FromBody] UpdateUserDto request)
        {
           
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;

            if (string.IsNullOrEmpty(orgIdClaim) || !Guid.TryParse(orgIdClaim, out Guid organizationId))
            {
                return Unauthorized(new { Error = "Invalid token or organization data." });
            }

            GetCallerContext(out var callerUserId, out var callerRole, out var callerUnitId);
            var result = await _userService.UpdateEmployeeAsync(userId, request, organizationId, callerUserId, callerRole, callerUnitId);
            if (result.IsSuccess)
            {
                return Ok(new { Message = "User details updated successfully!" });
            }

            return BadRequest(new { Error = result.ErrorMessage });
        }
    }
}