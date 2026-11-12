using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Filters;
using PrivacyGuard.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PrivacyGuard.Controllers
{
    [Route("api/campaign-audience-groups")]
    [ApiController]
    [Authorize]
    public class CampaignAudienceGroupController : ControllerBase
    {
        private readonly ICampaignAudienceGroupService _service;

        public CampaignAudienceGroupController(ICampaignAudienceGroupService service)
        {
            _service = service;
        }

        [HttpPost]
        [HasPermission("Campaigns.Create")]
        public async Task<IActionResult> Create([FromBody] CreateCampaignAudienceGroupDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                var group = await _service.CreateAsync(organizationId, dto);
                return CreatedAtAction(nameof(GetById), new { id = group.GroupId }, group);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [HasPermission("Campaigns.View")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync(GetOrganizationId()));
        }

        [HttpGet("{id:guid}")]
        [HasPermission("Campaigns.View")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var group = await _service.GetByIdAsync(id, GetOrganizationId());
            return group == null ? NotFound(new { error = "Audience group not found." }) : Ok(group);
        }

        [HttpPut("{id:guid}")]
        [HasPermission("Campaigns.Create")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCampaignAudienceGroupDto dto)
        {
            try
            {
                var group = await _service.UpdateAsync(id, GetOrganizationId(), dto);
                return group == null ? NotFound(new { error = "Audience group not found." }) : Ok(group);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        [HasPermission("Campaigns.Create")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id, GetOrganizationId());
            return deleted ? NoContent() : NotFound(new { error = "Audience group not found." });
        }

        [HttpPost("{id:guid}/apply")]
        [HasPermission("Campaigns.Create")]
        public async Task<IActionResult> Apply(Guid id, [FromBody] ApplyCampaignAudienceGroupDto dto)
        {
            try
            {
                await _service.ApplyToCampaignAsync(id, dto.CampaignId, GetOrganizationId());
                return Ok(new { message = "Audience group applied to the campaign successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        private Guid GetOrganizationId()
        {
            var organizationId = User.FindFirstValue("OrganizationId");
            if (!Guid.TryParse(organizationId, out var parsedOrganizationId))
                throw new UnauthorizedAccessException();
            return parsedOrganizationId;
        }
    }
}
