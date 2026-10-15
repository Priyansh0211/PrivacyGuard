
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Services;
using System.Security.Claims;

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

        private bool TryGetOrganizationId(out Guid organizationId)
        {
            organizationId = Guid.Empty;

            var claim = User.FindFirst("OrganizationId");

            return claim != null &&
                   Guid.TryParse(claim.Value, out organizationId);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!TryGetOrganizationId(out var organizationId))
                return Forbid();

            var result = await _service.GetAllAsync(organizationId);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (!TryGetOrganizationId(out var organizationId))
                return Forbid();

            var result = await _service.GetByIdAsync(id, organizationId);

            if (result == null)
                return NotFound(new { Message = "Inventory record not found." });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateDataInventoryDto request)
        {
            if (!TryGetOrganizationId(out var organizationId))
                return Forbid();

            var result = await _service.CreateAsync(request, organizationId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.DataInventoryId },
                result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateDataInventoryDto request)
        {
            if (!TryGetOrganizationId(out var organizationId))
                return Forbid();

            var updated = await _service.UpdateAsync(
                id, request, organizationId);

            if (!updated)
                return NotFound(new { Message = "Inventory record not found." });

            return NoContent();
        }
    }
}
