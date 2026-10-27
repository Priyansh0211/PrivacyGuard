using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Services;

namespace PrivacyGuard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpPost("create")]
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
[Authorize(Roles = "Admin")] // Sirf Admin dekh sakta hai apne staff ko
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