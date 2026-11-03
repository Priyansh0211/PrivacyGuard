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
    public class NoticeController : ControllerBase
    {
        private readonly INoticeService _noticeService;

        public NoticeController(INoticeService noticeService)
        {
            _noticeService = noticeService;
        }

        private (bool isValid, Guid orgId) GetOrgIdClaim()
        {
            var orgIdStr = User.FindFirstValue("OrganizationId");
            if (Guid.TryParse(orgIdStr, out Guid orgId)) return (true, orgId);
            return (false, Guid.Empty);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllNotices()
        {
            var claim = GetOrgIdClaim();
            if (!claim.isValid) return Unauthorized(new { message = "Invalid Organization Token" });

            var result = await _noticeService.GetAllNoticesAsync(claim.orgId);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateNotice([FromBody] CreateNoticeDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var claim = GetOrgIdClaim();
            if (!claim.isValid) return Unauthorized(new { message = "Invalid Organization Token" });

            var noticeId = await _noticeService.CreateNoticeAsync(dto, claim.orgId);
            return Ok(new { message = "Notice created successfully.", noticeId = noticeId });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNoticeDetails(Guid id)
        {
            var claim = GetOrgIdClaim();
            if (!claim.isValid) return Unauthorized(new { message = "Invalid Organization Token" });

            var notice = await _noticeService.GetNoticeDetailsAsync(id, claim.orgId);
            if (notice == null) return NotFound(new { message = "Notice not found." });

            return Ok(notice);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotice(Guid id, [FromBody] UpdateNoticeDto dto)
        {
            var claim = GetOrgIdClaim();
            if (!claim.isValid) return Unauthorized(new { message = "Invalid Organization Token" });

            var success = await _noticeService.UpdateNoticeAsync(id, dto, claim.orgId);
            if (!success) return NotFound(new { message = "Notice not found or you don't have access." });

            return Ok(new { message = "Notice updated successfully with Template and Purposes." });
        }

        [HttpPut("{id}/publish")]
        public async Task<IActionResult> PublishNotice(Guid id)
        {
            var claim = GetOrgIdClaim();
            if (!claim.isValid) return Unauthorized(new { message = "Invalid Organization Token" });

            try
            {
                var success = await _noticeService.PublishNoticeAsync(id, claim.orgId);
                if (!success) return BadRequest(new { message = "Notice not found or already published." });

                return Ok(new { message = "Notice published successfully and securely hashed." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/create-version")]
        public async Task<IActionResult> CreateNewVersion(Guid id)
        {
            var claim = GetOrgIdClaim();
            if (!claim.isValid) return Unauthorized(new { message = "Invalid Organization Token" });

            var newNoticeId = await _noticeService.CreateNewVersionAsync(id, claim.orgId);

            if (newNoticeId == null)
                return BadRequest(new { message = "Notice not found or it is not in 'Published' state yet." });

            return Ok(new
            {
                message = "New version created successfully. It is currently in Draft mode.",
                noticeId = newNoticeId
            });
        }
    }
}