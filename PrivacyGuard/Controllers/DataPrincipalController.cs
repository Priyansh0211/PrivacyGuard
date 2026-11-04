using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using System;
using System.Threading.Tasks;

namespace PrivacyGuard.Controllers
{
    [Route("api/dataprincipal")]
    [ApiController]
    [Authorize]
    public class DataPrincipalController : ControllerBase
    {
        private readonly IDataPrincipalService _dataPrincipalService;

        public DataPrincipalController(IDataPrincipalService dataPrincipalService)
        {
            _dataPrincipalService = dataPrincipalService;
        }

        // ==========================================
        // NAYA ENDPOINT: Paginated List for Dashboard Grid
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetDataPrincipals(
            [FromQuery] string search = "",
            [FromQuery] string classType = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var result = await _dataPrincipalService.GetDataPrincipalsListAsync(
                Guid.Parse(orgIdClaim), search, classType, page, pageSize);

            return Ok(result);
        }

        // ==========================================
        // ENDPOINT: Add Single Data Principal
        // ==========================================
        [HttpPost("add")]
        public async Task<IActionResult> AddDataPrincipal([FromBody] DataPrincipalDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            if (string.IsNullOrEmpty(request.Mobile) && string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("At least Mobile or Email is required.");
            }

            var dataPrincipal = await _dataPrincipalService.CreateSingleDataPrincipalAsync(request, Guid.Parse(orgIdClaim));

            return Ok(new
            {
                Message = "Data Principal added successfully",
                ReferenceId = dataPrincipal.ReferenceId,
                DataPrincipalId = dataPrincipal.DataPrincipalId
            });
        }

        // ==========================================
        // ENDPOINT: 360-Degree View Dashboard
        // ==========================================
        [HttpGet("{id}/360-view")]
        public async Task<IActionResult> Get360View(Guid id)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            var viewData = await _dataPrincipalService.Get360ViewAsync(id, Guid.Parse(orgIdClaim));

            if (viewData == null)
                return NotFound(new { Message = "Data Principal not found." });

            return Ok(viewData);
        }
    }
}