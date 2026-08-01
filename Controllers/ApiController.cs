using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using talentacquisition_jobplacement_mvc.Data;

namespace talentacquisition_jobplacement_mvc.Controllers
{
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiController(ApplicationDbContext context)
        {
            _context = context;
        }

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

            // Simple aggregated data
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
                    // You can add real aggregation later
                    AggregatedValue = "Sample aggregation"
                })
            };

            return Ok(result);
        }
    }
}