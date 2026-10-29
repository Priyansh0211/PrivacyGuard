using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Services;

namespace PrivacyGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ImportController : ControllerBase
    {
        private readonly IBulkImportService _bulkImportService;

        public ImportController(IBulkImportService bulkImportService)
        {
            _bulkImportService = bulkImportService;
        }

        // POST API: CSV upload ya paste karne ke liye
        [HttpPost("stage")]
        public async Task<IActionResult> StageImport([FromForm] BulkImportRequestDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(orgIdClaim))
            {
                return Unauthorized("User or Organization details missing in token.");
            }

            if (!request.IsValid())
            {
                return BadRequest(new { Error = "Please provide either a CSV file or paste raw CSV text." });
            }

            var userId = Guid.Parse(userIdClaim);
            var orgId = Guid.Parse(orgIdClaim);

            var result = await _bulkImportService.StageImportBatchAsync(request, orgId, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage });
            }

            return Ok(new
            {
                Message = "Batch staged successfully for review.",
                BatchId = result.BatchId
            });
        }

        // GET API: UI ke dual filters (Kind aur Status) ke sath
        [HttpGet("batches")]
        public async Task<IActionResult> GetBatches([FromQuery] string? kind, [FromQuery] string? status)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            // Service method mein bhi 'kind' pass karna hoga
            var batches = await _bulkImportService.GetBatchesAsync(Guid.Parse(orgIdClaim), kind, status);
            return Ok(batches);
        }

        // GET API: Ek specific batch aur uski rows ki detail dekhne ke liye (Paginated)
        [HttpGet("batches/{id}")]
        public async Task<IActionResult> GetBatchDetails(Guid id, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var details = await _bulkImportService.GetBatchDetailsAsync(id, Guid.Parse(orgIdClaim), status, page, pageSize);

            if (details == null) return NotFound("Batch not found.");

            return Ok(details);
        }

        // POST API: Staged batch ko final execute karke database mein save karna
        [HttpPost("batches/{id}/execute")]
        public async Task<IActionResult> ExecuteBatch(Guid id)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var result = await _bulkImportService.ExecuteImportBatchAsync(id, Guid.Parse(orgIdClaim));

            if (!result.IsSuccess)
                return BadRequest(new { Error = result.Message });

            return Ok(new { Message = result.Message });
        }
    }
}