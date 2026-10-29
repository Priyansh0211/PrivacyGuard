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

        // POST API: CSV upload/paste karne ke liye
        [HttpPost("stage")]
        public async Task<IActionResult> StageImport([FromForm] BulkImportRequestDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(orgIdClaim))
            {
                return Unauthorized("User or Organization details missing in token.");
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

        // GET API: Saare uploaded batches ki list dekhne ke liye
        [HttpGet("batches")]
        public async Task<IActionResult> GetBatches([FromQuery] string? status)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var batches = await _bulkImportService.GetBatchesAsync(Guid.Parse(orgIdClaim), status);
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