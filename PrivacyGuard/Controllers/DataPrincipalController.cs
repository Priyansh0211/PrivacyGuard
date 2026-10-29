using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using System;
using System.Threading.Tasks;

namespace PrivacyGuard.Controllers
{
    // Yahan maine explicitly 'dataprincipal' route de diya hai taaki URL /api/dataprincipal/add hi rahe
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

        [HttpPost("add")]
        public async Task<IActionResult> AddDataPrincipal([FromBody] DataPrincipalDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            if (string.IsNullOrEmpty(request.Mobile) && string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("At least Mobile or Email is required.");
            }

            // Naya method call kiya gaya hai
            var dataPrincipal = await _dataPrincipalService.CreateSingleDataPrincipalAsync(request, Guid.Parse(orgIdClaim));

            return Ok(new
            {
                Message = "Data Principal added successfully",
                ReferenceId = dataPrincipal.ReferenceId,
                DataPrincipalId = dataPrincipal.DataPrincipalId // Update property
            });
        }
    }
}