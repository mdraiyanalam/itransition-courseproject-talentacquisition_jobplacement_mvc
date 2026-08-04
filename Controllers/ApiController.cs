using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using talentacquisition_jobplacement_mvc.Data;
using talentacquisition_jobplacement_mvc.Models;
using talentacquisition_jobplacement_mvc.Models.ViewModels;

namespace talentacquisition_jobplacement_mvc.Controllers
{
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public ApiController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ===== Existing endpoint (for Odoo Import) =====
        [HttpGet("positions/{id}/stats")]
        public async Task<IActionResult> GetPositionStats(int id, [FromQuery] string token)
        {
            var position = await _context.Positions
                .Include(p => p.PositionAttributes)
                    .ThenInclude(pa => pa.AttributeDefinition)
                .Include(p => p.CVs)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (position == null)
                return NotFound("Position not found");

            if (string.IsNullOrEmpty(position.ApiToken) || position.ApiToken != token)
                return Unauthorized("Invalid API token");

            var result = new
            {
                PositionId = position.Id,
                Title = position.Title,
                Company = position.Company,
                TotalApplications = position.CVs?.Count ?? 0,
                Attributes = position.PositionAttributes.Select(pa => new
                {
                    Name = pa.AttributeDefinition.Name,
                    Type = pa.AttributeDefinition.Type,
                    AggregatedValue = "Sample aggregation"
                })
            };

            return Ok(result);
        }

        // ===== New endpoint (for Odoo Export) =====
        [HttpPost("positions")]
        public async Task<IActionResult> CreatePositionFromOdoo(
            [FromHeader(Name = "X-Api-Key")] string apiKey,
            [FromBody] OdooPositionDto model)
        {
            var expectedKey = _config["ApiSettings:ExportApiKey"];

            if (string.IsNullOrEmpty(apiKey) || apiKey != expectedKey)
                return Unauthorized(new { message = "Invalid API Key" });

            if (string.IsNullOrWhiteSpace(model.Title))
                return BadRequest(new { message = "Title is required" });

            var position = new Position
            {
                Title = model.Title.Trim(),
                Description = model.Description ?? string.Empty,
                Company = model.Company ?? string.Empty,
                ProjectTags = model.ProjectTags,
                CreatedAt = DateTime.UtcNow
            };

            _context.Positions.Add(position);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = position.Id,
                message = "Position created successfully in TalentHub"
            });
        }
    }
}