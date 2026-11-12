using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Filters;
using PrivacyGuard.Interfaces.Services;
using System;
using System.Threading.Tasks;
// using PrivacyGuard.Filters; // Apne project ke hisaab se HasPermission ka namespace uncomment kar lena

namespace PrivacyGuard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DataInventoryController : ControllerBase
    {
        private readonly IDataInventoryService _service;

        public DataInventoryController(IDataInventoryService service)
        {
            _service = service;
        }

        // Helper: Token se Org ID nikalne ke liye
        private Guid GetOrganizationId()
        {
            var claim = User.FindFirst("OrganizationId");
            return claim != null && Guid.TryParse(claim.Value, out var orgId) ? orgId : Guid.Empty;
        }

        [HttpGet]
        [HasPermission("DataInventory.View")] // Sirf View permission
        public async Task<IActionResult> GetAll()
        {
            var orgId = GetOrganizationId();
            if (orgId == Guid.Empty) return Unauthorized("Organization context missing.");

            var result = await _service.GetAllAsync(orgId);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [HasPermission("DataInventory.View")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var orgId = GetOrganizationId();
            var result = await _service.GetByIdAsync(id, orgId);

            if (result == null) return NotFound(new { Message = "Inventory record not found." });

            return Ok(result);
        }

        [HttpPost("Create")]
        [HasPermission("DataInventory.Manage")] // Manage permission for Creating
        public async Task<IActionResult> Create([FromBody] CreateDataInventoryDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var orgId = GetOrganizationId();
            if (orgId == Guid.Empty) return Unauthorized("Organization context missing.");

            var result = await _service.CreateAsync(request, orgId);

            return CreatedAtAction(nameof(GetById), new { id = result.DataInventoryId }, result);
        }

        [HttpPut("{id:guid}")]
        [HasPermission("DataInventory.Manage")] // Manage permission for Updating
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDataInventoryDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var orgId = GetOrganizationId();
            var updated = await _service.UpdateAsync(id, request, orgId);

            if (!updated) return NotFound(new { Message = "Inventory record not found." });

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [HasPermission("DataInventory.Manage")] // Delete method add kiya gaya hai
        public async Task<IActionResult> Delete(Guid id)
        {
            var orgId = GetOrganizationId();
            var deleted = await _service.DeleteAsync(id, orgId);

            if (!deleted) return NotFound(new { Message = "Inventory record not found." });

            return NoContent();
        }
    }
}