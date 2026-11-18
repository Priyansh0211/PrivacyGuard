using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Services;

namespace PrivacyGuard.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize] // Using your standard JWT Auth
    public class RulesAndActsController : ControllerBase
    {
        private readonly IRuleAndActService _service;

        public RulesAndActsController(IRuleAndActService service)
        {
            _service = service;
        }

        [HttpGet("organization/{orgId:guid}")]
        public async Task<IActionResult> GetRules(Guid orgId)
        {
            var rules = await _service.GetRulesAsync(orgId);
            return Ok(new { success = true, data = rules });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetRuleById(Guid id)
        {
            var rule = await _service.GetRuleByIdAsync(id);
            if (rule == null) return NotFound(new { success = false, message = "Rule not found" });

            return Ok(new { success = true, data = rule });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRule([FromBody] CreateRuleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdRule = await _service.CreateRuleAsync(dto);
            return CreatedAtAction(nameof(GetRuleById), new { id = createdRule.Id }, new { success = true, data = createdRule });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteRule(Guid id)
        {
            var deleted = await _service.DeleteRuleAsync(id);
            if (!deleted) return NotFound(new { success = false, message = "Rule not found" });

            return Ok(new { success = true, message = "Rule deleted successfully" });
        }
    }
} 