using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces;
using PrivacyGuard.Interfaces.Services;

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
            var result = await _tenantService.LoginAsync(request);

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
    }
}