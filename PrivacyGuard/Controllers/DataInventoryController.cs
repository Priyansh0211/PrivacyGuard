
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Services;
using System.Security.Claims;

namespace PrivacyGuard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Bina valid JWT token ke API access nahi - 401 dega

    public class DataInventoryController : ControllerBase
    {
        private readonly IDataInventoryService _service;

        public DataInventoryController(IDataInventoryService service)
        {
            _service = service;
        }

        // Custom helper - JWT token ke andar se (claims se) OrganizationId nikalta hai taaki user sirf apni company ka data dekh sake

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
            // Agar token me organization id nahi mili (gadbad hai), to request ko reject kar dega (403 Forbidden)
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
        
        [HttpPost("Create")]
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
        public async Task<IActionResult> Update(Guid id,[FromBody] UpdateDataInventoryDto request)
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
