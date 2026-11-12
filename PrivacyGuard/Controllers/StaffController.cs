using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Filters; // DHYAN RAHE: Ye namespace add karna zaroori hai
using PrivacyGuard.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace PrivacyGuard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Pura controller secure kar diya
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpPost("create")]
        [HasPermission("Users.Create")] // FIX: Ab sirf wo user jiske paas Create permission hai, wahi staff bana payega
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffDto dto)
        {
            var orgIdClaim = User.FindFirst("OrganizationId");
            if (orgIdClaim == null || !Guid.TryParse(orgIdClaim.Value, out var organizationId))
            {
                return Forbid();
            }

            var result = await _staffService.CreateStaffAsync(dto, organizationId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }

            return Ok(new { Message = "Staff created successfully", Data = result.Staff });
        }

        [HttpGet]
        [HasPermission("Users.View")] // FIX: Staff list dekhne ke liye View permission
        public async Task<IActionResult> GetStaffList()
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Forbid();

            var organizationId = Guid.Parse(orgIdClaim);

            var staffList = await _staffService.GetStaffListAsync(organizationId);

            return Ok(staffList);
        }
    }
}