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
        [HttpPost("grant")]
        public async Task<IActionResult> GrantConsent([FromBody] GrantConsentDto request)
        {
            // Token se ID nikalne ki koshish ki, fail hua toh 403 Forbid de diya
            if (!TryGetOrganizationId(out var organizationId))
                return Forbid();

            // Service ko request bhej di processing ke liye
            var result = await _consentService.GrantConsentAsync(request, organizationId);

            // 200 OK ke sath success message aur nayi Consent ID wapas ki
            return Ok(new { Message = "Consent granted successfully", ConsentId = result.ConsentRecordId });
        }

        // API Endpoint 2: Consent Withdraw (Wapas Lene) Ke Liye (PUT /api/consent/withdraw/{id})
        [HttpPut("withdraw/{consentId:guid}")]
        public async Task<IActionResult> WithdrawConsent(Guid consentId)
        {
            // Phir se token validation check kiya
            if (!TryGetOrganizationId(out var organizationId))
                return Forbid();

            // Service ko withdraw karne ka order diya, isme true/false aayega
            var success = await _consentService.WithdrawConsentAsync(consentId, organizationId);

            // Agar false aaya matlab record mila hi nahi, toh 404 Not Found bhej do
            if (!success)
                return NotFound(new { Message = "Consent record not found." });

            // Sab theek raha toh 200 OK success message de diya
            return Ok(new { Message = "Consent withdrawn successfully." });
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