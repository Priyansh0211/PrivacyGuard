using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PrivacyGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NoticeTemplateController : ControllerBase
    {
        private readonly INoticeTemplateService _templateService;
        private readonly IConsentService _purposeService;

        public NoticeTemplateController(INoticeTemplateService templateService , IConsentService purposeService)
        {
            _templateService = templateService;
            _purposeService = purposeService;
        }

        private (bool isValid, Guid orgId) GetOrgIdClaim()
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (Guid.TryParse(orgIdStr, out Guid orgId)) return (true, orgId);
            return (false, Guid.Empty);
        }

        

        [HttpGet("all")]
        public async Task<IActionResult> GetAllTemplates()
        {
            var claim = GetOrgIdClaim();
            if (!claim.isValid) return Unauthorized();
            return Ok(await _templateService.GetAllTemplatesAsync(claim.orgId));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTemplateDetails(Guid id)
        {
            var claim = GetOrgIdClaim();
            if (!claim.isValid) return Unauthorized();

            var template = await _templateService.GetTemplateByIdAsync(id, claim.orgId);
            if (template == null) return NotFound(new { message = "Template not found." });

            return Ok(template);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateDto dto)
        {
            var claim = GetOrgIdClaim();
            if (!claim.isValid) return Unauthorized();

            var templateId = await _templateService.CreateTemplateAsync(dto, claim.orgId);
            return Ok(new { message = "Notice Template created successfully.", templateId = templateId });
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] UpdateTemplateDto dto)
        {
            var claim = GetOrgIdClaim();
            if (!claim.isValid) return Unauthorized();

            var success = await _templateService.UpdateTemplateAsync(id, claim.orgId, dto);
            if (!success) return NotFound(new { message = "Template not found or you don't have permission to edit it." });

            return Ok(new { message = "Notice Template updated successfully." });
        }

       
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplate(Guid id)
        {
            var claim = GetOrgIdClaim();
            if (!claim.isValid) return Unauthorized();

            var success = await _templateService.DeleteTemplateAsync(id, claim.orgId);
            if (!success) return NotFound(new { message = "Template not found or you don't have permission to delete it." });

            return Ok(new { message = "Notice Template deleted successfully." });
        }

        [HttpGet("checkbox-list")]
        public async Task<IActionResult> GetPurposesForCheckboxes()
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (!Guid.TryParse(orgIdStr, out Guid orgId)) return Unauthorized();

            var purposes = await _purposeService.GetPurposesForCheckboxesAsync(orgId);
            return Ok(purposes);
        }
    }

}