using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces;

namespace PrivacyGuard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // SECURITY: Sirf Admin hi is API ko call kar sakta hai!
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("add-employee")]
        public async Task<IActionResult> AddEmployee([FromBody] CreateUserDto request)
        {
            // JWT Token ke claims me se OrganizationId nikalna
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;

            if (string.IsNullOrEmpty(orgIdClaim) || !Guid.TryParse(orgIdClaim, out Guid organizationId))
            {
                return Unauthorized(new { Error = "Invalid token or organization data." });
            }

            // Service ko data aur secure OrganizationId pass karna
            var result = await _userService.AddEmployeeAsync(request, organizationId);

            if (result.IsSuccess)
            {
                return Ok(new { Message = "New employee added successfully!" });
            }

            return BadRequest(new { Error = result.ErrorMessage });
        }

        [HttpGet("my-team")]
        public async Task<IActionResult> GetMyTeam()
        {
           
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;

            if (string.IsNullOrEmpty(orgIdClaim) || !Guid.TryParse(orgIdClaim, out Guid organizationId))
            {
                return Unauthorized(new { Error = "Invalid token or organization data." });
            }

            
            var team = await _userService.GetOrganizationEmployeesAsync(organizationId);

            return Ok(team);
        }

        [HttpPatch("toggle-status/{userId}")]
        public async Task<IActionResult> ToggleEmployeeStatus(Guid userId)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;

            if (string.IsNullOrEmpty(orgIdClaim) || !Guid.TryParse(orgIdClaim, out Guid organizationId))
            {
                return Unauthorized(new { Error = "Invalid token or organization data." });
            }

            var result = await _userService.ToggleUserStatusAsync(userId, organizationId);

            if (result.IsSuccess)
            {
                return Ok(new { Message = "User status updated successfully." });
            }

            return BadRequest(new { Error = result.ErrorMessage });
        }
    }
}