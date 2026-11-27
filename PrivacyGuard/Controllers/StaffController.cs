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
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        private void GetCallerContext(out Guid callerUserId, out string? callerRole, out Guid callerUnitId)
        {
            Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out callerUserId);
            callerRole = User.FindFirst(ClaimTypes.Role)?.Value;
            Guid.TryParse(User.FindFirst("OrgUnitId")?.Value, out callerUnitId);
        }

        [HttpPost("create")]
        [HasPermission("Users.Create")]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffDto dto)
        {
            var orgIdClaim = User.FindFirst("OrganizationId");
            if (orgIdClaim == null || !Guid.TryParse(orgIdClaim.Value, out var organizationId))
            {
                return Forbid();
            }

            GetCallerContext(out var callerUserId, out var callerRole, out var callerUnitId);

            var result = await _staffService.CreateStaffAsync(dto, organizationId, callerUserId, callerRole, callerUnitId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }

            return Ok(new { Message = "Staff created successfully", Data = result.Staff });
        }

        [HttpGet]
        [HasPermission("Users.View")]
        public async Task<IActionResult> GetStaffList()
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (!Guid.TryParse(orgIdClaim, out var organizationId)) return Forbid();

            GetCallerContext(out _, out var callerRole, out var callerUnitId);

            var staffList = await _staffService.GetStaffListAsync(organizationId, callerRole, callerUnitId);

            return Ok(staffList);
        }
    }
}