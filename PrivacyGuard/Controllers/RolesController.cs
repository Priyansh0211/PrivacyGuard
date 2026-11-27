using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.Filters;
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

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var organizationIdValue = User.FindFirstValue("OrganizationId");
            if (!Guid.TryParse(organizationIdValue, out var organizationId))
                return Unauthorized(new { message = "Organization context missing in token." });

            var result = await _roleService.GetRoleSummariesAsync(organizationId);
            return Ok(result);
        }

        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var result = await _roleService.GetGroupedPermissionsAsync();
            return Ok(result);
        }

        [HttpPost("create")]
        [HasPermission("Roles.Manage")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto request)
        {
            if (!Guid.TryParse(User.FindFirst("OrganizationId")?.Value, out var organizationId))
            {
                return Unauthorized(new { message = "Organization context missing in token." });
            }

            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var callerUserId))
            {
                return Unauthorized(new { message = "User context missing." });
            }

            var callerRole = User.FindFirstValue(ClaimTypes.Role);

            var result = await _roleService.CreateCustomRoleAsync(request, organizationId, callerUserId, callerRole);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = "Custom Role created successfully!" });
        }

        [HttpGet("user-permissions/grouped/{userId}")]
        [HasPermission("Users.View")]
        public async Task<IActionResult> GetUserGroupedPermissions(Guid userId)
        {
            if (!Guid.TryParse(User.FindFirstValue("OrganizationId"), out var organizationId))
                return Unauthorized(new { message = "Organization context missing in token." });

            var result = await _roleService.GetUserGroupedPermissionsForOrgAsync(userId, organizationId);
            if (result == null)
                return NotFound(new { message = "User not found." });

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

        [HttpGet("{id}/edit")]
        [HasPermission("Roles.Manage")] // Sirf Role Manage karne wale dekh sakte hain
        public async Task<IActionResult> GetRoleForEdit(Guid id)
        {
            try
            {
                var orgIdStr = User.FindFirstValue("OrganizationId");
                if (string.IsNullOrEmpty(orgIdStr))
                    return Unauthorized(new { message = "Organization ID missing in token." });

                var organizationId = Guid.Parse(orgIdStr);

                var roleDetails = await _roleService.GetRoleForEditAsync(id, organizationId);

                if (roleDetails == null)
                    return NotFound(new { message = "Role not found." });

                return Ok(roleDetails);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}/edit")]
        [HasPermission("Roles.Manage")]
        public async Task<IActionResult> EditRole(Guid id, [FromBody] UpdateRoleDto dto)
        {
            try
            {
                var orgIdStr = User.FindFirstValue("OrganizationId");
                if (!Guid.TryParse(orgIdStr, out var organizationId))
                {
                    return Unauthorized(new { message = "Organization ID missing in token." });
                }

                if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var callerUserId))
                {
                    return Unauthorized(new { message = "User context missing." });
                }

                var callerRole = User.FindFirstValue(ClaimTypes.Role);

                var result = await _roleService.UpdateRoleAndPermissionsAsync(id, organizationId, dto, callerUserId, callerRole);

                if (result.NotFound)
                {
                    return NotFound(new { message = result.ErrorMessage });
                }

                if (!result.IsSuccess)
                {
                    return BadRequest(new { message = result.ErrorMessage });
                }

                return Ok(new { message = "Role and permissions updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}