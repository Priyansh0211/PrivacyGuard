using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Filters;
using PrivacyGuard.Interfaces.Services;
using System;
using System.Threading.Tasks;
// Is namespace ko apne project ke hisaab se adjust kar lena jahan HasPermission attribute rakha hai
// using PrivacyGuard.Filters; 

namespace PrivacyGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Pura controller secure kar diya, bina login koi access nahi kar payega
    public class RetentionPolicyController : ControllerBase
    {
        private readonly IRetentionPolicyService _retentionPolicyService;

        // Dependency Injection ke through service ko call kiya
        public RetentionPolicyController(IRetentionPolicyService retentionPolicyService)
        {
            _retentionPolicyService = retentionPolicyService;
        }

        // ==========================================
        // HELPER METHOD: JWT Token se Organization ID nikalne ke liye
        // ==========================================
        private Guid GetOrganizationId()
        {
            // Tumhare authentication setup ke hisaab se token/claims se OrgId nikal rahe hain
            var orgIdString = User.FindFirst("OrganizationId")?.Value;
            return Guid.TryParse(orgIdString, out var orgId) ? orgId : Guid.Empty;
        }

        // 1. GET: api/RetentionPolicy - Saari policies list karne ke liye
        [HttpGet]
        [HasPermission("RetentionPolicy.View")] // Sirf un roles ko allow karega jinke paas View permission hai
        public async Task<IActionResult> GetAllPolicies()
        {
            var orgId = GetOrganizationId();
            if (orgId == Guid.Empty) return Unauthorized("Organization context missing.");

            var policies = await _retentionPolicyService.GetAllPoliciesAsync(orgId);
            return Ok(policies);
        }

        // 2. GET: api/RetentionPolicy/{id} - Kisi ek policy ki details dekhne ke liye
        [HttpGet("{id}")]
        [HasPermission("RetentionPolicy.View")]
        public async Task<IActionResult> GetPolicyById(Guid id)
        {
            var orgId = GetOrganizationId();
            var policy = await _retentionPolicyService.GetPolicyByIdAsync(id, orgId);

            if (policy == null) return NotFound(new { Message = $"Policy with ID {id} not found." });

            return Ok(policy);
        }

        // 3. POST: api/RetentionPolicy - Nayi policy banane ke liye
        [HttpPost]
        [HasPermission("RetentionPolicy.Manage")] // Sirf Create permission wale log naya rule bana sakte hain
        public async Task<IActionResult> CreatePolicy([FromBody] CreateRetentionPolicyDto dto)
        {
            // Agar DTO me lagaye gaye Required/Range validations fail hote hain
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var orgId = GetOrganizationId();
            if (orgId == Guid.Empty) return Unauthorized("Organization context missing.");

            var createdPolicy = await _retentionPolicyService.CreatePolicyAsync(dto, orgId);

            // REST standard: Create hone ke baad 201 Created status aur record ka naya link bhejo
            return CreatedAtAction(nameof(GetPolicyById), new { id = createdPolicy.PolicyId }, createdPolicy);
        }

        // 4. PUT: api/RetentionPolicy/{id} - Existing policy update karne ke liye
        [HttpPut("{id}")]
        [HasPermission("RetentionPolicy.Manage")]
        public async Task<IActionResult> UpdatePolicy(Guid id, [FromBody] UpdateRetentionPolicyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var orgId = GetOrganizationId();
            var updatedPolicy = await _retentionPolicyService.UpdatePolicyAsync(id, dto, orgId);

            if (updatedPolicy == null) return NotFound(new { Message = "Policy not found or you don't have access." });

            return Ok(updatedPolicy); // 200 OK ke sath updated data return kiya
        }

        // 5. DELETE: api/RetentionPolicy/{id} - Policy delete karne ke liye
        [HttpDelete("{id}")]
        [HasPermission("RetentionPolicy.Manage")]
        public async Task<IActionResult> DeletePolicy(Guid id)
        {
            var orgId = GetOrganizationId();
            var success = await _retentionPolicyService.DeletePolicyAsync(id, orgId);

            if (!success) return NotFound(new { Message = "Policy not found." });

            // Successfully delete hone par 204 No Content status bhejte hain
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
    }
}