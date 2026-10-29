using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Services;

namespace PrivacyGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsumerController : ControllerBase
    {
        private readonly IConsumerService _consumerService;

        public ConsumerController(IConsumerService consumerService)
        {
            _consumerService = consumerService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddConsumer([FromBody] CreateConsumerDto request)
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim)) return Unauthorized();

            if (string.IsNullOrEmpty(request.Mobile) && string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("At least Mobile or Email is required.");
            }

            var consumer = await _consumerService.CreateSingleConsumerAsync(request, Guid.Parse(orgIdClaim));

            return Ok(new
            {
                Message = "Consumer added successfully",
                ReferenceId = consumer.ReferenceId,
                ConsumerId = consumer.ConsumerId
            });
        }
    }
}