using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using talentacquisition_jobplacement_mvc.Data;
using talentacquisition_jobplacement_mvc.Models;

namespace talentacquisition_jobplacement_mvc.Controllers
{
    public class CVsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CVsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Apply to Position (Candidate)
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Create(int positionId)
        {
            var position = await _context.Positions
                .Include(p => p.PositionAttributes)
                .ThenInclude(pa => pa.AttributeDefinition)
                .FirstOrDefaultAsync(p => p.Id == positionId);

            if (position == null) return NotFound();

            var cv = new CV
            {
                PositionId = positionId,
                Position = position
            };

            return View(cv);
        }

        // POST: Save CV
        [HttpPost]
        [Authorize(Roles = "Candidate,Administrator")]
        public async Task<IActionResult> Create(CV cv, Dictionary<int, string> attributeValues)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            cv.UserId = userId;
            cv.CreatedAt = DateTime.UtcNow;

            // Save attribute values as JSON
            cv.AttributeValues = JsonSerializer.Serialize(attributeValues);

            _context.CVs.Add(cv);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Profiles");
        }

        // GET: All Submitted CVs (Recruiter View)
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> Index()
        {
            var cvs = await _context.CVs
                .Include(cv => cv.Position)
                .Include(cv => cv.User)
                .OrderByDescending(cv => cv.CreatedAt)
                .ToListAsync();

            return View(cvs);
        }

        // GET: CV Details (Recruiter)
        [Authorize(Roles = "Recruiter,Administrator")]
        public async Task<IActionResult> Details(int id)
        {
            var cv = await _context.CVs
                .Include(cv => cv.Position)
                .ThenInclude(p => p.PositionAttributes)
                .ThenInclude(pa => pa.AttributeDefinition)
                .Include(cv => cv.User)
                .FirstOrDefaultAsync(cv => cv.Id == id);

            if (cv == null) return NotFound();

            // Deserialize attribute values
            ViewBag.AttributeValues = string.IsNullOrEmpty(cv.AttributeValues)
                ? new Dictionary<int, string>()
                : JsonSerializer.Deserialize<Dictionary<int, string>>(cv.AttributeValues);

            return View(cv);
        }
    }
}