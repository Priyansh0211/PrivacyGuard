using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces;
using PrivacyGuard.Interfaces.Services;
using System.Security.Claims;

namespace PrivacyGuard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public AuthController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [HttpPost("register-tenant")]
        public async Task<IActionResult> Register([FromBody] RegisterTenantDto request)
        {
            var result = await _tenantService.RegisterTenantAsync(request);

            if (result.IsSuccess)
            {
                return Ok(new
                {
                    Message = "Tenant Registration Successful!",
                    OrganizationId = result.OrganizationId,
                    AdminId = result.AdminId
                });
            }

            return BadRequest(new { Error = result.ErrorMessage });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var loginIp = HttpContext.Connection.RemoteIpAddress?
                .MapToIPv4()
                .ToString();

            var result = await _tenantService.LoginAsync(request, loginIp);

            if (result.IsSuccess)
            {
                return Ok(new { Token = result.Token });
            }

            return Unauthorized(new { Error = result.ErrorMessage });
        }


        [HttpPost("logout")]
        [Authorize] 
        public IActionResult Logout()
        {
            // Future update: Agar tumhe user ke logout hone ka time database (AuditLog) mein save karna ho, 
            // toh wo service call yahan aayegi.

            // Abhi ke liye hum bas frontend ko HTTP 200 OK bhej rahe hain
            return Ok(new
            {
                IsSuccess = true,
                Message = "Logged out successfully. Please remove the token from local storage or cookies on the client side."
            });
        }

        [HttpGet("dropdownRole")]
        public async Task<IActionResult> GetRolesForDropdown()
        {
            // Token se OrganizationId nikal rahe hain taaki dusre org ke roles na dikhein
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (string.IsNullOrEmpty(orgIdStr))
            {
                return Unauthorized(new { message = "Organization ID missing in token." });
            }

            var roles = await _tenantService.GetRolesForDropdownAsync(Guid.Parse(orgIdStr));

            return Ok(roles);
        }
    }
}