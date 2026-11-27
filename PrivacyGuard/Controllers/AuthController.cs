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
        private readonly IAuditService _auditService;

        public AuthController(ITenantService tenantService, IAuditService auditService)
        {
            _tenantService = tenantService;
            _auditService = auditService;
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
            var loginIp = GetClientIpAddress();

            var result = await _tenantService.LoginAsync(request, loginIp);

            if (result.IsSuccess)
            {
                return Ok(new { Token = result.Token });
            }

            return Unauthorized(new { Error = result.ErrorMessage });
        }


        [HttpPost("logout")]
        [Authorize] 
        public async Task<IActionResult> Logout()
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ||
                !Guid.TryParse(User.FindFirstValue("OrganizationId"), out var organizationId))
            {
                return Unauthorized(new { Error = "Invalid token or user data." });
            }

            var actorEmail = User.FindFirstValue(ClaimTypes.Email) ?? userId.ToString();
            var actorRole = User.FindFirstValue(ClaimTypes.Role) ?? "Unknown";

            await _auditService.RecordAuthenticationEventAsync(
                userId,
                organizationId,
                actorEmail,
                actorRole,
                GetClientIpAddress(),
                "Logout");

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

        private string? GetClientIpAddress()
        {
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            if (remoteIpAddress?.IsIPv4MappedToIPv6 == true)
            {
                remoteIpAddress = remoteIpAddress.MapToIPv4();
            }

            return remoteIpAddress?.ToString();
        }
    }
}