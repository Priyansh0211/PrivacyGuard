using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using System;
using System.Threading.Tasks;

namespace PrivacyGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // JWT token required hoga
    public class ConsentController : ControllerBase
    {
        private readonly IConsentService _consentService;

        public ConsentController(IConsentService consentService)
        {
            _consentService = consentService;
        }

        // NAYA ENDPOINT: UI Dashboard Grid aur Filters ke liye
        [HttpGet("records")]
        public async Task<IActionResult> GetConsentRecords(
            [FromQuery] string? reference,
            [FromQuery] string? status,
            [FromQuery] string? purpose,
            [FromQuery] string? channel,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var records = await _consentService.GetFilteredRecordsAsync(
                Guid.Parse(orgIdClaim), reference, status, purpose, channel, fromDate, toDate);

            return Ok(records);
        }

        [HttpGet("purposes")]
        public async Task<IActionResult> GetPurposes()
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var purposes = await _consentService.GetPurposesAsync(Guid.Parse(orgIdClaim));
            return Ok(purposes);
        }

        [HttpPost("record")]
        public async Task<IActionResult> RecordConsent([FromBody] CreateConsentRequestDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var result = await _consentService.RecordConsentAsync(request, Guid.Parse(orgIdClaim));
            if (!result.IsSuccess) return BadRequest(new { Error = result.Message });

            return Ok(new { Message = result.Message, ReceiptId = result.ReceiptId });
        }

        [HttpPost("withdraw/{consentRecordId}")]
        public async Task<IActionResult> WithdrawConsent(Guid consentRecordId)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var success = await _consentService.WithdrawConsentAsync(consentRecordId, Guid.Parse(orgIdClaim));
            if (!success) return NotFound("Consent record not found.");

            return Ok(new { Message = "Consent withdrawn successfully. New version generated." });
        }

        [HttpGet("latest/{dataPrincipalId}")]
        public async Task<IActionResult> GetLatestConsent(Guid dataPrincipalId)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var consent = await _consentService.GetLatestConsentAsync(dataPrincipalId, Guid.Parse(orgIdClaim));
            if (consent == null) return NotFound("No records found.");

            return Ok(consent);
        }

        [HttpGet("history/{dataPrincipalId}")]
        public async Task<IActionResult> GetHistory(Guid dataPrincipalId)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var history = await _consentService.GetHistoryAsync(dataPrincipalId, Guid.Parse(orgIdClaim));
            return Ok(history);
        }

        [HttpPost("add-purpose")]
        public async Task<IActionResult> AddPurpose([FromBody] CreatePurposeDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var purpose = await _consentService.CreatePurposeAsync(request, Guid.Parse(orgIdClaim));
            return Ok(new { Message = "Complex Purpose added successfully", PurposeId = purpose.ConsentPurposeId });
        }

        // ==========================================
        // DATA CATEGORIES APIs
        // ==========================================
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            // Service method call karna (Service aur Repo me method add kar lena)
            var categories = await _consentService.GetCategoriesAsync(Guid.Parse(orgIdClaim));
            return Ok(categories);
        }

        [HttpPost("categories")]
        public async Task<IActionResult> AddCategory([FromBody] CreateDataCategoryDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var category = await _consentService.AddCategoryAsync(request, Guid.Parse(orgIdClaim));
            return Ok(new { Message = "Data Category added successfully", CategoryId = category.DataCategoryId });
        }

        // ==========================================
        // DATA PROCESSORS APIs
        // ==========================================
        [HttpGet("processors")]
        public async Task<IActionResult> GetProcessors()
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var processors = await _consentService.GetProcessorsAsync(Guid.Parse(orgIdClaim));
            return Ok(processors);
        }

        [HttpPost("processors")]
        public async Task<IActionResult> AddProcessor([FromBody] CreateDataProcessorDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var processor = await _consentService.AddProcessorAsync(request, Guid.Parse(orgIdClaim));
            return Ok(new { Message = "Data Processor added successfully", ProcessorId = processor.DataProcessorId });
        }
    }
}