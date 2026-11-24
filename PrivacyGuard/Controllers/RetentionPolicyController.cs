using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Filters;
using PrivacyGuard.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace PrivacyGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RetentionPolicyController : ControllerBase
    {
        private readonly IRetentionPolicyService _retentionPolicyService;

        public RetentionPolicyController(IRetentionPolicyService retentionPolicyService)
        {
            _retentionPolicyService = retentionPolicyService;
        }

        private Guid GetOrganizationId()
        {
            var orgIdString = User.FindFirst("OrganizationId")?.Value;
            return Guid.TryParse(orgIdString, out var orgId) ? orgId : Guid.Empty;
        }

        [HttpGet]
        [HasPermission("RetentionPolicy.View")]
        public async Task<IActionResult> GetAllPolicies()
        {
            var orgId = GetOrganizationId();
            if (orgId == Guid.Empty) return Unauthorized("Organization context missing.");

            var policies = await _retentionPolicyService.GetAllPoliciesAsync(orgId);
            return Ok(policies);
        }

        [HttpGet("{id}")]
        [HasPermission("RetentionPolicy.View")]
        public async Task<IActionResult> GetPolicyById(Guid id)
        {
            var orgId = GetOrganizationId();
            if (orgId == Guid.Empty) return Unauthorized("Organization context missing.");

            var policy = await _retentionPolicyService.GetPolicyByIdAsync(id, orgId);
            if (policy == null) return NotFound(new { Message = $"Policy with ID {id} not found." });

            return Ok(policy);
        }

        [HttpPost]
        [HasPermission("RetentionPolicy.Manage")]
        public async Task<IActionResult> CreatePolicy([FromBody] CreateRetentionPolicyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var orgId = GetOrganizationId();
            if (orgId == Guid.Empty) return Unauthorized("Organization context missing.");

            var createdPolicy = await _retentionPolicyService.CreatePolicyAsync(dto, orgId);
            return CreatedAtAction(nameof(GetPolicyById), new { id = createdPolicy.PolicyId }, createdPolicy);
        }

        [HttpPut("{id}")]
        [HasPermission("RetentionPolicy.Manage")]
        public async Task<IActionResult> UpdatePolicy(Guid id, [FromBody] UpdateRetentionPolicyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var orgId = GetOrganizationId();
            if (orgId == Guid.Empty) return Unauthorized("Organization context missing.");

            var updatedPolicy = await _retentionPolicyService.UpdatePolicyAsync(id, dto, orgId);
            if (updatedPolicy == null) return NotFound(new { Message = "Policy not found or you don't have access." });

            return Ok(updatedPolicy);
        }

        [HttpDelete("{id}")]
        [HasPermission("RetentionPolicy.Manage")]
        public async Task<IActionResult> DeletePolicy(Guid id)
        {
            var orgId = GetOrganizationId();
            if (orgId == Guid.Empty) return Unauthorized("Organization context missing.");

            var success = await _retentionPolicyService.DeletePolicyAsync(id, orgId);
            if (!success) return NotFound(new { Message = "Policy not found." });

            return NoContent();
        }

        [HttpGet("metrics")]
        [HasPermission("RetentionPolicy.View")]
        public async Task<IActionResult> GetMetrics()
        {
            var orgId = GetOrganizationId();
            if (orgId == Guid.Empty) return Unauthorized("Organization context missing.");

            var metrics = await _retentionPolicyService.GetDashboardMetricsAsync(orgId);
            return Ok(metrics);
        }

        [HttpGet("queue")]
        [HasPermission("RetentionPolicy.View")]
        public async Task<IActionResult> GetErasureQueue()
        {
            var orgId = GetOrganizationId();
            if (orgId == Guid.Empty) return Unauthorized("Organization context missing.");

            var queueList = await _retentionPolicyService.GetErasureQueueAsync(orgId);
            return Ok(queueList);
        }

        // Manual trigger endpoint for Admin ("Run now")
        [HttpPost("run")]
        [HasPermission("RetentionPolicy.Manage")]
        public async Task<IActionResult> RunRetentionJobNow()
        {
            var orgId = GetOrganizationId();
            if (orgId == Guid.Empty) return Unauthorized("Organization context missing.");

            var processedCount = await _retentionPolicyService.ExecuteErasureJobForOrganizationAsync(orgId);
            return Ok(new
            {
                Message = "Retention and erasure job executed.",
                ProcessedRecords = processedCount,
                ExecutedAtUtc = DateTime.UtcNow
            });
        }
    }
}