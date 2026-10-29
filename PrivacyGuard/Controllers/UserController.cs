using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Filters;
using PrivacyGuard.Interfaces;

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

        [HttpPost("add-employee")]
        [HasPermission("Users.Add")]
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
        [HasPermission("Users.View")]
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
        [HasPermission("Users.Edit")]
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

       
        [HttpPut("edit/{userId}")]
        [HasPermission("Users.Edit")]
        public async Task<IActionResult> EditEmployee(Guid userId, [FromBody] UpdateUserDto request)
        {
           
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;

            if (string.IsNullOrEmpty(orgIdClaim) || !Guid.TryParse(orgIdClaim, out Guid organizationId))
            {
                return Unauthorized(new { Error = "Invalid token or organization data." });
            }

            // Service call karna
            var result = await _userService.UpdateEmployeeAsync(userId, request, organizationId);

            if (result.IsSuccess)
            {
                return Ok(new { Message = "User details updated successfully!" });
            }

            return BadRequest(new { Error = result.ErrorMessage });
        }



    }
}