using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using talentacquisition_jobplacement_mvc.Data;

namespace talentacquisition_jobplacement_mvc.Controllers.Api
{
    [Route("api/attributes")]
    [ApiController]
    public class AttributesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttributesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/attributes/suggest?q=term
        [HttpGet("suggest")]
        public async Task<IActionResult> Suggest(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(new string[0]);

            var query = q.Trim().ToLower();
            var results = await _context.AttributeDefinitions
                .Where(a => a.Name.ToLower().Contains(query) || (a.Description != null && a.Description.ToLower().Contains(query)))
                .OrderBy(a => a.Name)
                .Take(12)
                .Select(a => new { a.Id, a.Name, a.Category, a.Type })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/attributes/recent
        [HttpGet("recent")]
        public async Task<IActionResult> Recent()
        {
            var recent = await _context.PositionAttributes
                .Include(pa => pa.AttributeDefinition)
                .GroupBy(pa => pa.AttributeDefinitionId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.First().AttributeDefinition)
                .Take(8)
                .Select(a => new { a.Id, a.Name, a.Category, a.Type })
                .ToListAsync();

            return Ok(recent);
        }
    }
}
