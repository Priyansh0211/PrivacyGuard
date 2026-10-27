using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Services;
using System.Security.Claims;

namespace PrivacyGuard.Controllers
{
    // Yeh attribute is class ko API controller banata hai
    [ApiController]
    // Route set kiya, ye API '/api/consent' par chalegi
    [Route("api/[controller]")]
    // Bina JWT token ke koi isme enter nahi kar sakta
    [Authorize]
    public class ConsentController : ControllerBase
    {
        // Service ka variable banaya (Controller DB/Repo se direct baat nahi karega)
        private readonly IConsentService _consentService;

        // Constructor mein service ko inject kiya
        public ConsentController(IConsentService consentService)
        {
            _consentService = consentService;
        }

        // JWT token se OrganizationId nikalne ka helper function
        private bool TryGetOrganizationId(out Guid organizationId)
        {
            organizationId = Guid.Empty; // Default empty
            // Token ke andar 'OrganizationId' naam ka data (claim) dhundo
            var claim = User.FindFirst("OrganizationId");
            // Agar mil gaya aur valid Guid hai, toh true return karo
            return claim != null && Guid.TryParse(claim.Value, out organizationId);
        }

        // API Endpoint 1: Consent Dene Ke Liye (POST /api/consent/grant)
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitConsent([FromBody] SubmitConsentDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized("Token mein OrganizationId missing hai.");

            var orgId = Guid.Parse(orgIdClaim);
            var result = await _consentService.SubmitGranularConsentAsync(request, orgId);
            return Ok(result);
        }

        [HttpPost("withdraw/{recordId}/{inventoryId}")]
        public async Task<IActionResult> WithdrawConsent(Guid recordId, Guid inventoryId)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized("Token mein OrganizationId missing hai.");

            var orgId = Guid.Parse(orgIdClaim);
            var success = await _consentService.WithdrawConsentAsync(recordId, inventoryId, orgId);
            if (!success) return BadRequest("Withdrawal failed or not found.");
            return Ok(new { message = "Consent withdrawn successfully." });
        }

        // API Endpoint 3: User ki Consent History Dekhne Ke Liye (GET /api/consent/history/{userId})
        [HttpGet("history/{dataPrincipalId}")]
        public async Task<IActionResult> GetConsentHistory(string dataPrincipalId)
        {
            // Token validation
            if (!TryGetOrganizationId(out var organizationId))
                return Forbid();

            // Service ko bola history lao
            var history = await _consentService.GetHistoryAsync(dataPrincipalId, organizationId);

            // Jo bhi list aayi, usko 200 OK ke sath frontend ko bhej diya
            return Ok(history);
        }
    }
}