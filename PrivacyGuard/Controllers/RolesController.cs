using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using PrivacyGuard.Services;
using System.Security.Claims;
using static PrivacyGuard.DTOs.RolePermissionDto;

namespace PrivacyGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var result = await _roleService.GetGroupedPermissionsAsync();
            return Ok(result);
        }
    

    [HttpPost("create")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto request)
        {
           
            var orgIdString = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdString))
            {
                return Unauthorized(new { message = "Organization context missing in token." });
            }

            var organizationId = Guid.Parse(orgIdString);

           
            var result = await _roleService.CreateCustomRoleAsync(request, organizationId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = "Custom Role created successfully!" });
        }


        

[HttpGet("user-permissions/grouped/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUserGroupedPermissions(Guid userId)
    {
        var result = await _roleService.GetUserGroupedPermissionsAsync(userId);
        return Ok(result);
    }

    
    [HttpGet("my-permissions/grouped")]
    [Authorize]
    public async Task<IActionResult> GetMyGroupedPermissions()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized(new { message = "User context missing." });
        }

        var userId = Guid.Parse(userIdString);
        var result = await _roleService.GetUserGroupedPermissionsAsync(userId);
        return Ok(result);
    }

}
}