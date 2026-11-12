using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Filters; 
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using System;
using System.Threading.Tasks;

namespace PrivacyGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsentController : ControllerBase
    {
        private readonly IConsentService _consentService;

        public ConsentController(IConsentService consentService)
        {
            _consentService = consentService;
        }

        // ================= GET APIs (View Permission) =================

        [HttpGet("records")]
        [HasPermission("Consent.View")]
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

        [HttpGet("record/{id}")]
        [HasPermission("Consent.View")]
        public async Task<IActionResult> GetConsentRecordById(Guid id)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var consentRecord = await _consentService.GetConsentRecordByIdAsync(id, Guid.Parse(orgIdClaim));
            if (consentRecord == null) return NotFound("Consent record not found.");

            return Ok(consentRecord);
        }

        [HttpGet("purposes")]
        [HasPermission("Consent.View")]
        public async Task<IActionResult> GetPurposes()
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var purposes = await _consentService.GetPurposesAsync(Guid.Parse(orgIdClaim));
            return Ok(purposes);
        }

        [HttpGet("latest/{dataPrincipalId}")]
        [HasPermission("Consent.View")]
        public async Task<IActionResult> GetLatestConsent(Guid dataPrincipalId)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var consent = await _consentService.GetLatestConsentAsync(dataPrincipalId, Guid.Parse(orgIdClaim));
            if (consent == null) return NotFound("No records found.");

            return Ok(consent);
        }

        [HttpGet("history/{dataPrincipalId}")]
        [HasPermission("Consent.View")]
        public async Task<IActionResult> GetHistory(Guid dataPrincipalId)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var history = await _consentService.GetHistoryAsync(dataPrincipalId, Guid.Parse(orgIdClaim));
            return Ok(history);
        }

        [HttpGet("categories")]
        [HasPermission("Consent.View")]
        public async Task<IActionResult> GetCategories()
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var categories = await _consentService.GetCategoriesAsync(Guid.Parse(orgIdClaim));
            return Ok(categories);
        }

        [HttpGet("processors")]
        [HasPermission("Consent.View")]
        public async Task<IActionResult> GetProcessors()
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var processors = await _consentService.GetProcessorsAsync(Guid.Parse(orgIdClaim));
            return Ok(processors);
        }

        [HttpGet("purposes/{id}/history")]
        [HasPermission("Consent.View")]
        public async Task<IActionResult> GetPurposeHistory(Guid id)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var history = await _consentService.GetPurposeHistoryAsync(id, Guid.Parse(orgIdClaim));
            return Ok(history);
        }

        // ================= POST & PUT APIs (Manage Permission) =================

        [HttpPost("record")]
        [HasPermission("Consent.Manage")] // UPDATE: Post ke liye manage permission
        public async Task<IActionResult> RecordConsent([FromBody] CreateConsentRequestDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var result = await _consentService.RecordConsentAsync(request, Guid.Parse(orgIdClaim));
            if (!result.IsSuccess) return BadRequest(new { Error = result.Message });

            return Ok(new { Message = result.Message, ReceiptId = result.ReceiptId });
        }

        [HttpPost("withdraw/{consentRecordId}")]
        [HasPermission("Consent.Manage")] // UPDATE
        public async Task<IActionResult> WithdrawConsent(Guid consentRecordId)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var success = await _consentService.WithdrawConsentAsync(consentRecordId, Guid.Parse(orgIdClaim));
            if (!success) return NotFound("Consent record not found.");

            return Ok(new { Message = "Consent withdrawn successfully. New version generated." });
        }

        [HttpPost("add-purpose")]
        [HasPermission("Consent.Manage")] // UPDATE
        public async Task<IActionResult> AddPurpose([FromBody] CreatePurposeDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var purpose = await _consentService.CreatePurposeAsync(request, Guid.Parse(orgIdClaim));
            return Ok(new { Message = "Complex Purpose added successfully", PurposeId = purpose.ConsentPurposeId });
        }

        [HttpPost("categories")]
        [HasPermission("Consent.Manage")] // UPDATE
        public async Task<IActionResult> AddCategory([FromBody] CreateDataCategoryDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var category = await _consentService.AddCategoryAsync(request, Guid.Parse(orgIdClaim));
            return Ok(new { Message = "Data Category added successfully", CategoryId = category.DataCategoryId });
        }

        [HttpPost("processors")]
        [HasPermission("Consent.Manage")] // UPDATE
        public async Task<IActionResult> AddProcessor([FromBody] CreateDataProcessorDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var processor = await _consentService.AddProcessorAsync(request, Guid.Parse(orgIdClaim));
            return Ok(new { Message = "Data Processor added successfully", ProcessorId = processor.DataProcessorId });
        }

        [HttpPut("purposes/{id}")]
        [HasPermission("Consent.Manage")] // UPDATE
        public async Task<IActionResult> EditPurpose(Guid id, [FromBody] CreatePurposeDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var updatedPurpose = await _consentService.EditPurposeAsync(id, request, Guid.Parse(orgIdClaim));
            if (updatedPurpose == null) return NotFound("Purpose not found.");

            return Ok(new { Message = "Purpose updated successfully (new version created)", PurposeId = updatedPurpose.ConsentPurposeId });
        }

        [HttpPut("categories/{id}")]
        [HasPermission("Consent.Manage")] // UPDATE
        public async Task<IActionResult> EditCategory(Guid id, [FromBody] CreateDataCategoryDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var category = await _consentService.EditCategoryAsync(id, request, Guid.Parse(orgIdClaim));
            if (category == null) return NotFound("Category not found.");

            return Ok(category);
        }

        [HttpPut("processors/{id}")]
        [HasPermission("Consent.Manage")] // UPDATE
        public async Task<IActionResult> EditProcessor(Guid id, [FromBody] CreateDataProcessorDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var processor = await _consentService.EditProcessorAsync(id, request, Guid.Parse(orgIdClaim));
            if (processor == null) return NotFound("Processor not found.");

            return Ok(processor);
        }
    }
}